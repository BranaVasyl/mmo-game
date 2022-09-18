using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;
using SocketIO;

namespace BV
{
    public class MenuPanel : MonoBehaviour
    {
        [Header("Panel Option")]
        public string name;

        [HideInInspector]
        public NewPlayerControls inputActions;
        [HideInInspector]
        public float clickTimer = 0;
        public bool rb_input = false;
        [HideInInspector]
        public bool lt_input = false;
        [HideInInspector]
        public bool rt_input = false;
        [HideInInspector]
        public bool x_input = false;

        public bool dragStart = false;
        public bool dragged = false;
        public bool dragEnd = false;

        public virtual void Init(SocketIOComponent soc, PlayerData pD)
        {
            if (inputActions == null)
            {
                inputActions = new NewPlayerControls();
                GetInput();
            }

            inputActions.Enable();
        }

        public void Update()
        {
            if (clickTimer > 0)
            {
                clickTimer -= Time.deltaTime;
            }

            if (dragEnd)
            {
                dragEnd = false;
            }

            if (dragged && !rb_input)
            {
                dragEnd = true;
                dragged = false;
            }

            if (dragStart)
            {
                dragStart = false;
                dragged = true;
            }

            if (rb_input && !dragged)
            {
                dragStart = true;
            }
        }

        void GetInput()
        {
            inputActions.PlayerActions.X.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref x_input);
            inputActions.PlayerActions.RT.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref rt_input);
            inputActions.PlayerActions.LT.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref lt_input);

            //RBInput
            inputActions.PlayerActions.RB.performed += inputActions => rb_input = true;
            inputActions.PlayerActions.RB.canceled += inputActions => rb_input = false;
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

        public virtual void Deinit()
        {
        }
    }
}
