using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace BV
{
    public class ItemsManager : MonoBehaviour
    {
        public List<ItemData> allItems;
        public GameObject itemPrefab;

        public void Init()
        {
        }

        public ItemData? GetItemData(string itemId)
        {
            return allItems.Find(i => i.id == itemId);
        }

        public static ItemsManager singleton;
        void Awake()
        {
            singleton = this;
        }

    }
}
