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

        public void ShowEmptyEquipTolltip(ItemGrid grid, float scaleFactor)
        {
            string tooltipContent = "";
            switch (grid.gridId)
            {
                case "leftHandGrid":
                    tooltipContent = "Ліва рука";
                    break;
                case "rightHandGrid":
                    tooltipContent = "Права рука";
                    break;
                case "bowGrid":
                    tooltipContent = "Лук";
                    break;
                case "arrowGrid":
                    tooltipContent = "Стріли";
                    break;
                case "quickSpellGrid4":
                case "quickSpellGrid3":
                case "quickSpellGrid2":
                case "quickSpellGrid1":
                    tooltipContent = "Магія";
                    break;
                case "pocketsGrid4":
                case "pocketsGrid3":
                case "pocketsGrid2":
                case "pocketsGrid1":
                    tooltipContent = "Кармани";
                    break;
                case "bodyArmorGrid":
                    tooltipContent = "Броня";
                    break;
                case "handArmorGrid":
                    tooltipContent = "Рукавиці";
                    break;
                case "legsArmorGrid":
                    tooltipContent = "Штани";
                    break;
                case "shoesArmorGrid":
                    tooltipContent = "Взуття";
                    break;
                case "jewelryGrid4":
                case "jewelryGrid3":
                case "jewelryGrid2":
                case "jewelryGrid1":
                    tooltipContent = "Біжутерія";
                    break;
                case "dropItemGrid":
                    tooltipContent = "Викинути предмет";
                    break;
            }

            if (string.IsNullOrEmpty(tooltipContent))
            {
                return;
            }

            Vector2 cellPosition = grid.placeholder.GetComponent<RectTransform>().position;

            float itemWidth = grid.placeholder.GetComponent<RectTransform>().sizeDelta.x;
            float itemHeight = grid.placeholder.GetComponent<RectTransform>().sizeDelta.y;
            float offsetX = +((itemWidth / 2) * scaleFactor);
            float offsetY = -((itemHeight / 2) * scaleFactor);

            TooltipData tooltipData = new TooltipData();
            tooltipData.subtitle = tooltipContent;
            tooltipData.cellPosition = cellPosition;
            tooltipData.offsetX = offsetX;
            tooltipData.offsetY = offsetY;
            tooltipData.calculatePivot = false;

            ShowTooltip(tooltipData);
        }

        public void ShowTooltip(TooltipData data)
        {
            tooltip.SetData(data);
            tooltip.Open();
        }

        public void HideTooltip()
        {
            tooltip.Clean();
            tooltip.Close();
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
        public bool calculatePivot = true;

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
