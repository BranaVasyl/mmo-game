using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using Project.Utility;
using UnityEngine;
using SocketIO;
using System;
using Project.Networking;
using System.Linq;
using System.Threading.Tasks;

namespace BV
{
    public class InventoryController : MenuPanel
    {
        private MenuManager menuManager;
        private GridManager gridManager;

        private UnityEvent<List<InventoryGridData>> onUpdateEquip = new UnityEvent<List<InventoryGridData>>();


        public override void Init(MenuManager mM)
        {
            singleton = this;
            menuManager = mM;

            gridManager = GridManager.Instance;
        }

        public void SetUpdateEquipListener(UnityAction<List<InventoryGridData>> newListener)
        {
            onUpdateEquip.RemoveAllListeners();
            onUpdateEquip.AddListener(newListener);
        }

        public override void Open()
        {
            ApplicationManager.Instance.ShowSpinerLoader();
            NetworkRequestManager.Instance.EmitWithTimeout(
                new NetworkEvent(
                    "inventoryOpen",
                    null,
                    (response) =>
                        {
                            ApplicationManager.Instance.CloseSpinerLoader();

                            InventoryGridDataListWrapper gridDataWrapper = JsonUtility.FromJson<InventoryGridDataListWrapper>(response[0].ToString());
                            gridManager.SetData(gridDataWrapper.data);

                            gridManager.updateItemPositionCallback.Add(UpdateItemPositionCallback);
                        },
                    (msg) =>
                        {
                            ApplicationManager.Instance.CloseSpinerLoader();
                            ApplicationManager.Instance.ShowConfirmationModal("Не вдалося відкрити інвентар", () =>
                                {
                                    menuManager.CloseMenu();
                                });
                        }
                )
            );
        }

        private async Task<bool> UpdateItemPositionCallback(UpdateItemPositionData itemUpdateData)
        {
            bool requestStatus = false;
            bool result = false;

            NetworkRequestManager.Instance.EmitWithTimeout(
                new NetworkEvent(
                    "inventoryChange",
                    new JSONObject(JsonUtility.ToJson(itemUpdateData)),
                    (response) =>
                        {
                            result = response[0]["result"].ToString() == "true";
                            requestStatus = true;

                            InventoryGridDataListWrapper inventoryGridData = JsonUtility.FromJson<InventoryGridDataListWrapper>(response[0].ToString());
                            OnUpdateGrid(inventoryGridData.data);
                        },
                    (msg) =>
                        {
                            requestStatus = true;
                            ApplicationManager.Instance.ShowConfirmationModal("Не вдалося перенести предмет");
                        }
                )
            );

            while (!requestStatus)
            {
                await Task.Yield();
            }

            return result;
        }

        private void OnUpdateGrid(List<InventoryGridData> updatedGrids)
        {
            string[] equipGridList = gridManager.GetEquipGridList();
            List<InventoryGridData> equipGrids = new List<InventoryGridData>();

            foreach (var grid in updatedGrids)
            {
                if (equipGridList.Contains(grid.gridId))
                {
                    equipGrids.Add(grid);
                }
            }

            if (equipGrids.Count > 0)
            {
                onUpdateEquip.Invoke(equipGrids);
            }
        }

        public override void Deinit()
        {
            if (gridManager != null)
            {
                gridManager.Deinit();
                NetworkClient.Instance.Emit("inventoryClose");
            }
        }

        public static InventoryController singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
