using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class GetQuestFromDasshboard : MonoBehaviour, IInteractable
    {
        public string questName;

        public List<QuestEvent> questEvents;

        public string GetDescription()
        {
            return "Взяти квест : " + questName;
        }

        public void Interact(GameObject player)
        {
            for (int i = 0; i < questEvents.Count; i++)
            {
                questEvents[i].TriggerEvent();
            }

            Destroy(this.gameObject);
        }
    }
}
