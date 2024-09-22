using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using SocketIO;
using System;

namespace BV
{
    public class ChestController : MenuPanel
    {
        private SampleSceneManager managersController;
        private MenuManager menuManager;

        private GridManager gridManager;

        [Header("Player data")]
        private string playerId;

        public override void Init(SampleSceneManager mC, MenuManager mM)
        {
            managersController = mC;
            menuManager = mM;

            gridManager = GridManager.singleton;
        }

        public override void Open()
        {
            gridManager.SetData(managersController.playerInventoryData);
            gridManager.onUpdateData.AddListener(UpdateData);
            gridManager.canUpdateGridCallback.Add(CanUpdateGridCallback);

            playerId = managersController.stateManager.networkIdentity.GetID();

            managersController.socket.Emit("openChest", new JSONObject(JsonUtility.ToJson(new ChestData(menuManager.currentChestId))));
        }

        private async Task<bool> CanUpdateGridCallback(ItemGrid startGrid, ItemGrid targetGrid, InventoryItem selectedItem, bool placeItemMode)
        {
            if (startGrid == null || targetGrid == null || startGrid.gridId == targetGrid.gridId)
            {
                return true;
            }

            if (startGrid.gridId != "chestGrid" && targetGrid.gridId != "chestGrid")
            {
                return true;
            }

            if (!placeItemMode)
            {
                return true;
            }

            bool result = false;
            int operationType = startGrid.gridId == "chestGrid" ? 1 : 2;
            result = await PickUpItem(playerId, menuManager.currentChestId, selectedItem.GetItemId(), operationType);

            return result;
        }

        private async Task<bool> PickUpItem(string playerId, string chestID, string itemId, int operationType)
        {
            bool requestStatus = false;
            bool result = false;

            SendChestPickUpData sendData = new SendChestPickUpData(playerId, chestID, itemId, operationType);
            managersController.socket.Emit("chestPickUp", new JSONObject(JsonUtility.ToJson(sendData)), (response) =>
            {
                var data = response[0];
                result = data["result"].ToString() == "true";

                requestStatus = true;
            });

            while (!requestStatus)
            {
                await Task.Yield();
            }

            return result;
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