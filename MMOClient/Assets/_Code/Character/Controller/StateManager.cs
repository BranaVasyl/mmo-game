using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class StateManager : CharacterManager
    {
        [Header("Init")]
        public GameObject activeModel;
        List<Rigidbody> ragdollRigids = new List<Rigidbody>();
        List<Collider> ragdollColliders = new List<Collider>();

        [Header("Inputs")]
        public float vertical;
        public float horizontal;
        public float moveAmount;
        public Vector3 moveDir;
        public bool rt, rb, lt, lb;
        public bool rollInput;
        public bool itemInput;

        [Header("Stats")]
        public float moveSpeed = 2;
        public float runSpeed = 3.5f;
        public float rotateSpeed = 5;
        public float toGround = 0.5f;
        public float rollSpeed = 1;

        [Header("States")]
        public bool onGround;
        public bool run;
        public bool walk;
        public bool lockOn;
        public bool inAction;
        public bool canMove;
        public bool isTwoHanded;
        public bool usingItem;
        public bool openInventory;
        public string currentAnimation;

        [Header("Other")]
        public EnemyTarget lockOnTarget;
        public Transform lockOnTransform;
        public AnimationCurve roll_curve;

        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public Rigidbody rigid;
        [HideInInspector]
        public PlayerAnimatorManager animatorManager;
        [HideInInspector]
        public ActionManager actionManager;
        [HideInInspector]
        public InventoryManager inventoryManager;

        [HideInInspector]
        public float delta;
        [HideInInspector]
        public LayerMask ignoreLayers;
        float _actionDelay;

        [Header("TestEquip")]
        public int curEquip;
        public GameObject equipHand;
        public GameObject equipSpine;

        [Header("StayAnim")]
        public string[] stayAnim;
        public int playAnimIn;
        private float timerStay;

        [Header("Network Sync")]
        private float lastSynchronizationTime;
        private float syncDelay = 0f;
        private float syncTime = 0f;
        private Vector3 syncStartPosition = Vector3.zero;
        private Vector3 syncEndPosition = Vector3.zero;
        private Quaternion syncStartRotation = Quaternion.identity;
        private Quaternion syncEndRotation = Quaternion.identity;

        float lastPelvisPositionY;
        FootIK footIk;

        public void Update()
        {
            if (moveAmount == 0)
            {
                anim.SetFloat("moveAmount", 0);
                timerStay += Time.deltaTime;
            }
            else
            {
                anim.SetFloat("moveAmount", moveAmount);
                timerStay = 0;
            }

            if (timerStay >= playAnimIn)
            {
                PlayRandomStayAnim();
            }

            if (base.isInvicible)
            {
                base.isInvicible = !anim.GetBool("canMove");
            }
        }

        void FixedUpdate()
        {
            if (!base.networkIdentity.IsControlling())
            {
                syncTime += delta;

                gameObject.transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
                gameObject.transform.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime / syncDelay);
            }
        }

        public void UpdateState(PlayerData playerData)
        {
            if (!isDead && playerData.isDead)
            {
                isDead = playerData.isDead;
                EnableRagdoll();
            }

            if (currentAnimation != playerData.currentAnimation)
            {
                PlayAnimation(playerData.currentAnimation);
            }

            syncTime = 0f;
            Vector3 syncVelocity = Vector3.zero;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncEndPosition = playerData.position + syncVelocity * syncDelay;
            syncStartPosition = gameObject.transform.position;

            syncEndRotation = Quaternion.Euler(playerData.rotation.x, playerData.rotation.y, playerData.rotation.z);
            syncStartRotation = gameObject.transform.rotation;

            horizontal = playerData.horizontal;
            vertical = playerData.vertical;

            float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            moveAmount = Mathf.Clamp01(m);

            run = playerData.run;
            walk = playerData.walk;

            if (isTwoHanded != playerData.isTwoHanded)
            {
                isTwoHanded = playerData.isTwoHanded;
                HandleTwoHanded();
            };
        }

        public void Init()
        {
            SetupAnimator();
            footIk = anim.gameObject.GetComponent<FootIK>();

            rigid = GetComponent<Rigidbody>();
            rigid.angularDrag = 999;
            rigid.drag = 4;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            base.SetAnimator(anim);
            base.SetNetworkIdentity(gameObject.GetComponent<NetworkIdentity>());

            inventoryManager = GetComponent<InventoryManager>();
            inventoryManager.Init(this);

            actionManager = GetComponent<ActionManager>();
            actionManager.Init(this);

            animatorManager = activeModel.AddComponent<PlayerAnimatorManager>();
            animatorManager.Init(this);

            gameObject.layer = 8;
            ignoreLayers = ~(1 << 9);

            anim.SetBool("onGround", true);

            InitRagdoll();

            if (base.networkIdentity.IsControlling())
            {
                InventoryController.singleton.RegisterCharacterListener(this);
            }
        }

        void InitRagdoll()
        {
            Rigidbody[] rigs = GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < rigs.Length; i++)
            {
                if (rigs[i] == rigid)
                {
                    continue;
                }

                ragdollRigids.Add(rigs[i]);
                rigs[i].isKinematic = true;

                Collider col = rigs[i].gameObject.GetComponent<Collider>();
                col.isTrigger = true;
                ragdollColliders.Add(col);
            }
        }

        public void EnableRagdoll()
        {
            for (int i = 0; i < ragdollColliders.Count; i++)
            {
                ragdollRigids[i].isKinematic = false;
                ragdollColliders[i].isTrigger = false;
            }

            Collider controllerCollider = rigid.gameObject.GetComponent<Collider>();
            controllerCollider.enabled = false;
            rigid.isKinematic = true;

            StartCoroutine("CloseAnimator");
        }

        IEnumerator CloseAnimator()
        {
            yield return new WaitForEndOfFrame();
            anim.enabled = false;
            //this.enabled = false;
        }

        void SetupAnimator()
        {
            if (activeModel == null)
            {
                anim = GetComponentInChildren<Animator>();
                if (anim == null)
                {
                    Debug.Log("No Model Found");
                }
                else
                {
                    activeModel = anim.gameObject;
                }
            }

            if (anim == null)
                anim = activeModel.GetComponent<Animator>();

            anim.applyRootMotion = false;
        }

        public void FixedTick(float d)
        {
            delta = d;

            usingItem = anim.GetBool("interacting");

            DetectItemAction();
            DetectedAction();

            if (inventoryManager.rightHandObject)
            {
                inventoryManager.rightHandObject.SetActive(!usingItem);
            }

            if (inAction)
            {
                anim.applyRootMotion = true;

                _actionDelay += delta;
                if (_actionDelay > 0.3f)
                {
                    inAction = false;
                    _actionDelay = 0;
                }
                else
                {
                    return;
                }
            }

            canMove = anim.GetBool("canMove");

            if (!canMove)
            {
                return;
            }

            if (currentAnimation.Length > 0 && !usingItem)
            {
                currentAnimation = "";
            }

            animatorManager.CloseRoll();
            HandleRolls();

            anim.applyRootMotion = false;
            rigid.drag = (moveAmount > 0 || !onGround) ? 0 : 4;

            float targetSpeed = moveSpeed;
            if (usingItem)
            {
                walk = true;
            }

            if (walk)
            {
                if (run)
                {
                    moveAmount = Mathf.Clamp(moveAmount, 0, 1f);
                }
                else
                {
                    moveAmount = Mathf.Clamp(moveAmount, 0, 0.5f);
                }
            }

            if (run && !walk)
            {
                targetSpeed = runSpeed;
                lockOn = false;
            }

            if (onGround)
                rigid.velocity = moveDir * (targetSpeed * moveAmount);
            else
                lastPelvisPositionY = transform.position.y;

            Vector3 targetDir = (lockOn == false) ? moveDir
            : (lockOnTransform != null) ?
                lockOnTransform.transform.position - transform.position : moveDir;

            targetDir.y = 0;

            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * rotateSpeed);
            transform.rotation = targetRotation;

            anim.SetBool("lock_on", lockOn);

            if (lockOn == false)
            {
                HandleMovementAnimation();
            }
            else
            {
                HandleLockOnAnimations(moveDir);
            }
        }

        public void Tick(float d)
        {
            delta = d;
            onGround = OnGround();

            anim.SetBool("onGround", onGround);
        }

        public void ChangeEquip(int but)
        {
            anim.CrossFade("Empty Override", 0.2f);

            if (but == 0)
                curEquip = 0;
            else if (curEquip != but)
                curEquip = but;
            else
                curEquip = 0;

            if (curEquip == 0)
            {
                equipHand.SetActive(false);
                equipSpine.SetActive(true);
            }
            else
            {
                equipHand.SetActive(true);
                equipSpine.SetActive(false);
            }

            anim.SetInteger("equip", curEquip);
        }

        public void PlayRandomStayAnim()
        {
            if (curEquip != 0)
            {
                ChangeEquip(0);
                timerStay = 0;
                return;
            }

            string targetAnim;
            int r = Random.Range(0, stayAnim.Length);
            targetAnim = stayAnim[r];

            //anim.CrossFade(targetAnim, 0.2f);
            timerStay = 0;
        }

        void HandleMovementAnimation()
        {
            anim.SetBool("run", run);
            if (!run || walk)
                anim.SetFloat("vertical", moveAmount, 0.2f, delta);
            else
                anim.SetFloat("vertical", 1.5f, 0.2f, delta);
        }

        void HandleLockOnAnimations(Vector3 moveDir)
        {
            Vector3 relativeDir = transform.InverseTransformDirection(moveDir);
            float h = relativeDir.x;
            float v = relativeDir.z;

            anim.SetFloat("vertical", v, 0.2f, delta);
            anim.SetFloat("horizontal", h, 0.2f, delta);
        }

        public bool OnGround()
        {
            bool r = false;

            Vector3 origin = transform.position + (Vector3.up * toGround);
            Vector3 dir = -Vector3.up;
            float dis = toGround + 0.5f;

            RaycastHit hit;
            if (Physics.Raycast(origin, dir, out hit, dis, ignoreLayers))
            {
                r = true;
                Vector3 targetPosition = hit.point;

                if (moveAmount == 0 && footIk.enableFeetIk)
                    targetPosition.y = Mathf.Lerp(lastPelvisPositionY, footIk.MovePelvisHeight(), 0.05f);
                else if (moveAmount <= 0.5f)
                    targetPosition.y = Mathf.Lerp(lastPelvisPositionY, targetPosition.y, 0.15f);
                else if (moveAmount <= 1 && !run)
                    targetPosition.y = Mathf.Lerp(lastPelvisPositionY, targetPosition.y, 0.2f);
                else if (run)
                    targetPosition.y = Mathf.Lerp(lastPelvisPositionY, targetPosition.y, 0.3f);

                // if(lastPelvisPositionY < targetPosition.y)
                //     Debug.Log("up");
                // else    
                //     Debug.Log("down");

                transform.position = targetPosition;
                lastPelvisPositionY = transform.position.y;
            }

            return r;
        }

        public void DetectItemAction()
        {
            if (canMove == false || usingItem)
            {
                return;
            }

            if (itemInput == false)
            {
                return;
            }

            ItemAction slot = actionManager.consumableItem;
            string targetAnim = slot.targetAnim;
            if (string.IsNullOrEmpty(targetAnim))
            {
                return;
            }

            usingItem = true;
            anim.Play(targetAnim);
            currentAnimation = targetAnim;
        }

        public void DetectedAction()
        {
            if (canMove == false || usingItem)
            {
                return;
            }

            if (rb == false && rt == false && lt == false && lb == false)
            {
                return;
            }

            string targetAnim = null;

            Action slot = actionManager.GetActionSlot(this);
            if (slot == null)
            {
                return;
            }
            targetAnim = slot.targetAnim;

            if (string.IsNullOrEmpty(targetAnim))
            {
                return;
            }

            canMove = false;
            inAction = true;
            anim.SetBool("mirror", slot.mirror);
            anim.CrossFade(targetAnim, 0.2f);
            currentAnimation = targetAnim;
        }

        void HandleRolls()
        {
            if (!rollInput || usingItem)
            {
                return;
            }

            float v = vertical;
            float h = horizontal;

            //Blending Mode
            // if (lockOn == false)
            // {
            //     v = (moveAmount > 0.3f) ? 1 : 0;
            //     h = 0;
            // }
            // else
            // {
            //     if (Mathf.Abs(v) < 0.3f)
            //     {
            //         v = 0;
            //     }
            //     if (Mathf.Abs(h) < 0.3f)
            //     {
            //         h = 0;
            //     }
            // }
            //end Blending Mode

            //not use BlendingMode
            v = (moveAmount > 0.3f) ? 1 : 0;
            h = 0;

            if (v != 0)
            {
                if (moveDir == Vector3.zero)
                {
                    moveDir = transform.forward;
                }
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = targetRot;
                animatorManager.InitForRoll();
                animatorManager.rm_multi = rollSpeed;
            }
            else
            {
                animatorManager.rm_multi = 1.3f;
            }
            //end not use BlendingMode


            anim.SetFloat("vertical", v);
            anim.SetFloat("horizontal", h);

            canMove = false;
            inAction = true;
            anim.CrossFade("Rolls", 0.2f);
            currentAnimation = "Rolls";
        }

        public void HandleTwoHanded()
        {
            anim.SetBool("twoHanded", isTwoHanded);

            if (isTwoHanded)
            {
                actionManager.UpdateActionsTwoHanded();
            }
            else
            {
                actionManager.UpdateActionsOneHanded();
            }
        }

        public void PlayAnimation(string targetAnim)
        {
            anim.Play(targetAnim);
        }
    }
}