﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BV
{
    public class SaveUtility
    {
        private DialogueGraphView _targetGrpaphView;

        private List<Edge> Edges => _targetGrpaphView.edges.ToList();

        private List<BaseNode> AllNodes => _targetGrpaphView.nodes.ToList().Where(x => x is BaseNode).Cast<BaseNode>().ToList();
        private EntryPointNode EntryPoint => _targetGrpaphView.nodes.ToList().Where(x => x is EntryPointNode).First() as EntryPointNode;
        private List<AnswerNode> AnswerNodes => _targetGrpaphView.nodes.ToList().Where(x => x is AnswerNode).Cast<AnswerNode>().ToList();
        private List<PhraseNode> PhraseNodes => _targetGrpaphView.nodes.ToList().Where(x => x is PhraseNode).Cast<PhraseNode>().ToList();
        private List<DialogPhrase> Phrases => _targetGrpaphView.Phrases;
        private List<OutPointNode> OutPoints => _targetGrpaphView.nodes.ToList().Where(x => x is OutPointNode).Cast<OutPointNode>().ToList();
        private List<SelectedNode> SelectedNodes => _targetGrpaphView.nodes.ToList().Where(x => x is SelectedNode).Cast<SelectedNode>().ToList();
        private List<TradeNode> TradeNodes => _targetGrpaphView.nodes.ToList().Where(x => x is TradeNode).Cast<TradeNode>().ToList();

        private Dialog _dialog = new Dialog();

        public static SaveUtility GetInstance(DialogueGraphView targetGrpaphView)
        {
            return new SaveUtility
            {
                _targetGrpaphView = targetGrpaphView
            };
        }

        public void SaveGraph(string fileName)
        {
            List<DialogAnswer> answers = new List<DialogAnswer>();
            List<DialogPhrase> phrases = new List<DialogPhrase>();
            List<DialogOutput> outputs = new List<DialogOutput>();
            List<DialogSelected> selecteds = new List<DialogSelected>();
            List<DialogTrade> trades = new List<DialogTrade>();

            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
            foreach (var answerNode in AnswerNodes)
            {
                DialogAnswer newAnswer = new DialogAnswer
                {
                    idAnswer = answerNode.GUID,
                    nodeText = answerNode.AnswerName,
                    nodePosition = answerNode.GetPosition().position,
                    answerItems = new List<DialogAnswerItem>()
                };

                for (var i = 0; i < connectedPorts.Length; i++)
                {
                    if (connectedPorts[i].output.node is AnswerNode)
                    {
                        BaseNode inputNode = connectedPorts[i].input.node as BaseNode;
                        BaseNode outputNode = connectedPorts[i].output.node as BaseNode;

                        if (inputNode == null || outputNode == null)
                        {
                            continue;
                        }

                        if (answerNode.GUID == outputNode.GUID)
                        {
                            DialogAnswerItem answerItem = new DialogAnswerItem
                            {
                                nextPhrase = inputNode.GUID,
                                sentence = connectedPorts[i].output.portName
                            };
                            newAnswer.answerItems.Add(answerItem);
                            continue;
                        }
                    }
                }

                answers.Add(newAnswer);
            }

            foreach (var phraseNode in PhraseNodes)
            {
                DialogPhrase newPhrase = new DialogPhrase
                {
                    idPhrase = phraseNode.GUID,
                    nodeText = phraseNode.PhraseName,
                    nodePosition = phraseNode.GetPosition().position,
                    phraseItems = new List<DialogPhraseItem>()
                };

                newPhrase.phraseItems = Phrases.Where(x => x.nodeText == phraseNode.PhraseName).First().phraseItems;

                for (var i = 0; i < connectedPorts.Length; i++)
                {
                    BaseNode inputNode = connectedPorts[i].input.node as BaseNode;
                    BaseNode outputNode = connectedPorts[i].output.node as BaseNode;

                    if (inputNode == null || outputNode == null)
                    {
                        continue;
                    }

                    if (phraseNode.GUID == outputNode.GUID)
                    {
                        newPhrase.nextAnswer = inputNode.GUID;
                        continue;
                    }
                }

                phrases.Add(newPhrase);
            }

            foreach (var selectedNode in SelectedNodes)
            {
                DialogSelected node = new DialogSelected
                {
                    nodeId = selectedNode.GUID,
                    nodePosition = selectedNode.GetPosition().position,
                    parameter = selectedNode.parameter
                };

                for (var i = 0; i < connectedPorts.Length; i++)
                {
                    if (connectedPorts[i].output.node is SelectedNode)
                    {
                        BaseNode inputNode = connectedPorts[i].input.node as BaseNode;
                        BaseNode outputNode = connectedPorts[i].output.node as BaseNode;

                        if (inputNode == null || outputNode == null)
                        {
                            continue;
                        }

                        if (selectedNode.GUID == outputNode.GUID)
                        {
                            if (connectedPorts[i].output.portName == "True")
                                node.nextElementPositive = inputNode.GUID;
                            else
                                node.nextElementNegative = inputNode.GUID;
                            continue;
                        }
                    }
                }

                selecteds.Add(node);
            }

            foreach (var tradeNode in TradeNodes)
            {
                DialogTrade node = new DialogTrade
                {
                    nodeId = tradeNode.GUID,
                    nodePosition = tradeNode.GetPosition().position,
                };

                for (var i = 0; i < connectedPorts.Length; i++)
                {
                    if (connectedPorts[i].output.node is TradeNode)
                    {
                        BaseNode inputNode = connectedPorts[i].input.node as BaseNode;
                        BaseNode outputNode = connectedPorts[i].output.node as BaseNode;

                        if (inputNode == null || outputNode == null)
                        {
                            continue;
                        }

                        if (tradeNode.GUID == outputNode.GUID)
                        {
                            node.nextItem = inputNode.GUID;
                            continue;
                        }
                    }
                }

                trades.Add(node);
            }

            foreach (var outputNode in OutPoints)
            {
                DialogOutput node = new DialogOutput
                {
                    nodeId = outputNode.GUID,
                    nodePosition = outputNode.GetPosition().position,
                    parameter = outputNode.parameter
                };

                outputs.Add(node);
            }

            for (var i = 0; i < connectedPorts.Length; i++)
            {
                if (connectedPorts[i].output.node is EntryPointNode)
                {
                    var outputNode = connectedPorts[i].output.node as EntryPointNode;
                    if (EntryPoint.GUID == outputNode.GUID)
                    {
                        string inputNodeId = null;

                        if (connectedPorts[i].input.node is PhraseNode)
                        {
                            inputNodeId = (connectedPorts[i].input.node as PhraseNode).GUID;
                        }

                        if (connectedPorts[i].input.node is AnswerNode)
                        {
                            inputNodeId = (connectedPorts[i].input.node as AnswerNode).GUID;
                        }

                        if (connectedPorts[i].input.node is SelectedNode)
                        {
                            inputNodeId = (connectedPorts[i].input.node as SelectedNode).GUID;
                        }

                        if (inputNodeId != null)
                        {
                            _dialog.startPhrase = inputNodeId;
                            break;
                        }
                    }
                }
            }

            _dialog.dialogName = fileName;
            _dialog.entryPointGUID = EntryPoint.GUID;

            _dialog.allPhrase = phrases.ToArray();
            _dialog.allAnswer = answers.ToArray();
            _dialog.allOutputNode = outputs;
            _dialog.allSelectedNode = selecteds;
            _dialog.allTradeNode = trades;

            if (_dialog.allPhrase.Length == 0 && _dialog.allAnswer.Length == 0)
            {
                Debug.Log("No data to save");
                return;
            }

            string json = JsonUtility.ToJson(_dialog, true);
            File.WriteAllText(Application.dataPath + "/Resources/DialogText/" + fileName + ".json", json);

            Debug.Log("Сhanges saved successfully");
        }

        public void CreateGraph()
        {
            ClearGraph();
        }

        public void LoadGraph(string fileName)
        {
            string json = File.ReadAllText(Application.dataPath + "/Resources/DialogText/" + fileName + ".json");
            _dialog = JsonUtility.FromJson<Dialog>(json);

            ClearGraph();
            CreateNodes();
            ConnectNodes();

            Debug.Log("Data loaded successfully");
        }

        private void ConnectNodes()
        {
            for (var i = 0; i < AnswerNodes.Count; i++)
            {
                AnswerNodes[i].SetPosition(new Rect(
                    _dialog.allAnswer.First(x => x.idAnswer == AnswerNodes[i].GUID).nodePosition,
                    _targetGrpaphView.DefaultNodeSize
                ));

                var connections = _dialog.allAnswer.Where(x => x.idAnswer == AnswerNodes[i].GUID).First().answerItems.ToList();
                for (var j = 0; j < connections.Count; j++)
                {
                    var targetNodeGuid = connections[j].nextPhrase;
                    if (String.IsNullOrEmpty(targetNodeGuid))
                    {
                        continue;
                    }

                    Node targetNode = AllNodes.Find(x => x.GUID == targetNodeGuid);
                    LinkNodes(AnswerNodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);
                }
            }

            for (var i = 0; i < PhraseNodes.Count; i++)
            {
                PhraseNodes[i].SetPosition(new Rect(
                    _dialog.allPhrase.First(x => x.idPhrase == PhraseNodes[i].GUID).nodePosition,
                    _targetGrpaphView.DefaultNodeSize
                ));

                var connections = _dialog.allPhrase.Where(x => x.idPhrase == PhraseNodes[i].GUID).First();
                var targetNodeGuid = connections.nextAnswer;
                if (String.IsNullOrEmpty(targetNodeGuid))
                {
                    continue;
                }

                Node targetNode = AllNodes.Find(x => x.GUID == targetNodeGuid);
                LinkNodes(PhraseNodes[i].outputContainer[0].Q<Port>(), (Port)targetNode.inputContainer[0]);
            }

            for (var i = 0; i < OutPoints.Count; i++)
            {
                OutPoints[i].SetPosition(new Rect(
                    _dialog.allOutputNode.First(x => x.nodeId == OutPoints[i].GUID).nodePosition,
                    _targetGrpaphView.DefaultNodeSize
                ));
            }

            for (var i = 0; i < SelectedNodes.Count; i++)
            {
                SelectedNodes[i].SetPosition(new Rect(
                    _dialog.allSelectedNode.First(x => x.nodeId == SelectedNodes[i].GUID).nodePosition,
                    _targetGrpaphView.DefaultNodeSize
                ));

                var dialogSelected = _dialog.allSelectedNode.First(x => x.nodeId == SelectedNodes[i].GUID);
                if (dialogSelected.nextElementPositive.Length > 0)
                {
                    Node targetNode = AllNodes.Find(x => x.GUID == dialogSelected.nextElementPositive);
                    LinkNodes(SelectedNodes[i].outputContainer[0].Q<Port>(), (Port)targetNode.inputContainer[0]);
                }
                if (dialogSelected.nextElementNegative.Length > 0)
                {
                    Node targetNode = AllNodes.Find(x => x.GUID == dialogSelected.nextElementNegative);
                    LinkNodes(SelectedNodes[i].outputContainer[1].Q<Port>(), (Port)targetNode.inputContainer[0]);
                }
            }

            for (var i = 0; i < TradeNodes.Count; i++)
            {
                DialogTrade dilogNodeData = _dialog.allTradeNode.Find(x => x.nodeId == TradeNodes[i].GUID);
                if (dilogNodeData == null)
                {
                    continue;
                }

                TradeNodes[i].SetPosition(new Rect(dilogNodeData.nodePosition, _targetGrpaphView.DefaultNodeSize));
                if (String.IsNullOrEmpty(dilogNodeData.nextItem))
                {
                    continue;
                }

                Node targetNode = AllNodes.Find(x => x.GUID == dilogNodeData.nextItem);
                LinkNodes(TradeNodes[i].outputContainer[0].Q<Port>(), (Port)targetNode.inputContainer[0]);
            }

            if (!String.IsNullOrEmpty(_dialog.entryPointGUID))
            {
                var targetNodeGuid = _dialog.startPhrase;
                if (String.IsNullOrEmpty(targetNodeGuid))
                {
                    return;
                }

                Node targetNode = AllNodes.Find(x => x.GUID == targetNodeGuid);
                LinkNodes(EntryPoint.outputContainer[0].Q<Port>(), (Port)targetNode.inputContainer[0]);
            }
        }

        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);

            _targetGrpaphView.Add(tempEdge);
        }

        private void CreateNodes()
        {
            foreach (var answer in _dialog.allAnswer)
            {
                var tempNode = _targetGrpaphView.CreateAnserNode(answer.nodeText);
                tempNode.GUID = answer.idAnswer;
                _targetGrpaphView.AddElement(tempNode);

                answer.answerItems.ForEach(x => _targetGrpaphView.AddChoicePort(tempNode, x.sentence));
            }

            foreach (var phrase in _dialog.allPhrase)
            {
                var tempNode = _targetGrpaphView.CreatePhraseNode(phrase.nodeText, phrase.phraseItems);
                tempNode.GUID = phrase.idPhrase;
                _targetGrpaphView.AddElement(tempNode);
            }

            foreach (var output in _dialog.allOutputNode)
            {
                var tempNode = _targetGrpaphView.CreateOutPointNode(output.parameter);
                tempNode.GUID = output.nodeId;
                _targetGrpaphView.AddElement(tempNode);
            }

            foreach (var selected in _dialog.allSelectedNode)
            {
                var tempNode = _targetGrpaphView.CreateSelectedNode(selected.parameter);
                tempNode.GUID = selected.nodeId;
                _targetGrpaphView.AddElement(tempNode);
            }

            foreach (var trade in _dialog.allTradeNode)
            {
                var tempNode = _targetGrpaphView.CreateTradeNode();
                tempNode.GUID = trade.nodeId;
                _targetGrpaphView.AddElement(tempNode);
            }
        }

        private void ClearGraph()
        {
            EntryPoint.GUID = _dialog.entryPointGUID;
            if (Edges.Find(x => x.input.node == EntryPoint) != null)
                _targetGrpaphView.RemoveElement(Edges.Find(x => x.input.node == EntryPoint));

            foreach (var node in AllNodes)
            {
                if (node is EntryPointNode)
                {
                    continue;
                }

                Edges.Where(x => x.input.node == node).ToList()
                    .ForEach(edge => _targetGrpaphView.RemoveElement(edge));

                _targetGrpaphView.RemoveElement(node);
            }

            _targetGrpaphView.Blackboard.Clear();
            _targetGrpaphView.Phrases = new List<DialogPhrase>();
        }
    }
}
