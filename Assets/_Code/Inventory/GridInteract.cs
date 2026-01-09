using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BV
{
    [RequireComponent(typeof(ItemGrid))]
    public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        GridManager gridManager;
        ItemGrid itemGrid;

        private void Start()
        {
            gridManager = GridManager.Instance;
            itemGrid = GetComponent<ItemGrid>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            gridManager.SelectedItemGrid = itemGrid;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            gridManager.Clean();
        }
    }
}
