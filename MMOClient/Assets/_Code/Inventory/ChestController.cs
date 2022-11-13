using System.Collections.Generic;
using System.Collections;
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

        [Header("Chest data")]
        private string chestId;

        public override void Init(SocketIOComponent soc, PlayerData playerData)
        {
            socket = soc;
            gridManager = GridManager.singleton;
            inventoryController = InventoryController.singleton;
        }

        public override void Open(MenuManagerOptions options)
        {
            gridManager.SetData(inventoryController.inventoryData);
            gridManager.onUpdateData.AddListener(UpdateData);

            chestId = options.chestId;
            socket.Emit("openChest", new JSONObject(JsonUtility.ToJson(new ChestData(chestId))));
        }

        public void SetChestData(InventoryGridData data)
        {
            List<InventoryGridData> gridData = new List<InventoryGridData>() { data };
            gridManager.SetData(gridData);
        }

        void UpdateData(InventoryGridData startGridData, InventoryGridData targetGridData)
        {
            if (startGridData != null)
            {
                if (startGridData.gridId == "chestGrid")
                {
                    UpdateChestData(startGridData);
                }
                else
                {
                    inventoryController.UpdateData(startGridData, null);
                }
            }

            if (targetGridData != null)
            {
                if (targetGridData.gridId == "chestGrid")
                {
                    UpdateChestData(targetGridData);
                }
                else
                {
                    inventoryController.UpdateData(targetGridData, null);
                }
            }
        }

        void UpdateChestData(InventoryGridData itemGridData)
        {
            ChestData chestData = new ChestData(chestId);
            chestData.items = itemGridData.items;

            socket.Emit("updateChestData", new JSONObject(JsonUtility.ToJson(chestData)));
        }

        public override void Deinit()
        {
            if (gridManager != null)
            {
                gridManager.onUpdateData.RemoveListener(UpdateData);
                gridManager.Deinit();

                if (socket != null)
                {
                    socket.Emit("closeChest", new JSONObject(JsonUtility.ToJson(new ChestData(chestId))));
                }
            }

            chestId = "";
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