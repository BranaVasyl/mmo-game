using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BV
{
    [Serializable]
    public class PlayerData
    {
        public string id;
        public Vector3 position;
        public Quaternion rotation;
        public float vertical;
        public float horizontal;
        public bool isDead;
        public bool run;
        public bool walk;
        public bool isTwoHanded;
        public string currentAnimation;
        public List<InventoryGridData> inventoryData = new List<InventoryGridData>();
    }

    [Serializable]
    public class InventoryGridData
    {
        public string gridId;
        public List<InventoryItemData> items = new List<InventoryItemData>();

        public InventoryGridData(string id)
        {
            gridId = id;
        }
    }

    [Serializable]
    public class InventoryItemData
    {
        public string id;
        public Vector2Int position;

        public InventoryItemData(string i, int x, int y)
        {
            id = i;
            position = new Vector2Int(x, y);
        }
    }
}
