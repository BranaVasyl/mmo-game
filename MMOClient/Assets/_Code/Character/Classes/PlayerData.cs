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
        public CharacterData characterData;

        public float health;
        public float money;
        public Vector3 position;
        public Quaternion rotation;
        public float vertical;
        public float horizontal;
        public bool isDead;
        public bool run;
        public bool walk;
        public bool isTwoHanded;
        public List<InventoryGridData> inventoryData = new List<InventoryGridData>();
        public List<InventoryGridData> playerEquipData = new List<InventoryGridData>();
    }

    [Serializable]
    public class InventoryGridData
    {
        public string gridId;
        public Vector2Int gridSize;
        public List<ItemType> supportedItemType;

        public List<InventoryItemData> items = new List<InventoryItemData>();

        public InventoryGridData(string id, Vector2Int gS, List<ItemType> sT)
        {
            gridId = id;
            gridSize = gS;
            supportedItemType = sT;
        }
    }

    [Serializable]
    public class InventoryItemData
    {
        public string id;
        public Vector2Int position;
        public bool rotated = false;

        public InventoryItemData(string i, int x, int y, bool r)
        {
            id = i;
            position = new Vector2Int(x, y);
            rotated = r;
        }
    }
}
