using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "ItemWeaponData", menuName = "Scriptable Objects/ItemWeaponData", order = 3)]
    public class ItemWeaponData : ItemData
    {
        [Header("Weapon Stats")]
        public List<Action> actions;
        public List<Action> two_handedActions;
        public GameObject weaponModel;
        [HideInInspector]
        public WeaponHook w_hook;
    }
}
