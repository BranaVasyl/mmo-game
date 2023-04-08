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
        public bool rt, rb, lt, lb, b_input;
        public bool rollInput;
        public bool itemInput;
        public bool interactInput;

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
        public bool isSpellcasting;
        public bool canAttack;
        public bool isTwoHanded;
        public bool usingItem;
        public bool isBlocking;
        public bool inDialog;
        public bool isInvicible;
        public bool isLeftHand;
        public bool onEmpty;
        public bool openMenu;

        [Header("LockOn")]
        public List<EnemyTarget> lockablesList;
        public EnemyTarget lockOnTarget;
        public Transform lockOnTransform;

        [Header("Other")]
        public AnimationCurve roll_curve;

        [Header("Sound")]
        public PlayerSoundManager soundManager;

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
        [HideInInspector]
        public float airTimer;
        private ActionInput storePrevAction;
        private ActionInput storeActionInput;

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
            anim.SetFloat("moveAmount", moveAmount);
            if (isInvicible)
            {
                isInvicible = !anim.GetBool("onEmpty");
            }
        }

        void FixedUpdate()
        {
            if (!networkIdentity.IsControlling())
            {
                syncTime += delta;

                gameObject.transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
                gameObject.transform.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime / syncDelay);
            }
        }

        public void UpdateState(PlayerData playerData)
        {
            health = playerData.health;
            if (!isDead && playerData.isDead)
            {
                isDead = playerData.isDead;
                EnableRagdoll();
            }

            money = playerData.money;

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

            base.Init(gameObject.GetComponent<NetworkIdentity>());

            inventoryManager = GetComponent<InventoryManager>();
            inventoryManager.Init(this);

            actionManager = GetComponent<ActionManager>();
            actionManager.Init(this);

            animatorManager = activeModel.GetComponent<PlayerAnimatorManager>();
            animatorManager.Init(this);

            gameObject.layer = 8;
            ignoreLayers = ~(1 << 9);

            anim.SetBool("onGround", true);

            InitRagdoll();

            if (base.networkIdentity.IsControlling())
            {
                InventoryController.singleton.RegisterCharacterListener(this);
            }
            else
            {
                if (inventoryManager.inventoryCameraHodler != null)
                {
                    Destroy(inventoryManager.inventoryCameraHodler);
                }
            }

            //sound
            soundManager = GetComponent<PlayerSoundManager>();
            soundManager.Init(this);
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

            isBlocking = false;
            isLeftHand = false;

            usingItem = anim.GetBool("interacting");
            anim.SetBool("spellcasting", isSpellcasting);

            bool mirror = anim.GetBool("mirror");
            if (inventoryManager.rightHandObject && inventoryManager.rightHandObject.activeSelf == usingItem)
            {
                if (!mirror)
                {
                    inventoryManager.rightHandObject.SetActive(!usingItem);
                }
            }

            if (inventoryManager.leftHandObject && inventoryManager.leftHandObject.activeSelf == usingItem)
            {
                if (mirror && !isTwoHanded)
                {
                    inventoryManager.leftHandObject.SetActive(!usingItem);
                }
            }

            if (!usingItem)
            {
                if (inventoryManager.rightHandObject)
                {
                    inventoryManager.rightHandObject.SetActive(true);
                }

                if (inventoryManager.leftHandObject && !isTwoHanded)
                {
                    inventoryManager.leftHandObject.SetActive(true);
                }
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

            onEmpty = anim.GetBool("onEmpty");
            // canMove = anim.GetBool("canMove");

            if (onEmpty)
            {
                canAttack = true;
                canMove = true;
                actionManager.actionIndex = 0;
            }

            if (!onEmpty && !canMove && !canAttack)
            {
                return;
            }

            if (canMove && !onEmpty)
            {
                if (moveAmount > 0.3f)
                {
                    anim.CrossFade("Empty Override", 0.1f);
                    onEmpty = true;
                }
            }

            if (canAttack)
            {
                DetectedAction();
            }

            if (canMove)
            {
                DetectItemAction();
            }
            anim.SetBool("blocking", isBlocking);
            anim.SetBool("isLeft", isLeftHand);

            anim.applyRootMotion = false;
            rigid.drag = (moveAmount > 0 || !onGround) ? 0 : 4;

            float targetSpeed = moveSpeed;
            if (usingItem || isSpellcasting)
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

            if (onGround && canMove)
                rigid.velocity = moveDir * (targetSpeed * moveAmount);
            else
                lastPelvisPositionY = transform.position.y;

            HandleRotation();

            anim.SetBool("lock_on", lockOn);

            if (lockOn == false)
            {
                HandleMovementAnimation();
            }
            else
            {
                HandleLockOnAnimations(moveDir);
            }

            if (isSpellcasting)
            {
                HandleSpellCasting();
                return;
            }

            animatorManager.CloseRoll();
            HandleRolls();
        }

        float lockSampleRate = 0;

        public void UpdateLockableTagets()
        {
            if (lockSampleRate < 0)
            {
                LayerMask mask = (1 << 12);
                Collider[] cols = Physics.OverlapSphere(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), 20, mask);

                for (int i = 0; i < cols.Length; i++)
                {
                    EnemyTarget lockable = cols[i].gameObject.GetComponentInParent<EnemyTarget>();
                    if (lockable != null)
                    {
                        CharacterManager characterManager = lockable.gameObject.GetComponent<CharacterManager>();
                        if (!lockablesList.Contains(lockable) && !characterManager.isDead)
                        {
                            lockablesList.Add(lockable);
                        }
                    }
                }

                for (int i = 0; i < lockablesList.Count; i++)
                {
                    if (lockablesList[i] == null)
                    {
                        lockablesList.Remove(lockablesList[i]);
                        continue;
                    }

                    float distanse = Vector3.Distance(transform.position, lockablesList[i].transform.position);
                    CharacterManager characterManager = lockablesList[i].gameObject.GetComponent<CharacterManager>();

                    if (distanse > 20 || characterManager.isDead)
                    {
                        lockablesList.Remove(lockablesList[i]);
                    }
                }

                if (!lockablesList.Contains(lockOnTarget))
                {
                    DisableLockOn();
                }

                lockSampleRate = 1;
            }
            else
            {
                lockSampleRate -= Time.deltaTime;
            }
        }

        public EnemyTarget FindLockableTarget()
        {
            EnemyTarget result = null;
            float distanse = float.MaxValue;

            for (int i = 0; i < lockablesList.Count; i++)
            {
                float tempDistance = Vector3.Distance(transform.position, lockablesList[i].transform.position);
                if (tempDistance < distanse)
                {
                    distanse = tempDistance;
                    result = lockablesList[i];
                }
            }

            return result;
        }

        public void EnableLockon(EnemyTarget target)
        {
            lockOn = true;
            lockOnTarget = target;
            CameraManager.singleton.lockonTarget = target;

            lockOnTransform = CameraManager.singleton.lockonTransform;
            CameraManager.singleton.lockon = lockOn;
        }

        public void DisableLockOn()
        {
            lockOn = false;
            lockOnTarget = null;
            lockOnTransform = null;

            CameraManager.singleton.lockon = false;
            CameraManager.singleton.lockonTarget = null;
        }

        void HandleRotation()
        {
            Vector3 targetDir = (lockOn == false) ? moveDir
                        : (lockOnTransform != null) ?
                            lockOnTransform.transform.position - transform.position : moveDir;

            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * rotateSpeed);
            transform.rotation = targetRotation;
        }

        public bool IsInput()
        {
            if (rt || rb || lt || lb || rollInput)
            {
                return true;
            }

            return false;
        }

        public void Tick(float d)
        {
            delta = d;
            onGround = OnGround();
            anim.SetBool("onGround", onGround);

            if (!onGround)
            {
                airTimer += delta;
            }
            else
            {
                airTimer = 0;
            }
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
            if (onEmpty == false || usingItem || isBlocking)
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
        }

        public void DetectedAction()
        {
            if ((!canAttack && (onEmpty == false || usingItem)) || isSpellcasting || inDialog)
            {
                return;
            }

            if (rb == false && lb == false)
            {
                return;
            }

            ActionInput targetInput = actionManager.GetActionInput(this);

            storeActionInput = targetInput;
            if (onEmpty == false)
            {
                animatorManager.killDelta = true;
                targetInput = storePrevAction;
            }

            storePrevAction = targetInput;
            Action slot = actionManager.GetActionFromInput(targetInput);

            if (slot == null)
            {
                return;
            }

            switch (slot.type)
            {
                case ActionType.attack:
                    AttackAction(slot);
                    break;
                case ActionType.block:
                    BlockAction(slot);
                    break;
                case ActionType.spells:
                    SpellAction(slot);
                    break;
                case ActionType.parry:
                    ParryAction(slot);
                    break;
                case ActionType.interact:
                    break;
                default:
                    break;
            }
        }

        void AttackAction(Action slot)
        {
            string targetAnim = null;
            targetAnim = slot.GetActionStep(ref actionManager.actionIndex).GetBranch(storeActionInput).targetAnim;

            if (string.IsNullOrEmpty(targetAnim))
            {
                return;
            }

            canAttack = false;
            onEmpty = false;
            canMove = false;
            inAction = true;
            anim.SetBool("mirror", slot.mirror);
            anim.CrossFade(targetAnim, 0.08f);
        }

        void BlockAction(Action slot)
        {
            isBlocking = true;
            isLeftHand = slot.mirror;
        }

        void SpellAction(Action slot)
        {
            if (isSpellcasting)
            {
                return;
            }

            if (inventoryManager.currentSpellData == null || slot.spellClass != inventoryManager.currentSpellData.spellClass)
            {
                //targetAnim = cant cast spell
                Debug.Log("spell class doesn`t match");
                //anim.CrossFade(targetAnim, 0.2f);
                return;
            }

            ActionInput inp = actionManager.GetActionInput(this);
            if (inp == ActionInput.lb)
            {
                inp = ActionInput.rb;
            }
            if (inp == ActionInput.lt)
            {
                inp = ActionInput.rt;
            }

            Spell s_data = inventoryManager.currentSpellData;

            SpellAction s_slot = actionManager.GetSpellActionFromList(s_data.actions, inp);
            if (s_slot == null)
            {
                return;
            }

            canAttack = false;
            onEmpty = false;
            isSpellcasting = true;

            spellcastTime = 0;
            max_spellCastTime = s_slot.castTime;
            spellTargetAnim = s_slot.throwAnim;
            spellIsMirrored = slot.mirror;

            string targetAnim = s_slot.targetAnim;
            if (spellIsMirrored)
            {
                targetAnim += "_l";
            }
            else
            {
                targetAnim += "_r";
            }

            projectileCandidate = s_data.projectile;
            inventoryManager.CreateSpellParticle(spellIsMirrored);

            anim.SetBool("spellcasting", true);
            anim.SetBool("mirror", slot.mirror);
            anim.CrossFade(targetAnim, 0.2f);
        }

        float spellcastTime;
        float max_spellCastTime;
        string spellTargetAnim;
        bool spellIsMirrored;
        GameObject projectileCandidate;

        void HandleSpellCasting()
        {
            spellcastTime += delta;
            if (spellcastTime > max_spellCastTime)
            {
                canAttack = false;
                onEmpty = false;
                canMove = false;
                inAction = true;
                isSpellcasting = false;

                string targetAnim = spellTargetAnim;
                anim.SetBool("mirror", spellIsMirrored);
                anim.CrossFade(targetAnim, 0.2f);
            }
        }

        public void ThrowProjectile()
        {
            if (projectileCandidate == null)
            {
                return;
            }

            GameObject go = Instantiate(projectileCandidate) as GameObject;
            Transform p = spellIsMirrored ? inventoryManager.leftHandPivot.transform : inventoryManager.rightHandPivot.transform;
            go.transform.position = p.position;

            if (lockOnTransform && lockOn)
            {
                go.transform.LookAt(lockOnTransform.position);
            }
            else
            {
                go.transform.rotation = transform.rotation;
            }

            Projectile proj = go.GetComponent<Projectile>();
            proj.Init(this as CharacterManager);
        }

        void ParryAction(Action slot)
        {
            string targetAnim = null;
            targetAnim = slot.GetActionStep(ref actionManager.actionIndex).GetBranch(storeActionInput).targetAnim;

            if (string.IsNullOrEmpty(targetAnim))
            {
                return;
            }

            canAttack = false;
            onEmpty = false;
            canMove = false;
            inAction = true;
            anim.SetBool("mirror", slot.mirror);
            anim.CrossFade(targetAnim, 0.2f);
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

            canAttack = false;
            onEmpty = false;
            canMove = false;
            inAction = true;
            anim.CrossFade("Rolls", 0.2f);
        }

        public void HandleTwoHanded()
        {
            anim.SetBool("twoHanded", isTwoHanded);

            bool isRight = true;
            ItemWeaponData currentWeapon = inventoryManager.rightHandData;
            if (currentWeapon == null)
            {
                isRight = false;
                currentWeapon = inventoryManager.leftHandData;
            }

            if (currentWeapon == null)
            {
                return;
            }

            if (isTwoHanded)
            {
                anim.CrossFade(currentWeapon.th_idle_name, 0.2f);
                actionManager.UpdateActionsTwoHanded();

                if (isRight)
                {
                    if (inventoryManager.leftHandData != null)
                    {
                        inventoryManager.leftHandObject.SetActive(false);
                    }
                }
                else
                {
                    if (inventoryManager.rightHandData != null)
                    {
                        inventoryManager.rightHandObject.SetActive(false);
                    }
                }
            }
            else
            {
                anim.Play("equipWeapon_oh");
                actionManager.UpdateActionsOneHanded();

                if (inventoryManager.leftHandData != null)
                {
                    inventoryManager.leftHandObject.SetActive(true);
                }

                if (inventoryManager.rightHandData != null)
                {
                    inventoryManager.rightHandObject.SetActive(true);
                }
            }
        }

        public void PlayAnimation(string targetAnim)
        {
            anim.Play(targetAnim);
        }

        public override bool canDoDamage()
        {
            return !isInvicible && !isBlocking;
        }

        public override void DoDamage()
        {
            isInvicible = true;
            if (anim != null)
            {
                anim.Play("damage_1");
                anim.applyRootMotion = true;
                anim.SetBool("onEmpty", false);
            }
        }
    }
}