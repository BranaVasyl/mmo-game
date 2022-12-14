using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/Quest/Quest", order = 1)]
    public class Quest : ScriptableObject
    {
        [Header("Quest Base")]
        public string id;
        public Sprite questIcon;
        public string questName;
        public string description;
        public int level;
        public string locality;
        public int type;
        public bool active;
        public bool completed;

        public List<QuestStageBase> questStages;

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
}
