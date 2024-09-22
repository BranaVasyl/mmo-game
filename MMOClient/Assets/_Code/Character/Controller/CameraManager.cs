using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class CameraManager : Singleton<CameraManager>
    {
        public bool lockon;
        public float followSpeed = 9;
        public float mouseSpeed = 1;

        public Transform target;
        public EnemyTarget lockonTarget;
        public Transform lockonTransform;

        [HideInInspector]
        public Transform pivot;
        [HideInInspector]
        public Transform camTrans;
        StateManager states;

        float turnSmoothing = .1f;
        public float minAngle = -35;
        public float maxAngle = 35;

        float smoothX;
        float smoothY;
        float smoothXvelocity;
        float smoothYvelocity;
        private float lookAngle;
        private float tiltAngle;

        bool usedRightAxis;

        NewPlayerControls inputActions;
        Vector2 cameraInput;

        public void Init(StateManager st)
        {
            states = st;
            target = st.transform;

            camTrans = Camera.main.transform;
            pivot = camTrans.parent;
        }

        public void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new NewPlayerControls();
                inputActions.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
            }

            inputActions.Enable();
        }

        public void Tick(float d)
        {
            float h = cameraInput.x;
            float v = cameraInput.y;
            float targetSpeed = mouseSpeed;

            if (lockonTarget != null)
            {
                if (lockonTransform == null)
                {
                    lockonTransform = lockonTarget.GetTarget();
                    states.lockOnTransform = lockonTransform;
                }
                else
                {
                    lockonTransform = lockonTarget.GetTarget();
                    states.lockOnTransform = lockonTransform;
                    usedRightAxis = true;
                }
            }

            FollowTarget(d);

            if (states.openMenu)
            {
                return;
            }
            HandleRotation(d, v, h, targetSpeed);
        }

        void FollowTarget(float d)
        {
            float speed = d * followSpeed;
            Vector3 targetPosition = Vector3.Lerp(transform.position, target.position, speed);
            transform.position = targetPosition;
        }

        void HandleRotation(float d, float v, float h, float targetSpeed)
        {
            if (turnSmoothing > 0)
            {
                smoothX = Mathf.SmoothDamp(smoothX, h, ref smoothXvelocity, turnSmoothing);
                smoothY = Mathf.SmoothDamp(smoothY, v, ref smoothYvelocity, turnSmoothing);

            }
            else
            {
                smoothX = h;
                smoothY = v;
            }

            tiltAngle -= smoothY * targetSpeed;
            tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);
            pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);

            if (lockon && lockonTarget != null)
            {
                Vector3 targetDir = lockonTransform.position - transform.position;
                targetDir.Normalize();
                //targetDir.y = 0;

                if (targetDir == Vector3.zero)
                {
                    targetDir = transform.forward;
                }
                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, d * 9);
                lookAngle = transform.eulerAngles.y;
                return;
            }

            lookAngle += smoothX * targetSpeed;
            transform.rotation = Quaternion.Euler(0, lookAngle, 0);
        }

        public static CameraManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}