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
        public string name;
        public string description;
        public float mass = -1;
        public float price = -1;

        [Header("Inventory Stats")]
        public int width = 1;
        public int height = 1;

        public ItemType type;
        public Sprite icon;
        public Sprite smallIcon;
    }

    public enum ItemType
    {
        weapon, spell, armor, food, elixir, craft, alchemy, butter, other, quest
    }
}
