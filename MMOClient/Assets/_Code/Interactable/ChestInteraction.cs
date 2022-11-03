using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class ChestInteraction : MonoBehaviour, IInteractable
    {
        public string GetDescription()
        {
            return "Відкрити судук";
        }

        public void Interact(GameObject player)
        {
            List<string> activatePanel = new List<string>();
            activatePanel.Add("chest");

            MenuManager.singleton.OpenMenu(activatePanel);
        }
    }
}
