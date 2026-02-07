using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class EnemyAnimatorManager : AnimatorManager
    {
        EnemyManager enemyManager;

        [Header("IK Loaok Setting")]
        [Range(0f, 1f)] public float bodyWeight = 0.3f;
        [Range(0f, 1f)] public float headWeight = 1f;
        [Range(0f, 1f)] public float eyesWeight = 0f;
        [Range(0f, 1f)] public float clampWeight = 0.5f;
        [Range(0f, 180f)] public float maxLookAngle = 90f;
        
        public float headPosHeight = 1.6f;
        public float lookLerpSpeed = 6f;
        

Vector3 currentLookDir;
        Vector3 currentLookAt;
        Vector3 lookAtVelocity;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            enemyManager = GetComponentInParent<EnemyManager>();
        }

        private void OnAnimatorMove()
        {
            float delta = Time.deltaTime;
            enemyManager.enemyRigidbody.drag = 0;
            Vector3 deltaPosition = anim.deltaPosition;
            deltaPosition.y = 0;

            if (!enemyManager.enemyRigidbody.isKinematic)
            {
                Vector3 velocity = deltaPosition / delta;
                enemyManager.enemyRigidbody.velocity = velocity;    
            }

            if (enemyManager.isRotatingWithRootMotion)
            {
                enemyManager.transform.rotation *= anim.deltaRotation;
            }
        }

        public void EnableCombo()
        {
            anim.SetBool("canDoCombo", true);
        }

        public void DisableCombo()
        {
            anim.SetBool("canDoCombo", false);
        }

        public void OpenDamageColliders()
        {
            if (enemyManager == null)
            {
                return;
            }

            enemyManager.inventoryManager.w_hook.OpenDamageColliders();
        }

        public void CloseDamageColliders()
        {
            if (enemyManager == null || enemyManager.inventoryManager.w_hook == null)
            {
                return;
            }

            enemyManager.inventoryManager.w_hook.CloseDamageColliders();
        }

        public void Step(string animationType)
        {
        }

        private void OnAnimatorIK(int layerIndex)
        {
            Transform t = anim.transform;

            Vector3 headPos = t.position + Vector3.up * headPosHeight;

            Vector3 desiredDir = t.forward;

            if (enemyManager.lookAtPosition != Vector3.zero)
            {
                Vector3 toTarget = enemyManager.lookAtPosition - headPos;
                toTarget.y = 0f;

                float angle = Vector3.Angle(t.forward, toTarget);

                if (angle <= maxLookAngle)
                {
                    desiredDir = toTarget.normalized;
                }
            }

            currentLookDir = Vector3.Lerp(
                currentLookDir,
                desiredDir,
                Time.deltaTime * lookLerpSpeed
            );

            currentLookDir.Normalize();
            currentLookAt = headPos + currentLookDir * 5f;

            anim.SetLookAtWeight(1f, bodyWeight, headWeight, eyesWeight, clampWeight);
            anim.SetLookAtPosition(currentLookAt);
        }

        private void OnDrawGizmos()
        {
            if (!GizmosManager.Instance.showAiLookAtTarget)
            {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(currentLookAt, 0.05f);
        }
    }
}