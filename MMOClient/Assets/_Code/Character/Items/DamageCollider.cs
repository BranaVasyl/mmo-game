using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class DamageCollider : MonoBehaviour
    {
        private DamageManager damageManager;
        private List<string> damagedCharactersId;

        void Start()
        {
            damageManager = DamageManager.singleton;
        }

        private void OnTriggerEnter(Collider other)
        {
            CharacterManager agentCManager = GetComponentInParent<CharacterManager>();
            CharacterManager targetCManager = other.transform.GetComponentInParent<CharacterManager>();

            if (targetCManager == null || agentCManager == null || targetCManager == agentCManager)
            {
                return;
            }

            if(targetCManager.networkIdentity == null) {
                return;
            }

            string targetId = targetCManager.networkIdentity.GetID();
            if(damagedCharactersId.Find(x => x == targetId) != null) {
                return;
            }

            float damage = 35;
            damageManager.CreatateDamageEvent(agentCManager, targetCManager, damage);
            damagedCharactersId.Add(targetId);
        }

        public void Deinit()
        {
            damagedCharactersId = new List<string>();
        }
    }
}
