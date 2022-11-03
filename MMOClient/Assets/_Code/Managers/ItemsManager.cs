using System.Collections;
using System.Collections.Generic;
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

        public static ItemsManager singleton;
        void Awake()
        {
            singleton = this;
        }

    }
}
