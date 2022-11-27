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

        public void ShowInventoryItemTooltip(InventoryItem item)
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

            TooltipData tooltipData = new TooltipData(
                itemData.name,
                subtitle,
                itemData.description,
                itemData.mass >= 0 ? itemData.mass.ToString() : "",
                itemData.price >= 0 ? itemData.price.ToString() : "",
                item.GetComponent<RectTransform>().position,
                false
            );

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
        public string title;
        public string subtitle;
        public string content;
        public string mass;
        public string price;

        public Vector2 position;
        public bool flexWidth; //if have only header contnet

        public TooltipData(string t = "", string sT = "", string c = "", string m = "", string p = "", Vector2 pos = new Vector2(), bool fW = true)
        {
            title = t;
            subtitle = sT;
            content = c;
            mass = m;
            price = p;
            position = pos;
            flexWidth = fW;
        }
    }
}
