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
        public string name = "";
        public string gender = "man";
        public string race = "human";
        public string characterClass = "warrior";
        public string alliance = "alliance1";
        public CharacterCustomizationData customization = new CharacterCustomizationData();
        public List<InventoryGridData> playerEquipData = new List<InventoryGridData>();

        //todo need add to base
        public bool isTwoHanded;
        public float money;

        public float health;
        public Vector3 position;
        public Quaternion rotation;
        public bool isDead;
        public float vertical;
        public float horizontal;
        public bool run;
        public bool walk;
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
        public ItemDetails item;

        public InventoryItemData(string i, int x, int y, bool r, string itemCode)
        {
            id = i;
            position = new Vector2Int(x, y);
            rotated = r;
            item = new ItemDetails(itemCode);
        }
    }

    [Serializable]
    public class ItemDetails
    {
        public string code = "";

        public ItemDetails(string code = "")
        {
            this.code = code;
        }
    }

    [Serializable]
    public class InventoryGridDataListWrapper
    {
        public List<InventoryGridData> data;
    }
}
