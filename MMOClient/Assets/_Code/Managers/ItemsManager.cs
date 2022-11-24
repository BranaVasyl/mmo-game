using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace BV
{
    public class ItemsManager : MonoBehaviour
    {
        public List<ItemData> allItems;
        public GameObject itemPrefab;

        private Dictionary<string, ItemData> itemsDict = new Dictionary<string, ItemData>();

        public void Init()
        {
            foreach (ItemData item in allItems)
            {
                itemsDict[item.id] = item;
            }
        }

        public int GetItemsCount()
        {
            return allItems.Count;
        }

        public ItemData? GetItemByIndex(int index)
        {
            return allItems[index];
        }

        public ItemData? GetItemById(string itemId)
        {
            return itemsDict[itemId];
        }

        public static ItemsManager singleton;
        void Awake()
        {
            singleton = this;
        }

    }
}
