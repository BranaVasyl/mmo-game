using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class ItemInteraction : MonoBehaviour, IInteractable
    {
        public string itemId = "apple";

        public string GetDescription()
        {
            return "Підібрати предмет";
        }

        public void Interact(GameObject player)
        {
            GridManager.singleton.PickUpItem(itemId);
        }
    }
}
