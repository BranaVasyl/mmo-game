using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class WeaponHook : MonoBehaviour
    {
        public GameObject[] damageCollider;

        public void OpenDamageColliders()
        {
            for (int i = 0; i < damageCollider.Length; i++)
            {
                damageCollider[i].SetActive(true);
            }
        }

        public void CloseDamageColliders()
        {
            for (int i = 0; i < damageCollider.Length; i++)
            {
                damageCollider[i].GetComponent<DamageCollider>().Deinit();
                damageCollider[i].SetActive(false);
            }
        }
    }
}
