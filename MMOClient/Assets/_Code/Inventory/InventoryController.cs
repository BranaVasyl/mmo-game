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
        private SampleSceneManager managersController;
        private MenuManager menuManager;

        private ItemsManager itemsManager;
        private GridManager gridManager;

        private UnityEvent<List<InventoryGridData>> onUpdateEquip = new UnityEvent<List<InventoryGridData>>();


        public override void Init(SampleSceneManager mC, MenuManager mM)
        {
            singleton = this;

            managersController = mC;
            menuManager = mM;

            itemsManager = ItemsManager.singleton;
            gridManager = GridManager.singleton;
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
                "openInventory",
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
            );
        }

        private async Task<bool> UpdateItemPositionCallback(ItemGrid startGrid, ItemGrid targetGrid, InventoryItem selectedItem, Vector2Int position)
        {
            bool requestStatus = false;
            bool result = false;

            UpdateItemPositionData sendData = new UpdateItemPositionData(startGrid.gridId, targetGrid.gridId, selectedItem.id, position, selectedItem.rotated);
            NetworkRequestManager.Instance.EmitWithTimeout(
                "inventoryChange",
                new JSONObject(JsonUtility.ToJson(sendData)),
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
                        ApplicationManager.Instance.CloseSpinerLoader();
                        ApplicationManager.Instance.ShowConfirmationModal("Не вдалося купити предмет");
                    }
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
            }
        }

        public static InventoryController singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
