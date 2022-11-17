using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using SocketIO;
using System;

namespace BV
{
    public class ChestController : MenuPanel
    {
        private ManagersController managersController;
        private MenuManager menuManager;

        private GridManager gridManager;

        public override void Init(ManagersController mC, MenuManager mM)
        {
            managersController = mC;
            menuManager = mM;

            gridManager = GridManager.singleton;
        }

        public override void Open()
        {
            gridManager.SetData(managersController.playerData.inventoryData);
            gridManager.onUpdateData.AddListener(UpdateData);

            managersController.socket.Emit("openChest", new JSONObject(JsonUtility.ToJson(new ChestData(menuManager.currentChestId))));
        }

        public void SetChestData(InventoryGridData data)
        {
            List<InventoryGridData> gridData = new List<InventoryGridData>() { data };
            gridManager.SetData(gridData);
        }

        void UpdateData(InventoryGridData startGridData, InventoryGridData targetGridData, InventoryItem selectedItem)
        {
            if (startGridData != null)
            {
                if (startGridData.gridId == "chestGrid")
                {
                    UpdateChestData(startGridData);
                }
                else
                {
                    menuManager.UpdateInventoryData(startGridData);
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
                    menuManager.UpdateInventoryData(targetGridData);
                }
            }
        }

        void UpdateChestData(InventoryGridData itemGridData)
        {
            ChestData chestData = new ChestData(menuManager.currentChestId);
            chestData.items = itemGridData.items;

            managersController.socket.Emit("updateChestData", new JSONObject(JsonUtility.ToJson(chestData)));
        }

        public override void Deinit()
        {
            if (gridManager != null)
            {
                gridManager.onUpdateData.RemoveListener(UpdateData);
                gridManager.Deinit();

                if (!String.IsNullOrEmpty(menuManager.currentChestId))
                {
                    managersController.socket.Emit("closeChest", new JSONObject(JsonUtility.ToJson(new ChestData(menuManager.currentChestId))));
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