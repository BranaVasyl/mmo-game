using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace BV
{
    public class DialogueGraphView : GraphView
    {
        private DialogueGraphView _targetGraphView;

        public readonly Vector2 DefaultNodeSize = new Vector2(150, 200);

        public Blackboard Blackboard;
        public List<DialogPhrase> Phrases = new List<DialogPhrase>();
        public DialogueGraphView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(GenerateEntryPointNode());
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private Port GeneratePort(AnswerNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); //Arbitrary type
        }

        private EntryPointNode GenerateEntryPointNode()
        {
            var node = new EntryPointNode
            {
                title = "START",
                GUID = System.Guid.NewGuid().ToString(),
            };

            var generatedPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            generatedPort.portName = "Next";
            node.outputContainer.Add(generatedPort);

            node.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            node.capabilities &= ~Capabilities.Movable;
            node.capabilities &= Capabilities.Deletable;

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));
            return node;
        }

        public void CreateNode(int id)
        {
            switch (id)
            {
                case 1:
                    AddElement(CreateAnserNode("New Dialog"));
                    break;
                case 2:
                    AddElement(CreatePhraseNode("New Phrase", new List<DialogPhraseItem>()));
                    break;
                case 3:
                    AddElement(CreateOutPointNode(""));//{command}#{id}&{option}
                    break;
                case 4:
                    AddElement(CreateSelectedNode(""));
                    break;
                case 5:
                    AddElement(CreateTradeNode());
                    break;
                default:
                    EditorUtility.DisplayDialog("Eror", "The name field cannot be empty", "OK");
                    break;
            }
        }

        public AnswerNode CreateAnserNode(string nodeName)
        {
            var localDialogName = nodeName;
            while (nodes.ToList().Where(x => x is AnswerNode).Cast<AnswerNode>().ToList().Any(x => x.AnswerName == localDialogName))
                localDialogName = $"{localDialogName}(1)";

            var answerNode = new AnswerNode
            {
                title = localDialogName,
                AnswerName = localDialogName,
                GUID = System.Guid.NewGuid().ToString()
            };

            var inputPort = GeneratePort(answerNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            answerNode.inputContainer.Add(inputPort);

            answerNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            var button = new Button(() => { AddChoicePort(answerNode); });
            button.text = "New Choice";
            answerNode.titleContainer.Add(button);

            var textField = new TextField(string.Empty);
            textField.RegisterValueChangedCallback(evt =>
            {
                answerNode.AnswerName = evt.newValue;
                answerNode.title = evt.newValue;
            });
            textField.SetValueWithoutNotify(answerNode.title);
            answerNode.mainContainer.Add(textField);

            answerNode.RefreshExpandedState();
            answerNode.RefreshPorts();
            answerNode.SetPosition(new Rect(Vector2.zero, DefaultNodeSize));

            return answerNode;
        }

        public PhraseNode CreatePhraseNode(string nodeName, List<DialogPhraseItem> phraseItems)
        {
            var localPhraseName = nodeName;
            while (nodes.ToList().Where(x => x is PhraseNode).Cast<PhraseNode>().ToList().Any(x => x.PhraseName == localPhraseName))
                localPhraseName = $"{localPhraseName}(1)";

            var phraseNode = new PhraseNode
            {
                title = localPhraseName,
                PhraseName = localPhraseName,
                GUID = System.Guid.NewGuid().ToString()
            };

            phraseNode.capabilities &= ~Capabilities.Deletable;

            var inputPort = phraseNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "Input";
            phraseNode.inputContainer.Add(inputPort);

            phraseNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            var outputPort = phraseNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Output";
            phraseNode.outputContainer.Add(outputPort);

            AddPropertyToBlackBoard(phraseNode.PhraseName, phraseItems);

            phraseNode.RefreshExpandedState();
            phraseNode.RefreshPorts();
            phraseNode.SetPosition(new Rect(Vector2.zero, DefaultNodeSize));

            return phraseNode;
        }

        public OutPointNode CreateOutPointNode(string parameter)
        {
            var outputNode = new OutPointNode
            {
                title = "OUT",
                GUID = System.Guid.NewGuid().ToString(),
                parameter = parameter
            };

            var generatedPort = outputNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            generatedPort.portName = "Out";
            outputNode.inputContainer.Add(generatedPort);

            var textField = new TextField
            {
                name = string.Empty,
                value = parameter
            };
            textField.RegisterValueChangedCallback(evt => { outputNode.parameter = evt.newValue; });
            outputNode.contentContainer.Add(textField);

            outputNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            outputNode.RefreshExpandedState();
            outputNode.RefreshPorts();
            outputNode.SetPosition(new Rect(Vector2.zero, DefaultNodeSize));

            return outputNode;
        }

        public SelectedNode CreateSelectedNode(string parameter)
        {
            var selectedNnode = new SelectedNode
            {
                title = "SELECTED",
                GUID = System.Guid.NewGuid().ToString(),
                parameter = parameter
            };

            var inputPort = selectedNnode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "Input";
            selectedNnode.inputContainer.Add(inputPort);

            var outputPortTrue = selectedNnode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPortTrue.portName = "True";
            selectedNnode.outputContainer.Add(outputPortTrue);

            var outputPortFalse = selectedNnode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPortFalse.portName = "False";
            selectedNnode.outputContainer.Add(outputPortFalse);

            var textField = new TextField
            {
                name = string.Empty,
                value = parameter
            };
            textField.RegisterValueChangedCallback(evt => { selectedNnode.parameter = evt.newValue; });
            selectedNnode.contentContainer.Add(textField);

            selectedNnode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            selectedNnode.RefreshExpandedState();
            selectedNnode.RefreshPorts();
            selectedNnode.SetPosition(new Rect(Vector2.zero, DefaultNodeSize));

            return selectedNnode;
        }

        public TradeNode CreateTradeNode()
        {
            var tradeNode = new TradeNode
            {
                title = "TRADE",
                GUID = System.Guid.NewGuid().ToString(),
            };

            var inputPort = tradeNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "Input";
            tradeNode.inputContainer.Add(inputPort);

            var outputPort = tradeNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Output";
            tradeNode.outputContainer.Add(outputPort);

            tradeNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            tradeNode.RefreshExpandedState();
            tradeNode.RefreshPorts();
            tradeNode.SetPosition(new Rect(Vector2.zero, DefaultNodeSize));

            return tradeNode;
        }

        public void AddChoicePort(AnswerNode answerNode, string overriddenPortName = "")
        {
            var generatedPort = GeneratePort(answerNode, Direction.Output);

            var oldLabel1 = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(oldLabel1);

            var outputPortCount = answerNode.outputContainer.Query("connector").ToList().Count;

            var choicePortName = string.IsNullOrEmpty(overriddenPortName)
                ? $"Choice {outputPortCount + 1}"
                : overriddenPortName;

            var textField = new TextField
            {
                name = string.Empty,
                value = choicePortName
            };
            textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(new Label(" "));
            generatedPort.contentContainer.Add(textField);

            var deleteButton = new Button(() => RemovePort(answerNode, generatedPort))
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);

            generatedPort.portName = choicePortName;
            answerNode.outputContainer.Add(generatedPort);
            answerNode.RefreshPorts();
            answerNode.RefreshExpandedState();
        }

        private void RemovePort(AnswerNode answerNode, Port generatedPort)
        {
            var targetEdge = edges.ToList().Where(x =>
                x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }

            answerNode.outputContainer.Remove(generatedPort);
            answerNode.RefreshPorts();
            answerNode.RefreshExpandedState();
        }

        public void AddPropertyToBlackBoard(String nodeName, List<DialogPhraseItem> phraseItems)
        {
            var phrase = new DialogPhrase();
            phrase.nodeText = nodeName;
            phrase.phraseItems = new List<DialogPhraseItem>();

            var container = new VisualElement();
            container.ToggleInClassList("container");
            var item = new VisualElement();

            var blackboardField = new BlackboardField { text = nodeName };

            var createButton = new Button(() => { AddPhraseSentence(item, phrase); }) { text = "+" };
            createButton.ToggleInClassList("positiveButton");
            blackboardField.contentContainer.Add(createButton);

            var removeButton = new Button(() => RemovePhrase(container, nodeName)) { text = "-" };
            removeButton.ToggleInClassList("negativeButton");
            blackboardField.contentContainer.Add(removeButton);

            container.Add(blackboardField);

            var sa = new BlackboardRow(blackboardField, item);
            container.Add(sa);
            Blackboard.Add(container);
            Phrases.Add(phrase);

            if (phraseItems.Count() > 0)
                for (var i = 0; i < phraseItems.Count(); i++)
                {
                    AddPhraseSentence(item, phrase, phraseItems[i].id, phraseItems[i].speaker, phraseItems[i].showTime, phraseItems[i].sentence);
                }

            container.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        }

        public void AddPhraseSentence(VisualElement item, DialogPhrase phrase, String id = "",
                                        String Speaker = "Speaker", float ShowTime = 4, String Sentence = "Sentence")
        {
            var phraseItem = new DialogPhraseItem
            {
                id = Guid.NewGuid().ToString(),
                speaker = Speaker,
                showTime = ShowTime,
                sentence = Sentence
            };
            var itemElement = new VisualElement();


            var speaker = new TextField { value = Speaker };
            var showTime = new TextField { value = ShowTime.ToString() };
            var sentence = new TextField { value = Sentence };

            speaker.RegisterValueChangedCallback(evt =>
            {
                phraseItem.speaker = evt.newValue;
            });
            sentence.RegisterValueChangedCallback(evt =>
            {
                phraseItem.sentence = evt.newValue;
            });
            showTime.RegisterValueChangedCallback(evt =>
            {
                phraseItem.showTime = (float)Convert.ToDouble(evt.newValue);
            });

            var upItem = new VisualElement();
            upItem.Add(speaker);

            var navButton = new VisualElement();
            navButton.ToggleInClassList("nawButton");
            navButton.Add(showTime);

            var positiveButton = new Button(() => { AddPhraseSentence(item, phrase, phraseItem.id); }) { text = "+" };
            positiveButton.ToggleInClassList("positiveButton");
            navButton.Add(positiveButton);

            var negativeButton = new Button(() => { item.Remove(itemElement); phrase.phraseItems.Remove(phraseItem); }) { text = "-" };
            negativeButton.ToggleInClassList("negativeButton");
            navButton.Add(negativeButton);

            upItem.Add(navButton);
            itemElement.Add(upItem);
            itemElement.Add(sentence);

            upItem.ToggleInClassList("itemContainer");
            speaker.ToggleInClassList("speakerLabel");
            showTime.ToggleInClassList("showTimeLabel");
            sentence.ToggleInClassList("sentenceLabel");

            upItem.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            var elemIndex = phrase.phraseItems.FindIndex(x => x.id == id);
            if (elemIndex != -1)
            {
                item.Insert(elemIndex + 1, itemElement);
                phrase.phraseItems.Insert(elemIndex + 1, phraseItem);
                return;
            }

            if (id.Length > 0)
            {
                item.Add(itemElement);
                phrase.phraseItems.Add(phraseItem);
                return;
            }

            item.Insert(0, itemElement);
            phrase.phraseItems.Insert(0, phraseItem);
        }

        public void RemovePhrase(VisualElement container, String nodeName)
        {
            Blackboard.Remove(container);

            var PhraseNodes = nodes.ToList().Where(x => x is PhraseNode).Cast<PhraseNode>().ToList();
            foreach (var phraseNode in PhraseNodes)
            {
                if (phraseNode.PhraseName == nodeName)
                {

                    edges.ToList().Where(x => x.input.node == phraseNode || x.output.node == phraseNode).ToList()
                        .ForEach(edge => RemoveElement(edge));

                    RemoveElement(phraseNode);
                }
            }
        }
    }
}