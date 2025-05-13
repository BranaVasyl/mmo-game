using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace BV
{
    [Serializable]
    public class QuestStageBase
    {
        [Header("Stage Base")]
        public string id;
        public string description;
        public bool active;
        public bool completed;

        public List<QuestEvent> startActions;
        public List<QuestEvent> completedActions;

        public bool IsActive()
        {
            return active;
        }

        public bool IsCompleted()
        {
            return completed;
        }

        public void OnStart()
        {
            if (IsCompleted())
            {
                return;
            }

            for (int i = 0; i < startActions.Count; i++)
            {
                startActions[i].TriggerEvent();
            }

            active = true;
        }

        public void OnComplete()
        {
            if (IsCompleted())
            {
                return;
            }

            for (int i = 0; i < completedActions.Count; i++)
            {
                completedActions[i].TriggerEvent();
            }

            active = false;
            completed = true;
        }
    }

    [Serializable]
    public class QuestEvent
    {
        public QuestEventType eventType;
        public string NPC_Id;
        public string dialogId;
        public string questId = "";
        public string stageId = "";

        public void TriggerEvent()
        {
            if (eventType == QuestEventType.updateDialog)
            {
                DialogManager dialogManager = DialogManager.singleton;
                dialogManager.UpdateDialogList(NPC_Id, dialogId);
                return;
            }

            if (eventType == QuestEventType.removeDialog)
            {
                Debug.Log("removeDialog dialog : " + dialogId + " NPC id : " + NPC_Id);
                return;
            }

            if (eventType == QuestEventType.updateQuest)
            {
                QuestManager questManager = QuestManager.singleton;
                questManager.SetActiveState(questId, stageId);
                return;
            }

            if (eventType == QuestEventType.completedQuest)
            {
                QuestManager questManager = QuestManager.singleton;
                questManager.SetCompleteState(questId, stageId);
                return;
            }
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(QuestEvent))]
    public class QuestEventDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty eventType = property.FindPropertyRelative("eventType");

            string eventTypeValue = eventType.enumNames[eventType.enumValueIndex];

            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            else
            {
                if (eventTypeValue == QuestEventType.updateDialog.ToString() || eventTypeValue == QuestEventType.removeDialog.ToString())
                {
                    return EditorGUIUtility.singleLineHeight * 4 + 6;
                }
                else if(eventTypeValue == QuestEventType.updateQuest.ToString() || eventTypeValue == QuestEventType.completedQuest.ToString() || 
                        eventTypeValue == QuestEventType.failedQuest.ToString())
                {
                    return EditorGUIUtility.singleLineHeight * 4 + 6;
                } else {
                    return EditorGUIUtility.singleLineHeight * 2 + 6;
                }
            }
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty eventType = property.FindPropertyRelative("eventType");
            string eventTypeValue = eventType.enumNames[eventType.enumValueIndex];

            Rect labelRect = new Rect(position.x, position.y, position.width, 16);
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, eventTypeValue);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                var eventTypeRect = new Rect(position.x, position.y + 18, position.width, 16);
                EditorGUI.PropertyField(eventTypeRect, eventType);


                if (eventTypeValue == QuestEventType.updateDialog.ToString() || eventTypeValue == QuestEventType.removeDialog.ToString())
                {
                    var nameRect = new Rect(position.x, position.y + 38, position.width, 16);
                    EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("NPC_Id"));

                    var ageRect = new Rect(position.x, position.y + 56, position.width, 16);
                    EditorGUI.PropertyField(ageRect, property.FindPropertyRelative("dialogId"));
                }
                else if(eventTypeValue == QuestEventType.updateQuest.ToString() || eventTypeValue == QuestEventType.completedQuest.ToString() || 
                        eventTypeValue == QuestEventType.failedQuest.ToString())
                {
                    var nameRect = new Rect(position.x, position.y + 38, position.width, 16);
                    EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("questId"));

                    var ageRect = new Rect(position.x, position.y + 56, position.width, 16);
                    EditorGUI.PropertyField(ageRect, property.FindPropertyRelative("stageId"));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
#endif

    public enum QuestEventType
    {
        updateDialog, removeDialog, updateQuest, completedQuest, failedQuest
    }
}
