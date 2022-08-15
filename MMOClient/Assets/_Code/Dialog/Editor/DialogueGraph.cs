using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;


namespace BV{

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;
    private string _fileName = "New Dialog";

    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();

        //MiniMap
        // GenerateMiniMap();
        GenerateBlackBoard();
    }

    private void ConstructGraphView()
    {
       _graphView = new DialogueGraphView
        {
            name = "Dialogue Graph"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView); 
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name: ");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        var nodeCreateAnswerButton = new Button(() => {_graphView.CreateNode(1); });
        nodeCreateAnswerButton.text = "Create Answer";
        toolbar.Add(nodeCreateAnswerButton);

        var nodeCreatePhraseButton = new Button(() => {_graphView.CreateNode(2); });
        nodeCreatePhraseButton.text = "Create Phrase";
        toolbar.Add(nodeCreatePhraseButton);

        var nodeCreateOutPointButton = new Button(() => {_graphView.CreateNode(3); });
        nodeCreateOutPointButton.text = "Create Out Point";
        toolbar.Add(nodeCreateOutPointButton);

        var nodeCreateSelectedButton = new Button(() => {_graphView.CreateNode(4); });
        nodeCreateSelectedButton.text = "Create Selected";
        toolbar.Add(nodeCreateSelectedButton);

        toolbar.Add(new Button(() => RequestDataOperation("create")){text = "Creat New Data"});
        toolbar.Add(new Button(() => RequestDataOperation("save")){text = "Save Data"});
        toolbar.Add(new Button(() => RequestDataOperation("load")){text = "Load Data"});

        rootVisualElement.Add(toolbar);
    }

    private void GenerateMiniMap(){
        var miniMap = new MiniMap {anchored = true}; 

        //give 10px offset from left side
        // var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
        // miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
        
        miniMap.SetPosition(new Rect(100, 20, 200, 140));
        _graphView.Add(miniMap);
    }

    private void GenerateBlackBoard(){
        var blackboard = new Blackboard(_graphView);

        blackboard.addItemRequested = _blackboard => {_graphView.CreateNode(2);};

        blackboard.editTextRequested = (blackboard1, elem, newValue) =>
        {
            var oldPropertyName = ((BlackboardField)elem).text;
            if(String.IsNullOrEmpty(newValue)){
                EditorUtility.DisplayDialog("Eror", "The name field cannot be empty", "OK");
                return;
            }

             if(!_graphView.Phrases.Any(x => x.nodeText == oldPropertyName))
            {
                ((BlackboardField)elem).text = newValue;
                return;
            }

            if(_graphView.Phrases.Any(x => x.nodeText == newValue))
            {
                EditorUtility.DisplayDialog("Eror", "This name is alredy exists", "OK");
                return;
            }

            var propertyIndex = _graphView.Phrases.FindIndex(x => x.nodeText == oldPropertyName);
            _graphView.Phrases[propertyIndex].nodeText = newValue;
            ((BlackboardField)elem).text = newValue;

            var PhraseNodes = _graphView.nodes.ToList().Where(x => x is PhraseNode).Cast<PhraseNode>().ToList();
            foreach(var phraseNode in PhraseNodes){
                if(phraseNode.PhraseName == oldPropertyName){
                    phraseNode.title = newValue;
                    phraseNode.PhraseName = newValue;
                    return;
                }
            }
        };

        blackboard.scrollable = true;  
        blackboard.SetPosition(new Rect(2, 30, 300, 630));//this.maxSize.y
        _graphView.Add(blackboard);
        _graphView.Blackboard = blackboard;
    }

    private void RequestDataOperation(string operation)
    {
        if(string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter valid file name", "OK");
            return;
        }

        var saveUtility = SaveUtility.GetInstance(_graphView);

        if(operation == "create"){
            saveUtility.CreateGraph();
            _fileName = "New Dialog";
        }
        else if(operation == "save")
            saveUtility.SaveGraph(_fileName);
        else if(operation == "load")
            saveUtility.LoadGraph(_fileName);
    }


    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
}
}