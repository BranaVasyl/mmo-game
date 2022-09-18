using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

namespace BV
{
    public class DialogManager : MonoBehaviour
    {
        public QuestManager questManager;
        public GameObject phraseUi;
        public GameObject answersUIField;
        public bool showSpeakerName;

        private string NPCName;
        private DialogPhrase[] allDialogPhrases;
        private DialogAnswer[] allDialogAnswers;
        private DialogSelectedOneElement[] allSelectedNode;
        private DialogOutput[] allOutputNode;

        private string nextElementId;
        private string currentAnsweId;

        private StateManager states;

        public void InitDialog(string path, StateManager st)
        {
            TextAsset targetFile = Resources.Load<TextAsset>(path);
            Dialog dialog = JsonUtility.FromJson<Dialog>(targetFile.text);

            ClearElement();
            states = st;

            NPCName = dialog.dialogName;
            allDialogPhrases = dialog.allPhrase;
            allDialogAnswers = dialog.allAnswer;
            allOutputNode = dialog.allOutputNode.ToArray();

            initDialogSelected(dialog.allSelectedNode);
            shearchNextElement(dialog.startPhrase);

            StartDialog();
        }

        public void StartDialog()
        {
            states.inDialog = true;
            Debug.Log("Stard dialog with " + NPCName);
        }

        void initDialogSelected(List<DialogSelected> dialogsSelected)
        {
            var dialogSelectedOneElements = new List<DialogSelectedOneElement>();

            foreach (var item in dialogsSelected)
            {
                bool parameterIsTrue = false;
                string[] command = item.parameter.Split(new char[] { '#' });

                if (command.Length > 1)
                {
                    switch (command[0])
                    {
                        case "q":
                            parameterIsTrue = questManager.getQuestOrPartComplated(command[1]);
                            break;
                        default:
                            parameterIsTrue = false;
                            Debug.Log("Unknow command" + command[0]);
                            break;
                    }
                }

                var dialogSelectedOneElement = new DialogSelectedOneElement
                {
                    nodeId = item.nodeId,
                    nextElementId = parameterIsTrue ? item.nextElementPositive : item.nextElementNegative
                };

                dialogSelectedOneElements.Add(dialogSelectedOneElement);
            }

            allSelectedNode = dialogSelectedOneElements.ToArray();
        }

        void shearchNextElement(string id)
        {
            DialogPhrase dialogPhrase = Array.Find(allDialogPhrases, i => i.idPhrase == id);
            DialogAnswer dialogAnswer = Array.Find(allDialogAnswers, i => i.idAnswer == id);
            DialogSelectedOneElement dialogSelected = Array.Find(allSelectedNode, i => i.nodeId == id);
            DialogOutput dialogsOutput = Array.Find(allOutputNode, i => i.nodeId == id);

            if (dialogPhrase != null)
                ShowPhrase(dialogPhrase);
            else if (dialogAnswer != null)
                ShowAnswer(dialogAnswer);
            else if (dialogSelected != null)
                shearchNextElement(dialogSelected.nextElementId);
            else
                EndDialog(dialogsOutput);
        }

        void ShowPhrase(DialogPhrase phrase)
        {
            Queue<DialogPhraseItem> PhraseItems = new Queue<DialogPhraseItem>();

            foreach (DialogPhraseItem phraseItem in phrase.phraseItems)
            {
                PhraseItems.Enqueue(phraseItem);
            }

            if (PhraseItems.Count > 0)
            {
                phraseUi.SetActive(true);
                nextElementId = phrase.nextAnswer;
                StartCoroutine(ShowNextPhrase(PhraseItems));
            }
            else if (phrase.nextAnswer.Length > 0)
                shearchNextElement(phrase.nextAnswer);
            else
                EndDialog();
        }

        IEnumerator ShowNextPhrase(Queue<DialogPhraseItem> PhraseItems)
        {
            DialogPhraseItem curPhraseItem = PhraseItems.Dequeue();

            phraseUi.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text =
            showSpeakerName ? curPhraseItem.speaker + " : " + curPhraseItem.sentence : curPhraseItem.sentence;

            yield return new WaitForSeconds(curPhraseItem.showTime);

            if (PhraseItems.Count == 0)
                shearchNextElement(nextElementId);
            else if (PhraseItems.Count > 0)
                StartCoroutine(ShowNextPhrase(PhraseItems));
        }

        private void ShowAnswer(DialogAnswer answer)
        {
            currentAnsweId = answer.idAnswer;
            GameObject answerContainer = answersUIField.transform.GetChild(1).gameObject;
            for (int i = 0; i <= answer.answerItems.Count - 1; i++)
            {
                answerContainer.transform.GetChild(i).gameObject.SetActive(true);
                TMP_Text buttonText = answerContainer.transform.GetChild(i).gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
                buttonText.text = i + 1 + ". " + answer.answerItems[i].sentence;
            }

            answersUIField.SetActive(true);
        }

        public void clickFromAnswer(int id)
        {
            DialogAnswer dialogAnswer = Array.Find(allDialogAnswers, i => i.idAnswer == currentAnsweId);

            if (dialogAnswer == null)
            {
                Debug.LogError("Not Found dialogAnswer");
                EndDialog();
                return;
            }

            answersUIField.SetActive(false);

            if (id < dialogAnswer.answerItems.Count)
            {
                ClearUIElement();
                nextElementId = dialogAnswer.answerItems[id].nextPhrase;
                shearchNextElement(nextElementId);
            }
            else
            {
                Debug.LogError("parameter id went beyond dialogAnswer.answerItems");
                EndDialog();
            }
        }

        public void EndDialog(DialogOutput dialogOutput = null)
        {
            if (dialogOutput != null)
            {
                string[] command = dialogOutput.parameter.Split(new char[] { '#' });

                if (command.Length > 1)
                {
                    switch (command[0])
                    {
                        case "q":
                            questManager.setQuestOrPartComplated(command[1]);
                            break;
                        default:
                            Debug.Log("Unknow command" + command[0]);
                            break;
                    }
                }
            }

            StopAllCoroutines();
            ClearElement();

            if (states != null)
            {
                states.inDialog = false;
                states = null;
            }
            Debug.Log("End dialog with" + NPCName);
        }

        void ClearElement()
        {
            allDialogPhrases = null;
            allDialogAnswers = null;
            allSelectedNode = null;
            currentAnsweId = "";
            nextElementId = "";
            NPCName = "";
            ClearUIElement();
        }

        void ClearUIElement()
        {
            phraseUi.SetActive(false);
            phraseUi.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "";
            answersUIField.SetActive(false);

            GameObject answerContainer = answersUIField.transform.GetChild(1).gameObject;
            for (int i = 0; i < answerContainer.transform.childCount; i++)
            {
                answerContainer.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public static DialogManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
