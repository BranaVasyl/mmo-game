using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BV
{
    [RequireComponent(typeof(ItemGrid))]
    public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        InventoryController inventoryController;
        ItemGrid itemGrid;

        private void Start()
        {
            inventoryController = InventoryController.singleton;
            itemGrid = GetComponent<ItemGrid>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
           inventoryController.SelectedItemGrid = itemGrid;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
           inventoryController.SelectedItemGrid = null;
        }
    }
}
