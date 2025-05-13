using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using SocketIO;
using System;
using Project.Networking;
using System.Linq;

namespace BV
{
    public class ChestController : MenuPanel
    {
        private MenuManager menuManager;
        private GridManager gridManager;

        public string currentChestId = "";

        public override void Init(SampleSceneManager mC, MenuManager mM)
        {
            singleton = this;
            menuManager = mM;

            gridManager = GridManager.singleton;
        }

        public override void Open()
        {
            if (string.IsNullOrEmpty(currentChestId))
            {
                return;
            }

            List<NetworkEvent> events = new List<NetworkEvent>();

            JSONObject chestData = new();
            chestData.AddField("chestId", currentChestId);

            events.Add(
                new NetworkEvent(
                    "chestOpen",
                    chestData,
                    (response) =>
                        {
                            InventoryGridDataListWrapper gridDataWrapper = JsonUtility.FromJson<InventoryGridDataListWrapper>(response[0].ToString());
                            SetChestData(gridDataWrapper.data);
                        }
                )
            );

            events.Add(
                new NetworkEvent(
                    "inventoryOpen",
                    null,
                    (response) =>
                        {
                            InventoryGridDataListWrapper gridDataWrapper = JsonUtility.FromJson<InventoryGridDataListWrapper>(response[0].ToString());
                            gridManager.SetData(gridDataWrapper.data);
                        }
                )
            );

            ApplicationManager.Instance.ShowSpinerLoader();
            NetworkRequestManager.Instance.EmitWithTimeoutAll(
                events,
                () =>
                    {
                        ApplicationManager.Instance.CloseSpinerLoader();
                        gridManager.updateItemPositionCallback.Add(UpdateItemPositionCallback);
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

        private async Task<bool> UpdateItemPositionCallback(ItemGrid startGrid, ItemGrid targetGrid, InventoryItem selectedItem, Vector2Int position)
        {
            bool requestStatus = false;
            bool result = false;

            UpdateItemPositionData itemPosition = new UpdateItemPositionData(startGrid.gridId, targetGrid.gridId, selectedItem.id, position, selectedItem.rotated);
            JSONObject sendData = new JSONObject(JsonUtility.ToJson(itemPosition));

            sendData.AddField("chestId", currentChestId);

            NetworkRequestManager.Instance.EmitWithTimeout(
                new NetworkEvent(
                    "chestChange",
                    sendData,
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
                )
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

        public override void Deinit()
        {
            if (gridManager != null)
            {
                gridManager.Deinit();

                JSONObject chestData = new();
                chestData.AddField("chestId", currentChestId);
                NetworkClient.Instance.Emit("chestClose", chestData);

                NetworkClient.Instance.Emit("inventoryClose");
            }

            currentChestId = "";
        }

        public static ChestController singleton;
        void Awake()
        {
            singleton = this;
        }
    }

    [Serializable]
    public class UpdateItemPositionData
    {
        public string startGrid;
        public string targetGrid;
        public string itemId;
        public Vector2Int position;
        public bool rotated;

        public UpdateItemPositionData(string sG, string tG, string iI, Vector2Int p, bool r)
        {
            startGrid = sG;
            targetGrid = tG;
            itemId = iI;
            position = p;
            rotated = r;
        }
    }
}