using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BV
{
    public class PieMenu : MonoBehaviour
    {
        public int itemCount = 8;
        private Vector2 normalisedMousePosition;
        private float currentAngle;
        [HideInInspector]
        public int selection = 0;
        private int previousSelection = 0;

        public TMP_Text titleContainer;
        public TMP_Text descriptionContainer;

        public PieMenuItem[] menuItems;
        public Sprite emptySpellIcon;
        public Sprite emptyItemIcon;

        private Spell[] spellsData;

        private PieMenuItem menuItemSc;
        private PieMenuItem previousMenuItemSc;

        private NewPlayerControls inputActions;

        void Awake()
        {
            if (inputActions == null)
            {
                inputActions = new NewPlayerControls();
            }

            inputActions.Enable();
        }

        public void SetSpellData(Spell[] sD)
        {
            spellsData = sD;
            for (int i = 0; i < spellsData.Length; i++)
            {
                if (spellsData[i] != null)
                {
                    menuItems[i].icon.GetComponent<Image>().sprite = spellsData[i].icon;
                }
            }
        }

        public void Clean()
        {
            titleContainer.text = "";
            descriptionContainer.text = "";

            for (int i = 0; i < 4; i++)
            {
                menuItems[i].icon.GetComponent<Image>().sprite = emptySpellIcon;
                menuItems[i].Deselect();
            }

            for (int i = 4; i < 8; i++)
            {
                menuItems[i].icon.GetComponent<Image>().sprite = emptyItemIcon;
                menuItems[i].Deselect();
            }

            previousSelection = -1;
            selection = 0;
        }

        void Update()
        {
            Vector2 mousePosition = inputActions.Mouse.MousePosition.ReadValue<Vector2>();
            normalisedMousePosition = new Vector2(mousePosition.x - Screen.width / 2, mousePosition.y - Screen.height / 2);
            currentAngle = Mathf.Atan2(normalisedMousePosition.y, normalisedMousePosition.x) * Mathf.Rad2Deg;

            currentAngle = (currentAngle + 360) % 360;

            selection = (int)currentAngle / (360 / itemCount);

            if (selection != previousSelection)
            {
                if (previousSelection >= 0 && previousSelection <= menuItems.Length)
                {
                    previousMenuItemSc = menuItems[previousSelection];
                    previousMenuItemSc.Deselect();
                }

                previousSelection = selection;
                menuItemSc = menuItems[previousSelection];
                titleContainer.text = "";
                descriptionContainer.text = "";

                if (selection < spellsData.Length)
                {
                    Spell selectionItem = spellsData[selection];
                    if (selectionItem != null)
                    {
                        titleContainer.text = selectionItem.name;
                        descriptionContainer.text = selectionItem.description;
                    }
                }
                menuItemSc.Select();
            }
        }
    }
}
