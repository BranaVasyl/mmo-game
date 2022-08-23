using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class EnemyInventoryManager : MonoBehaviour
    {
        public GameObject curWeapon;
        [HideInInspector]
        public WeaponHook w_hook;

        public void Init()
        {
            if (curWeapon == null)
            {
                return;
            }

            w_hook = curWeapon.GetComponent<WeaponHook>();
            w_hook.CloseDamageColliders();
        }
    }
}
