using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace BV
{
    public class TooltipManager : MonoBehaviour
    {
        public GameObject tooltipContainer;
        public Tooltip tooltip;

        public void Init()
        {
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
}
