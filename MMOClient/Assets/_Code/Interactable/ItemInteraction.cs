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
            if (itemId == "random")
            {
                int selectedItemID = UnityEngine.Random.Range(0, ItemsManager.singleton.GetItemsCount());
                ItemData item = ItemsManager.singleton.GetItemByIndex(selectedItemID);

                GridManager.singleton.PickUpItem(item.id);
                return;
            }

            GridManager.singleton.PickUpItem(itemId);
        }
    }
}
