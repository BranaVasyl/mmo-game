using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BV
{
    public class InventoryItem : MonoBehaviour
    {
        public ItemData itemData;

        public int HEIGHT
        {
            get
            {
                if (rotated == false)
                {
                    return itemData.height;
                }

                return itemData.width;
            }
        }

        public int WIDTH
        {
            get
            {
                if (rotated == false)
                {
                    return itemData.width;
                }

                return itemData.height;
            }
        }

        public int onGridPositionX;
        public int onGridPositionY;

        public bool rotated = false;

        internal void Set(ItemData itemData)
        {
            this.itemData = itemData;

            GetComponent<Image>().sprite = itemData.itemIcon;

            Vector2 size = new Vector2();
            size.x = itemData.width * ItemGrid.boundTileSizeWidth;
            size.y = itemData.height * ItemGrid.boundTileSizeHeight;
            GetComponent<RectTransform>().sizeDelta = size;
        }

        internal void Rotate()
        {
            rotated = !rotated;

            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.rotation = Quaternion.Euler(0, 0, rotated == true ? 90f : 0f);
        }

        public ItemType GetItemType() {
            return itemData.itemType;
        }
    }
}
