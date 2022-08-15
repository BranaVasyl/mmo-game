using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;
using System;

namespace BV
{
    public class CharacterManager : MonoBehaviour
    {
        [Header("Character Stats")]
        public NetworkIdentity networkIdentity;
        public bool isInvicible = false;
        private Animator animator;
        public float health = 100;
        public bool isDead = false;

        public void SetNetworkIdentity(NetworkIdentity nI)
        {
            networkIdentity = nI;
        }

        public void SetAnimator(Animator a)
        {
            animator = a;
        }

        public void DoDamage(string agentId, float damage)
        {
            if (isInvicible)
            {
                return;
            }

            isInvicible = true;
            if (animator != null)
            {
                animator.Play("damage_1");
                animator.applyRootMotion = true;
                animator.SetBool("canMove", false);
            }

            if (networkIdentity.IsControlling())
            {
                SendDamageData sendData = new SendDamageData(agentId, networkIdentity.GetID(), damage);
                networkIdentity.GetSocket().Emit("doDamage", new JSONObject(JsonUtility.ToJson(sendData)));
            }
        }
    }

    [Serializable]
    public class SendDamageData
    {
        public string agentId;
        public string targetId;
        public string damage;

        public SendDamageData(string aId, string tId, float dam)
        {
            agentId = aId;
            targetId = tId;
            damage = dam.ToString();
        }
    }
}
