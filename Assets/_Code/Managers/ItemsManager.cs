using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace BV
{
    public class ItemsManager : Singleton<ItemsManager>
    {
        public List<ItemData> allItems;

        private Dictionary<string, ItemData> itemsDict = new Dictionary<string, ItemData>();

        void Start()
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
