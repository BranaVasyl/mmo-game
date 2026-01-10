using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class EnemyAnimatorManager : AnimatorManager
    {
        EnemyManager enemyManager;

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
    }
}