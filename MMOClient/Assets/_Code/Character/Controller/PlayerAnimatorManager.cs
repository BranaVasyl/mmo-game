using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class PlayerAnimatorManager : AnimatorManager
    {
        StateManager states;

        public float rm_multi;
        bool rolling;
        float roll_t;

        public bool killDelta;

        public void Init(StateManager st)
        {
            states = st;
            anim = st.anim;
        }

        public void InitForRoll()
        {
            rolling = true;
            roll_t = 0;
        }

        public void CloseRoll()
        {
            if (rolling == false)
            {
                return;
            }

            rm_multi = 1;
            roll_t = 0;
            rolling = false;
        }

        void OnAnimatorMove()
        {
            if (states == null)
            {
                return;
            }

            if (states.onEmpty)
            {
                return;
            }

            states.rigid.drag = 0;

            if (states.delta == 0)
            {
                return;
            }

            if (rm_multi == 0)
            {
                rm_multi = 1;
            }

            if (rolling == false)
            {
                Vector3 delta = anim.deltaPosition;
                if (killDelta)
                {
                    killDelta = false;
                    delta = Vector3.zero;
                }

                Vector3 v = (delta * rm_multi) / states.delta;
                states.rigid.velocity = v;
            }
            else
            {
                roll_t += states.delta / .6f;
                if (roll_t > 1)
                {
                    roll_t = 1;
                }

                float zValue = states.roll_curve.Evaluate(roll_t);
                Vector3 v1 = Vector3.forward * zValue;
                Vector3 relative = transform.TransformDirection(v1);
                Vector3 v2 = (relative * rm_multi);
                states.rigid.velocity = v2;
            }
        }

        public void OpenAttack()
        {
            if (states)
            {
                states.canAttack = true;
            }
        }

        public void OpenCanMove()
        {
            if (states && states.canAttack)
            {
                states.canMove = true;
            }
        }

        public void OpenDamageColliders()
        {
            if (states)
            {
                states.inventoryManager.OpenAllDamageColliders();
            }

        }

        public void CloseDamageColliders()
        {
            if (states)
            {
                states.inventoryManager.CloseAllDamageColliders();
            }
        }

        public void CloseParticle()
        {
            if (states)
            {
                if (states.inventoryManager.currentSpellParticle != null)
                {
                    states.inventoryManager.currentSpellParticle.SetActive(false);
                }
            }
        }

        public void InitiateThrowForProjectile()
        {
            if (states)
            {
                states.ThrowProjectile();
            }
        }

        public void Roll()
        {
            if (states && states.soundManager)
            {
                states.soundManager.Roll();
            }
        }

        public void StepBack()
        {
            if (states && states.soundManager)
            {
                states.soundManager.StepBack();
            }
        }

        public void Step(string animationType)
        {
            if (states && states.soundManager)
            {
                states.soundManager.Step(animationType);
            }
        }
    }
}
