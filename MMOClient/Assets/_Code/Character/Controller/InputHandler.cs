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
        bool interact_Input;

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
        PieMenuManager pieMenuManager;
        GameUIManager gameUIManager;

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
                pieMenuManager = PieMenuManager.singleton;
                gameUIManager = GameUIManager.singleton;

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
            inputActions.PlayerActions.L.canceled += inputActions => rightAxis_down = false;

            //XInput
            inputActions.PlayerActions.X.performed += inputActions => x_input = true;
            inputActions.PlayerActions.X.canceled += inputActions => x_input = false;

            //InteractInput
            inputActions.PlayerActions.Interact.performed += inputActions => interact_Input = true;
            inputActions.PlayerActions.Interact.canceled += inputActions => interact_Input = false;

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
        }

        void UpdateStates()
        {
            if (inventory_input && !pieMenuManager.IsOpen())
            {
                inventory_input = false;
                if (!menuManager.IsOpen())
                {
                    states.openMenu = true;
                    menuManager.OpenMenu();
                }
                else
                {
                    states.openMenu = false;
                    menuManager.CloseMenu();
                }
            }

            if (rightAxis_down && !menuManager.IsOpen())
            {
                if (!pieMenuManager.IsOpen())
                {
                    states.openMenu = true;
                    pieMenuManager.OpenMenu();
                }
            }

            if (!rightAxis_down && pieMenuManager.IsOpen())
            {
                states.openMenu = false;
                pieMenuManager.CloseMenu();
            }

            if (menuManager.IsOpen() || states.inDialog)
            {
                states.run = false;
                states.moveAmount = 0;
                return;
            }

            states.horizontal = horizontal;
            states.vertical = vertical;

            Vector3 v = vertical * camManager.transform.forward;
            Vector3 h = horizontal * camManager.transform.right;
            states.moveDir = (v + h).normalized;

            float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            states.moveAmount = Mathf.Clamp01(m);

            if (pieMenuManager.IsOpen() || gameUIManager.IsAlreadyInteracted())
            {
                return;
            }

            if (states.openMenu)
            {
                states.openMenu = false;
            }

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
            states.b_input = b_input;
            states.interactInput = interact_Input;

            states.UpdateLockableTagets();

            if (y_input)
            {
                y_input = false;
                states.isTwoHanded = !states.isTwoHanded;
                states.HandleTwoHanded();
            }

            if (lt_input)
            {
                lt_input = false;
                if (states.lockOn)
                {
                    states.DisableLockOn();
                }
                else
                {
                    EnemyTarget target = states.FindLockableTarget();
                    if (target != null)
                    {
                        states.EnableLockon(target);
                    }
                }
            }

            if (interact_Input)
            {
                interact_Input = false;
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