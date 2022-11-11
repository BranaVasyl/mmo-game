using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;

namespace BV
{
    public class ChestController : MenuPanel
    {
        private SocketIOComponent socket;
        private GridManager gridManager;
        private InventoryController inventoryController;

        //@todo just example canUpdate grid
        private int itemCountUpdate = 0;

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

            socket.Emit("openChest", new JSONObject(JsonUtility.ToJson(new ChestData("1"))));
        }

        //@todo just example canUpdate grid
        private bool CanUpdateGridCallback(ItemGrid startGrid, ItemGrid targetGrid)
        {
            if (itemCountUpdate >= 3)
            {
                return false;
            }

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
                if (startGrid.gridId == "chestGrid")
                {
                    UpdateChestData(startGrid);
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
                if (targetGrid.gridId == "chestGrid")
                {
                    UpdateChestData(targetGrid);
                }
                else
                {
                    inventoryController.UpdateData(targetGrid, null);
                }
            }

            //@todo just example canUpdate grid
            if ((startGrid.gridId == "chestGrid" && targetGrid != null) || (startGrid != null && targetGrid.gridId == "chestGrid"))
            {
                itemCountUpdate++;
            }
        }

        void UpdateChestData(ItemGrid itemGrid)
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

            socket.Emit("updateChestData", new JSONObject(JsonUtility.ToJson(chestData)));
        }

        public override void Deinit()
        {
            itemCountUpdate = 0;

            if (gridManager != null)
            {
                gridManager.onUpdateData.RemoveListener(UpdateData);
                gridManager.Deinit();

                if (socket != null)
                {
                    socket.Emit("closeChest", new JSONObject(JsonUtility.ToJson(new ChestData("1"))));
                }
            }
        }

        public static ChestController singleton;
        void Awake()
        {
            singleton = this;
        }
    }

    [Serializable]
    public class ChestData
    {
        public string id = "";
        public List<InventoryItemData> items = new List<InventoryItemData>();

        public ChestData(string data)
        {
            id = data;
        }
    }
}