using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class InventoryHiglight : MonoBehaviour
    {
        [SerializeField]
        RectTransform higlighter;
        public void Show(bool b)
        {
            higlighter.gameObject.SetActive(b);
        }

        public void SetSize(InventoryItem targetItem)
        {
            Vector2 size = new Vector2();
            size.x = targetItem.WIDTH * ItemGrid.tileSizeWidth;
            size.y = targetItem.HEIGHT * ItemGrid.tileSizeHeight;

            higlighter.sizeDelta = size;
        }

        public void SetPosition(ItemGrid targetGrid, InventoryItem targetItem)
        {
            Vector2 pos = targetGrid.CalculatePositionOnGrid(targetItem, targetItem.onGridPositionX, targetItem.onGridPositionY);

            higlighter.localPosition = pos;
        }

        public void SetParent(ItemGrid targetGrid)
        {
            if (targetGrid == null)
            {
                return;
            }

            higlighter.SetParent(targetGrid.GetComponent<RectTransform>());
            higlighter.SetAsFirstSibling();
        }

        public void SetPosition(ItemGrid targetGrid, InventoryItem targetItem, int posX, int posY)
        {
            Vector2 pos = targetGrid.CalculatePositionOnGrid(targetItem, posX, posY);

            higlighter.localPosition = pos;
        }
    }
}
