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
        public string name;
        public string description;
        public int level;
        public string locality;
        public int type;
        public bool active;

        public List<QuestStageBase> questParts;
        
        public bool completed;

        public bool isQuestActive()
        {
            return false;
            // return questActive;
        }

        public void setActive()
        {
            // questActive = true;
        }

        public void setPartCompleted(string id)
        {
            // QuestPart part = Array.Find(questPart, i => i.partId == id);
            // if (part != null)
            // {
            //     part.setCompleted();
            //     if (!questActive)
            //         setActive();
            // }
        }

        public bool isPartCompleted(string id)
        {
            // QuestPart part = Array.Find(questPart, i => i.partId == id);
            // if (part != null)
            //     return part.isCompleted();
            // else
            //     return false;

            return false;
        }
    }
}
