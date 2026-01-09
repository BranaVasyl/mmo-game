using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class Projectile : MonoBehaviour
    {
        private DamageManager damageManager;
        private List<string> damagedCharactersId = new List<string>();

        Rigidbody rigid;

        public float hSpeed = 5;
        public float vSpeed = 2;

        public GameObject explosionPrefab;

        private CharacterManager agentCManager;
        private float lifeTime = 0f;

        void Start()
        {
            damageManager = DamageManager.Instance;
        }

        void Update()
        {
            lifeTime += Time.deltaTime;
            if (lifeTime > 10)
            {
                Destroy(this.gameObject);
            }
        }

        public void Init(CharacterManager cm)
        {
            agentCManager = cm;
            rigid = GetComponent<Rigidbody>();

            Vector3 targetForce = transform.forward * hSpeed;
            targetForce += transform.up * vSpeed;
            rigid.AddForce(targetForce, ForceMode.Impulse);
        }

        void OnTriggerEnter(Collider other)
        {
            if (lifeTime < .02f)
            {
                return;
            }

            CharacterManager targetCManager = other.transform.GetComponentInParent<CharacterManager>();
            if (targetCManager != null)
            {
                string targetId = targetCManager.networkIdentity.GetID();
                if (damagedCharactersId.Find(x => x == targetId) != null)
                {
                    return;
                }

                if (targetCManager.networkIdentity == null)
                {
                    return;
                }

                float damage = 50;
                damageManager.CreatateDamageEvent(agentCManager, targetCManager, damage);
                damagedCharactersId.Add(targetId);
            }

            GameObject go = Instantiate(explosionPrefab, transform.position, transform.rotation) as GameObject;
            Destroy(this.gameObject);
            Destroy(go, 1f);
        }
    }
}
