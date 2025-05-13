using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class ChestInteraction : MonoBehaviour, IInteractable
    {
        public string chestId = "1";

        public string GetDescription()
        {
            return "Відкрити судук";
        }

        public void Interact(GameObject player)
        {
            MenuManager.singleton.OpenChest(chestId);
        }
    }
}
