using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData", order = 3)]
    public class ItemData : ScriptableObject
    {
        public string id;
        public int width = 1;
        public int height = 1;

        public ItemType itemType;
        public Sprite itemIcon;

    }

    public enum ItemType
    {
        sword, armor, food, elixir, craft, alchemy, butter, other, quest
    }
}
