using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BV
{
    public class PieMenu : MonoBehaviour
    {
        public int itemCount = 8;
        private Vector2 normalisedMousePosition;
        private float currentAngle;
        private int selection = 0;
        private int previousSelection = 0;
        
        public TMP_Text titleContainer;
        public TMP_Text descriptionContainer; 

        public GameObject[] menuItems;

        private PieMenuItem menuItemSc;
        private PieMenuItem previousMenuItemSc;

        private NewPlayerControls inputActions;
        private float clickTimer = 0;
        private bool rb_input = false;
        private bool lt_input = false;
        private bool rt_input = false;
        private bool x_input = false;

        void Awake()
        {
            if (inputActions == null)
            {
                inputActions = new NewPlayerControls();
            }

            inputActions.Enable();
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
                previousMenuItemSc = menuItems[previousSelection].GetComponent<PieMenuItem>();
                titleContainer.text = "";
                descriptionContainer.text = "";
                previousMenuItemSc.Deselect();

                previousSelection = selection;
                menuItemSc = menuItems[previousSelection].GetComponent<PieMenuItem>();
                titleContainer.text = menuItemSc.title;
                descriptionContainer.text = menuItemSc.description;
                menuItemSc.Select();
            }
        }

        void GetInput()
        {
            inputActions.PlayerActions.X.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref x_input);
            inputActions.PlayerActions.RT.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref rt_input);
            inputActions.PlayerActions.LT.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref lt_input);
            inputActions.Mouse.LeftButtonDown.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref rb_input);
        }

        void ClickAction(float b, ref bool button)
        {
            if (clickTimer > 0)
            {
                return;
            }

            if (b > 0)
            {
                clickTimer = .1f;
                button = true;
            }
            else
            {
                button = false;
            }
        }
    }
}
