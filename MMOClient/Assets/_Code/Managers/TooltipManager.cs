using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace BV
{
    public class TooltipManager : MonoBehaviour
    {
        public GameObject tooltipContainer;
        public Tooltip tooltip;

        public void Init()
        {
            tooltip.Init();
            tooltipContainer.SetActive(true);
            HideTooltip();
        }

        public void Show()
        {
            tooltipContainer.SetActive(true);
        }

        public void Hide()
        {
            tooltipContainer.SetActive(false);
        }

        public void ShowInventoryItemTooltip(InventoryItem item, float scaleFactor)
        {
            ItemData itemData = item.itemData;
            string subtitle = "";
            switch (itemData.type)
            {
                case ItemType.weapon:
                    subtitle = "Зброя";
                    break;
                case ItemType.quest:
                    subtitle = "Предмет для квесту";
                    break;
                case ItemType.alchemy:
                    subtitle = "Інгредієнт";
                    break;
                case ItemType.elixir:
                    subtitle = "Еліксир";
                    break;
                default:
                    subtitle = "Сміття";
                    break;
            }

            Vector2 cellPosition = item.GetComponent<RectTransform>().position;
            float offsetX = 0;
            float offsetY = 0;

            bool top = Screen.height / 2 <= cellPosition.y;
            bool right = Screen.width / 2 <= cellPosition.x;
            if (right)
            {
                if (top)
                {
                    offsetX = -(((item.WIDTH * 64) / 2) * scaleFactor);
                    offsetY = -(((item.HEIGHT * 64) / 2) * scaleFactor);
                }
                else
                {
                    offsetX = -(((item.WIDTH * 64) / 2) * scaleFactor);
                    offsetY = +(((item.HEIGHT * 64) / 2) * scaleFactor);
                }
            }
            else
            {
                if (top)
                {
                    offsetX = +(((item.WIDTH * 64) / 2) * scaleFactor);
                    offsetY = -(((item.HEIGHT * 64) / 2) * scaleFactor);
                }
                else
                {
                    offsetX = +(((item.WIDTH * 64) / 2) * scaleFactor);
                    offsetY = +(((item.HEIGHT * 64) / 2) * scaleFactor);
                }
            }

            TooltipData tooltipData = new TooltipData(
                itemData.name,
                subtitle,
                itemData.description,
                itemData.mass >= 0 ? itemData.mass.ToString() : "",
                itemData.price >= 0 ? itemData.price.ToString() : ""
            );
            tooltipData.cellPosition = cellPosition;
            tooltipData.offsetX = offsetX;
            tooltipData.offsetY = offsetY;
            tooltipData.flexWidth = false;

            ShowTooltip(tooltipData);
        }

        public void ShowTooltip(TooltipData data)
        {
            tooltip.SetData(data);
            tooltip.gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            tooltip.Clean();
            tooltip.gameObject.SetActive(false);
        }

        public static TooltipManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }

    [Serializable]
    public class TooltipData
    {
        public string title = "";
        public string subtitle = "";
        public string content = "";
        public string mass = "";
        public string price = "";

        public Vector2 cellPosition = new Vector2();
        public float offsetX = 0;
        public float offsetY = 0;

        public bool flexWidth = true; //if have only header contnet

        public TooltipData(string t = "", string sT = "", string c = "", string m = "", string p = "")
        {
            title = t;
            subtitle = sT;
            content = c;
            mass = m;
            price = p;
        }
    }
}
