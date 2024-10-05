using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using SocketIO;
using System;
using Project.Networking;

namespace BV
{
    public class ChestController : MenuPanel
    {
        private SampleSceneManager managersController;
        private MenuManager menuManager;
        private GridManager gridManager;

        public string currentChestId = "";

        public override void Init(SampleSceneManager mC, MenuManager mM)
        {
            singleton = this;

            managersController = mC;
            menuManager = mM;

            gridManager = GridManager.singleton;
        }

        public override void Open()
        {
            if (string.IsNullOrEmpty(currentChestId))
            {
                return;
            }

            JSONObject chestData = new();
            chestData.AddField("id", currentChestId);

            ApplicationManager.Instance.ShowSpinerLoader();
            NetworkRequestManager.Instance.EmitWithTimeout(
                "openChest",
                chestData,
                (response) =>
                    {
                        ApplicationManager.Instance.CloseSpinerLoader();

                        gridManager.SetData(managersController.playerInventoryData);
                        gridManager.onUpdateData.AddListener(UpdateData);
                        gridManager.canUpdateGridCallback.Add(CanUpdateGridCallback);

                        InventoryGridDataListWrapper gridDataWrapper = JsonUtility.FromJson<InventoryGridDataListWrapper>(response[0].ToString());
                        SetChestData(gridDataWrapper.data);
                    },
                (msg) =>
                    {
                        ApplicationManager.Instance.CloseSpinerLoader();
                        ApplicationManager.Instance.ShowConfirmationModal("Не вдалося відкрити сундук", () =>
                            {
                                menuManager.CloseMenu();
                            });
                    }
            );
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
            result = await PickUpItem(currentChestId, selectedItem.id, operationType);

            return result;
        }

        private async Task<bool> PickUpItem(string chestID, string itemId, int operationType)
        {
            bool requestStatus = false;
            bool result = false;

            SendChestPickUpData sendData = new SendChestPickUpData(NetworkClient.SessionID, chestID, itemId, operationType);
            NetworkRequestManager.Instance.EmitWithTimeout(
                "chestPickUp",
                new JSONObject(JsonUtility.ToJson(sendData)),
                (response) =>
                    {
                        result = response[0]["result"].ToString() == "true";
                        requestStatus = true;
                    },
                (msg) =>
                    {
                        requestStatus = true;
                        ApplicationManager.Instance.CloseSpinerLoader();
                        ApplicationManager.Instance.ShowConfirmationModal("Не вдалося підібрати передмет");
                    }
            );

            while (!requestStatus)
            {
                await Task.Yield();
            }

            return result;
        }

        public void SetChestData(List<InventoryGridData> data)
        {
            gridManager.SetData(data);
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
            ChestData chestData = new ChestData(currentChestId);
            chestData.items.Add(itemGridData);

            NetworkClient.Instance.Emit("updateChestData", new JSONObject(JsonUtility.ToJson(chestData)));
        }

        public override void Deinit()
        {
            currentChestId = "";
            if (gridManager != null)
            {
                gridManager.onUpdateData.RemoveListener(UpdateData);
                gridManager.Deinit();

                if (!String.IsNullOrEmpty(currentChestId))
                {
                    NetworkClient.Instance.Emit("closeChest", new JSONObject(JsonUtility.ToJson(new ChestData(currentChestId))));
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
        public List<InventoryGridData> items = new List<InventoryGridData>();

        public ChestData(string data)
        {
            id = data;
        }
    }
}