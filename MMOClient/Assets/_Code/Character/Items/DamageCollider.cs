using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class DamageCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            CharacterManager agentCManager = GetComponentInParent<CharacterManager>();
            CharacterManager targetCManager = other.transform.GetComponentInParent<CharacterManager>();

            if (targetCManager == null || agentCManager == null || targetCManager == agentCManager)
            {
                return;
            }

            float damage = 35;
            targetCManager.DoDamage(agentCManager.networkIdentity.GetID(), damage);
        }
    }
}
