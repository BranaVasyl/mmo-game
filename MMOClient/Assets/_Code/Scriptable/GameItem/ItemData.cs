using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/Game Item/Item", order = 2)]
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
    }

    public enum ItemType
    {
        weapon, spell, armor, food, elixir, craft, alchemy, butter, other, quest
    }
}
