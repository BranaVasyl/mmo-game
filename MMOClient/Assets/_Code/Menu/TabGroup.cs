using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BV
{
    public class TabGroup : MonoBehaviour
    {
        public List<TabButton> tabButtons;
        public TabButton selectedTab;
        public List<GameObject> objectsToSwap;

        [SerializeField]
        Color32 iddleColor = new Color32(82, 66, 58, 255);
        [SerializeField]
        Color32 hoverColor = new Color32(106, 55, 29, 255);
        [SerializeField]
        Color32 activeColor = new Color32(255, 255, 225, 255);

        void Start()
        {

            OnTabSelected(tabButtons[0]);
        }

        public void Subscribe(TabButton button)
        {
            if (tabButtons == null)
            {
                tabButtons = new List<TabButton>();
            }

            tabButtons.Add(button);
        }

        public void OnTabEnter(TabButton button)
        {
            ResetTabs();
            if (selectedTab == null || button != selectedTab)
            {
                button.icon.color = hoverColor;
            }
        }

        public void OnTabExit(TabButton button)
        {
            ResetTabs();
        }

        public void OnTabSelected(TabButton button)
        {
            selectedTab = button;
            ResetTabs();
            button.icon.color = activeColor;
            int index = button.transform.GetSiblingIndex();
            for (int i = 0; i < objectsToSwap.Count; i++)
            {
                if (i == index)
                {
                    objectsToSwap[i].SetActive(true);
                }
                else
                {
                    objectsToSwap[i].SetActive(false);
                }
            }
        }

        public void ResetTabs()
        {
            foreach (TabButton button in tabButtons)
            {
                if (selectedTab != null && button == selectedTab)
                {
                    continue;
                }

                button.icon.color = iddleColor;
            }
        }
    }
}
