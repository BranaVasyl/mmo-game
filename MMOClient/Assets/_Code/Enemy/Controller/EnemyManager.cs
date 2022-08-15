using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Utility;
using Project.Networking;

namespace BV
{
    public class EnemyManager : CharacterManager
    {
        [Header("Init")]
        private GameObject activeModel;
        private Animator anim;
        private EnemyTarget enemyTarget;
        private EnemyNetworkTransform enemyNetworkTransform;
        public Rigidbody enemyRigidbody;
        public LayerMask ignoreLayers;

        List<Rigidbody> ragdollRigids = new List<Rigidbody>();
        List<Collider> ragdollColliders = new List<Collider>();

        private float lastSynchronizationTime;
        private float syncDelay = 0f;
        private float syncTime = 0f;
        private Vector3 syncStartPosition = Vector3.zero;
        private Vector3 syncEndPosition = Vector3.zero;
        private Quaternion syncStartRotation = Quaternion.identity;
        private Quaternion syncEndRotation = Quaternion.identity;

        [Header("Animation State")]
        public string tempAnimationId = "";
        public bool isInteracting = false;
        public string currentAnimation = "";
        [Header("Movement Flags")]
        public bool isRotatingWithRootMotion;

        bool isMove = false;
        bool onGround;
        float delta;

        float vertical;
        float horizontal;
        float moveAmount;

        float lastPelvisPositionY;
        FootIK footIk;

        public InventoryManager inventoryManager;

        void Start()
        {
            SetupAnimator();
            enemyRigidbody = GetComponent<Rigidbody>();
            footIk = anim.gameObject.GetComponent<FootIK>();
            enemyNetworkTransform = gameObject.GetComponent<EnemyNetworkTransform>();

            base.SetAnimator(anim);
            base.SetNetworkIdentity(gameObject.GetComponent<NetworkIdentity>());

            enemyTarget = gameObject.GetComponent<EnemyTarget>();
            enemyTarget.Init(anim);

            gameObject.layer = 8;
            ignoreLayers = ~(1 << 9);

            syncEndPosition = gameObject.transform.position;
            syncEndRotation = gameObject.transform.rotation;

            inventoryManager = GetComponent<InventoryManager>();
            inventoryManager.Init();

            InitRagdoll();
        }

        void InitRagdoll()
        {
            Rigidbody[] rigs = GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < rigs.Length; i++)
            {
                if (rigs[i] == enemyRigidbody)
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

            Collider controllerCollider = enemyRigidbody.gameObject.GetComponent<Collider>();
            controllerCollider.enabled = false;
            enemyRigidbody.isKinematic = true;

            StartCoroutine("CloseAnimator");
        }

        IEnumerator CloseAnimator()
        {
            yield return new WaitForEndOfFrame();
            anim.enabled = false;
            //this.enabled = false;
        }

        void Update()
        {
            delta = Time.deltaTime;
            onGround = OnGround();
            anim.SetBool("onGround", onGround);
            isRotatingWithRootMotion = anim.GetBool("isRotatingWithRootMotion");

            if (base.isInvicible)
            {
                base.isInvicible = !anim.GetBool("canMove");
            }
        }

        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;

            syncTime += delta;

            if (!isInteracting)
            {
                gameObject.transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
                gameObject.transform.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime / syncDelay);
            }

            vertical = isMove ? 0.5f : 0;
            horizontal = 0;
            float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            moveAmount = Mathf.Clamp01(m);

            UpdateStatesNetworkCleint();
        }

        public void UpdateState(Vector3 position, Quaternion rotation, bool move, bool interacting, string curAnim, string tempId, bool dead)
        {
            if (!isDead && dead)
            {
                isDead = dead;
                EnableRagdoll();
            }

            syncTime = 0f;
            Vector3 syncVelocity = Vector3.zero;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            if (isInteracting && base.networkIdentity.IsControlling())
            {
                enemyNetworkTransform.SendData();
            }
            syncEndPosition = position + syncVelocity * syncDelay;
            syncStartPosition = gameObject.transform.position;

            syncEndRotation = rotation;
            syncStartRotation = gameObject.transform.rotation;

            isMove = move;
            isInteracting = interacting;
            if (tempAnimationId != tempId)
            {
                if (curAnim == "Turn Behind Right" || curAnim == "Turn Behind Left" || curAnim == "Turn Left" || curAnim == "Turn Right")
                {
                    PlayTargetAnimationWithRootRotation(curAnim, isInteracting);
                }
                else
                {
                    PlayTargetAnimation(curAnim, isInteracting);
                }

                tempAnimationId = tempId;
                currentAnimation = curAnim;
            }
        }

        public bool OnGround()
        {
            bool result = false;

            Vector3 origin = transform.position + (Vector3.up * 0.5f);
            Vector3 dir = -Vector3.up;
            float dis = 1f;

            RaycastHit hit;
            if (Physics.Raycast(origin, dir, out hit, dis, ignoreLayers))
            {
                result = true;
                Vector3 targetPosition = hit.point;

                if (moveAmount == 0 && footIk.enableFeetIk)
                    targetPosition.y = Mathf.Lerp(lastPelvisPositionY, footIk.MovePelvisHeight(), 0.05f);
                else
                    targetPosition.y = Mathf.Lerp(lastPelvisPositionY, targetPosition.y, 0.2f);

                transform.position = targetPosition;
                lastPelvisPositionY = transform.position.y;
            }

            return result;
        }

        void UpdateStatesNetworkCleint()
        {
            anim.SetFloat("vertical", moveAmount, 0.2f, delta);
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

        public void PlayTargetAnimation(string targetAnim, bool isInteracting)
        {
            anim.applyRootMotion = isInteracting;
            anim.SetBool("isInteracting", isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
        }

        public void PlayTargetAnimationWithRootRotation(string targetAnim, bool isInteracting)
        {
            anim.applyRootMotion = isInteracting;
            anim.SetBool("isRotatingWithRootMotion", true);
            anim.SetBool("isInteracting", isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
        }
    }
}