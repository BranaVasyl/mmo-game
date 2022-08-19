using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class MenuPanel : MonoBehaviour
    {
        [Header("Panel Option")]
        public string name;

        public NetworkIdentity networkIdentity;
        [HideInInspector]
        public NewPlayerControls inputActions;
        [HideInInspector]
        public float clickTimer = 0;
        [HideInInspector]
        public bool rb_input = false;
        [HideInInspector]
        public bool lt_input = false;
        [HideInInspector]
        public bool rt_input = false;
        [HideInInspector]
        public bool x_input = false;

        public virtual void Init(NetworkIdentity nI)
        {
            networkIdentity = nI;
            
            if (inputActions == null)
            {
                inputActions = new NewPlayerControls();
                GetInput();
            }

            inputActions.Enable();
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
