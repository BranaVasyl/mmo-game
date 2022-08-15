using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData", order = 3)]
    public class ItemData : ScriptableObject
    {
        public int width = 1;
        public int height = 1;

        public Sprite itemIcon;
    }
}
