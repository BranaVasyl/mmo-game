using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace BV
{

    [System.Serializable]
    public class QuestOld
    {
        public string questName;
        public string questId;
        public int questLevel;
        public string questEnviropment;
        public int type;
        public bool questActive;

        public QuestPart[] questPart;

        public bool isQuestActive()
        {
            return questActive;
        }

        public void setActive()
        {
            questActive = true;
        }

        public void setPartCompleted(string id)
        {
            QuestPart part = Array.Find(questPart, i => i.partId == id);
            if (part != null)
            {
                part.setCompleted();
                if (!questActive)
                    setActive();
            }
        }

        public bool isPartCompleted(string id)
        {
            QuestPart part = Array.Find(questPart, i => i.partId == id);
            if (part != null)
                return part.isCompleted();
            else
                return false;
        }
    }
}
