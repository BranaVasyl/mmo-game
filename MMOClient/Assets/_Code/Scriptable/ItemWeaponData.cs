using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "ItemWeaponData", menuName = "Scriptable Objects/ItemWeaponData", order = 3)]
    public class ItemWeaponData : ItemData
    {
        public string oh_idle_name;
        public string th_idle_name;

        [Header("Weapon Stats")]
        public List<Action> actions;
        public bool LeftHandMirror = true;
        public List<Action> two_handedActions;
        public GameObject weaponModel;

        public Action GetAction(List<Action> l, ActionInput inp)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].input == inp)
                {
                    return l[i];
                }
            }

            return null;
        }
    }
}
