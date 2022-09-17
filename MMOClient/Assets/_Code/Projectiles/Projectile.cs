using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class Projectile : MonoBehaviour
    {
        private DamageManager damageManager;

        Rigidbody rigid;

        public float hSpeed = 5;
        public float vSpeed = 2;

        public GameObject explosionPrefab;

        private CharacterManager agentCManager;
        private float lifeTime = 0f;

        void Start()
        {
            damageManager = DamageManager.singleton;
        }

        void Update()
        {
            lifeTime += Time.deltaTime;
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

            EnemyManager em = other.GetComponentInParent<EnemyManager>();
            if (em != null)
            {
                CharacterManager targetCManager = other.transform.GetComponentInParent<CharacterManager>();
                float damage = 50;
                damageManager.CreatateDamageEvent(agentCManager, targetCManager, damage);
            }

            GameObject go = Instantiate(explosionPrefab, transform.position, transform.rotation) as GameObject;
            Destroy(this.gameObject);
            Destroy(go, 1f);
        }
    }
}
