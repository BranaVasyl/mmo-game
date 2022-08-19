using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class InputHandler : MonoBehaviour
    {
        [Header("Class References")]
        [SerializeField]
        private NetworkIdentity networkIdentity;

        public float vertical;
        public float horizontal;
        public bool run;
        public bool walk;

        bool jump_input;
        bool b_input;
        bool a_input;
        bool x_input;
        bool y_input;

        bool rb_input;
        bool rt_input;
        bool lb_input;
        bool lt_input;
        bool inventory_input;

        bool leftAxis_down;
        bool rightAxis_down;

        float b_timer;
        float rt_timer;
        float lt_timer;

        StateManager states;
        CameraManager camManager;
        MenuManager menuManager;

        NewPlayerControls inputActions;

        float delta;

        Vector2 movementInput;

        void Start()
        {
            states = GetComponent<StateManager>();
            states.Init();

            if (networkIdentity.IsControlling())
            {
                camManager = CameraManager.singleton;
                camManager.Init(states);

                menuManager = MenuManager.singleton;
                menuManager.Init(networkIdentity);

                if (inputActions == null)
                {
                    inputActions = new NewPlayerControls();
                    GetInput();
                }

                inputActions.Enable();
            }
        }

        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;

            states.FixedTick(delta);
            if (networkIdentity.IsControlling())
            {
                UpdateStates();
                camManager.Tick(delta);
            }

            if (b_input)
            {
                b_timer += delta;
            }
        }

        void Update()
        {
            delta = Time.deltaTime;
            states.Tick(delta);
            ResetInputNStates();
        }

        void GetInput()
        {
            //movement
            inputActions.PlayerMovement.Movement.performed += inputActions =>
            {
                movementInput = inputActions.ReadValue<Vector2>();
                horizontal = movementInput.x;
                vertical = movementInput.y;
            };

            //BInput
            inputActions.PlayerActions.Run.performed += inputActions => b_input = true;
            inputActions.PlayerActions.Run.canceled += inputActions => b_input = false;

            //JumpInput
            inputActions.PlayerActions.Jump.performed += inputActions => jump_input = true;
            inputActions.PlayerActions.Jump.canceled += inputActions => jump_input = false;

            //AInput
            inputActions.PlayerActions.Walk.performed += inputActions => a_input = !a_input;

            //YInput
            inputActions.PlayerActions.TwoHanded.performed += inputActions => y_input = true;

            //YInput
            inputActions.PlayerActions.Inventory.performed += inputActions => inventory_input = true;

            //LInput
            inputActions.PlayerActions.L.performed += inputActions => rightAxis_down = true;

            //XInput
            inputActions.PlayerActions.X.performed += inputActions => x_input = true;
            inputActions.PlayerActions.X.canceled += inputActions => x_input = false;

            //RBInput
            inputActions.PlayerActions.RB.performed += inputActions => rb_input = true;
            inputActions.PlayerActions.RB.canceled += inputActions => rb_input = false;

            //RTInput
            inputActions.PlayerActions.RT.performed += inputActions => rt_input = true;
            inputActions.PlayerActions.RT.canceled += inputActions => rt_input = false;

            //LBInput
            inputActions.PlayerActions.LB.performed += inputActions => lb_input = true;
            inputActions.PlayerActions.LB.canceled += inputActions => lb_input = false;

            //LTInput
            inputActions.PlayerActions.LT.performed += inputActions => lt_input = true;
            inputActions.PlayerActions.LT.canceled += inputActions => lt_input = false;

            // if(Input.GetButtonDown("showCursor"))
            //     Cursor.visible  = !Cursor.visible ;

            // if(Input.GetButtonDown("equip1"))
            //     states.ChangeEquip(1);

            // if(Input.GetButtonDown("equip2"))
            //     states.ChangeEquip(2);
        }

        bool GetButtonStatus(UnityEngine.InputSystem.InputActionPhase phase)
        {
            return phase == UnityEngine.InputSystem.InputActionPhase.Started;
        }

        void UpdateStates()
        {
            if (inventory_input)
            {
                inventory_input = false;
                if (!menuManager.IsOpen())
                {
                    menuManager.OpenMenu();
                }
                else
                {
                    menuManager.CloseMenu();
                }
            }
            if(menuManager.IsOpen()) {
                return;
            }        


            states.horizontal = horizontal;
            states.vertical = vertical;

            Vector3 v = vertical * camManager.transform.forward;
            Vector3 h = horizontal * camManager.transform.right;
            states.moveDir = (v + h).normalized;

            float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            states.moveAmount = Mathf.Clamp01(m);

            if (x_input)
            {
                b_input = false;
            }

            states.rollInput = jump_input;

            if (b_input && b_timer > 0.5f)
            {
                states.run = (states.moveAmount == 1);
            }
            else
            {
                states.run = false;
            }

            states.walk = a_input;
            states.itemInput = x_input;
            states.rt = rt_input;
            states.lt = lt_input;
            states.rb = rb_input;
            states.lb = lb_input;

            if (y_input)
            {
                y_input = false;
                states.isTwoHanded = !states.isTwoHanded;
                states.HandleTwoHanded();
            }

            if (rightAxis_down)
            {
                rightAxis_down = false;
                states.lockOn = !states.lockOn;
                if (states.lockOnTarget == null)
                {
                    states.lockOn = false;
                }

                camManager.lockonTarget = states.lockOnTarget;
                states.lockOnTransform = camManager.lockonTransform;
                camManager.lockon = states.lockOn;
            }
        }

        void ResetInputNStates()
        {
            if (b_input == false)
            {
                b_timer = 0;
            }
        }
    }
}