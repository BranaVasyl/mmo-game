using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using SocketIO;
using System;
using TMPro;

namespace BV
{
    public class ShopController : MenuPanel
    {
        public GameObject shopName;

        private SocketIOComponent socket;
        private GridManager gridManager;
        private InventoryController inventoryController;

        public override void Init(SocketIOComponent soc, PlayerData playerData)
        {
            socket = soc;
            gridManager = GridManager.singleton;
            inventoryController = InventoryController.singleton;
        }

        public override void Open()
        {
            gridManager.SetData(inventoryController.inventoryData);

            gridManager.onUpdateData.AddListener(UpdateData);
            gridManager.canUpdateGridCallback.Add(CanUpdateGridCallback);

            socket.Emit("openShop", new JSONObject(JsonUtility.ToJson(new ChestData("1"))));
        }

        public void SetPlayerData(string npcName)
        {
            shopName.GetComponent<TMP_Text>().text = npcName;
        }

        private bool CanUpdateGridCallback(ItemGrid startGrid, ItemGrid targetGrid)
        {
            return true;
        }

        public void SetChestData(InventoryGridData data)
        {
            List<InventoryGridData> gridData = new List<InventoryGridData>() { data };
            gridManager.SetData(gridData);
        }

        void UpdateData(ItemGrid startGrid, ItemGrid targetGrid)
        {
            if (startGrid != null)
            {
                if (startGrid.gridId == "shopGrid")
                {
                    UpdateShopData(startGrid);
                }
                else
                {
                    inventoryController.UpdateData(startGrid, null);
                }
            }

            if (startGrid != null && targetGrid != null && startGrid.gridId == targetGrid.gridId)
            {
                return;
            }

            if (targetGrid != null)
            {
                if (targetGrid.gridId == "shopGrid")
                {
                    UpdateShopData(targetGrid);
                }
                else
                {
                    inventoryController.UpdateData(targetGrid, null);
                }
            }
        }

        void UpdateShopData(ItemGrid itemGrid)
        {
            List<InventoryItem> checkedItem = new List<InventoryItem>();

            ChestData chestData = new ChestData("1");
            InventoryItem[,] inventoryItem = itemGrid.inventoryItemSlot;
            for (int i = 0; i < inventoryItem.GetLength(0); i++)
            {
                for (int j = 0; j < inventoryItem.GetLength(1); j++)
                {
                    if (inventoryItem[i, j] != null)
                    {
                        InventoryItem curInventoryItem = inventoryItem[i, j];
                        if (checkedItem.Find(x => x == curInventoryItem) != null)
                        {
                            continue;
                        }

                        chestData.items.Add(new InventoryItemData(curInventoryItem.itemData.id, curInventoryItem.onGridPositionX, curInventoryItem.onGridPositionY, curInventoryItem.rotated));
                        checkedItem.Add(curInventoryItem);
                    }
                }
            }

            socket.Emit("updateShopData", new JSONObject(JsonUtility.ToJson(chestData)));
        }

        public override void Deinit()
        {
            shopName.GetComponent<TMP_Text>().text = "";

            if (gridManager != null)
            {
                gridManager.onUpdateData.RemoveListener(UpdateData);
                gridManager.Deinit();

                if (socket != null)
                {
                    socket.Emit("closeShop", new JSONObject(JsonUtility.ToJson(new ChestData("1"))));
                }
            }
        }

        public static ShopController singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}