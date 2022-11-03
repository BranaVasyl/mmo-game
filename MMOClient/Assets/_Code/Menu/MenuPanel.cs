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
        public string panelId;
        public string panelName;

        [HideInInspector]
        public NewPlayerControls inputActions;
        [HideInInspector]
        public bool rb_input = false;
        [HideInInspector]
        public bool lt_input = false;
        [HideInInspector]
        public bool rt_input = false;
        [HideInInspector]
        public bool x_input = false;

        [HideInInspector]
        public bool dragStart = false;
        [HideInInspector]
        public bool dragged = false;
        [HideInInspector]
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
            SimulateDragEvent();
        }

        void SimulateDragEvent()
        {
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
            //LTInput
            inputActions.PlayerActions.X.performed += inputActions => x_input = true;
            inputActions.PlayerActions.X.canceled += inputActions => x_input = false;

            //LTInput
            inputActions.PlayerActions.LT.performed += inputActions => lt_input = true;
            inputActions.PlayerActions.LT.canceled += inputActions => lt_input = false;

            //RTInput
            inputActions.PlayerActions.RT.performed += inputActions => rt_input = true;
            inputActions.PlayerActions.RT.canceled += inputActions => rt_input = false;

            //RBInput
            inputActions.PlayerActions.RB.performed += inputActions => rb_input = true;
            inputActions.PlayerActions.RB.canceled += inputActions => rb_input = false;
        }

        public virtual void Open()
        {
        }

        public virtual void Deinit()
        {
        }
    }
}
