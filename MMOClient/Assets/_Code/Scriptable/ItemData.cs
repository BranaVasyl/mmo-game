using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData", order = 3)]
    public class ItemData : ScriptableObject
    {
        [Header("General Stats")]
        public string id;
        public string itemName;
        public string itemDescription;

        public int width = 1;
        public int height = 1;

        public ItemType itemType;
        public Sprite itemIcon;
        public GameObject weaponModel;
    }

    public enum ItemType
    {
        sword, shield, magicWeapon, armor, food, elixir, craft, alchemy, butter, other, quest
    }
}
