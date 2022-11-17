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
        private ManagersController managersController;
        private MenuManager menuManager;

        public GameObject shopNameObject;
        public GameObject shopMoneyObject;

        private GridManager gridManager;
        private InventoryController inventoryController;

        [Header("Shop data")]
        private string NPCId;
        private float shopMoney = 0;

        [Header("Player data")]
        private float playerMoney = 0;

        public override void Init(ManagersController mC, MenuManager mM)
        {
            managersController = mC;
            menuManager = mM;

            gridManager = GridManager.singleton;
            inventoryController = InventoryController.singleton;
        }

        public override void Open()
        {
            gridManager.SetData(managersController.playerData.inventoryData);

            gridManager.onUpdateData.AddListener(UpdateData);
            gridManager.canUpdateGridCallback.Add(CanUpdateGridCallback);

            playerMoney = managersController.stateManager.money;

            if (menuManager.currentNPCStates == null)
            {
                return;
            }

            NPCId = menuManager.currentNPCStates.networkIdentity.GetID();

            shopNameObject.GetComponent<TMP_Text>().text = menuManager.currentNPCStates.displayedName;
            managersController.socket.Emit("openShop", new JSONObject(JsonUtility.ToJson(new ChestData(NPCId))));
        }

        private bool CanUpdateGridCallback(ItemGrid startGrid, ItemGrid targetGrid, InventoryItem selectedItem)
        {
            if (startGrid == null || targetGrid == null || startGrid.gridId == targetGrid.gridId)
            {
                return true;
            }

            if (startGrid.gridId == "shopGrid")
            {
                if (playerMoney >= 50)
                {
                    return true;
                }
            }

            if (targetGrid.gridId == "shopGrid")
            {
                if (shopMoney >= 50)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetShopData(InventoryGridData data, float money)
        {
            List<InventoryGridData> gridData = new List<InventoryGridData>() { data };
            gridManager.SetData(gridData);

            shopMoney = money;
            RenderMoney(money);
        }

        void UpdateData(InventoryGridData startGridData, InventoryGridData targetGridData, InventoryItem selectedItem)
        {
            if (startGridData != null)
            {
                if (startGridData.gridId == "shopGrid")
                {
                    UpdateShopData(startGridData);
                }
                else
                {
                    menuManager.UpdateInventoryData(startGridData);
                }
            }

            if (targetGridData != null)
            {
                if (targetGridData.gridId == "shopGrid")
                {
                    UpdateShopData(targetGridData);
                }
                else
                {
                    menuManager.UpdateInventoryData(targetGridData);
                }
            }

            CalculatedMoney(startGridData, targetGridData, selectedItem);
        }

        //@todo rebuild this
        private void CalculatedMoney(InventoryGridData startGridData, InventoryGridData targetGridData, InventoryItem selectedItem)
        {
            if (startGridData != null && targetGridData != null)
            {
                if (startGridData.gridId == "shopGrid")
                {
                    if (selectedItem != null)
                    {
                        Debug.Log("Buy item : " + selectedItem.itemData.id);

                        playerMoney -= 50;
                        managersController.socket.Emit("updateMoneyCount", new JSONObject(JsonUtility.ToJson(
                            new SendUpdateMoneyData(managersController.stateManager.networkIdentity.GetID(), -50)
                        )));

                        shopMoney += 50;
                        managersController.socket.Emit("updateMoneyCount", new JSONObject(JsonUtility.ToJson(
                            new SendUpdateMoneyData(menuManager.currentNPCStates.networkIdentity.GetID(), 50)
                        )));

                        menuManager.RenderMoney(playerMoney);
                        RenderMoney(shopMoney);
                    }
                }

                if (targetGridData.gridId == "shopGrid")
                {
                    if (selectedItem != null)
                    {
                        Debug.Log("Sell item : " + selectedItem.itemData.id);

                        playerMoney += 50;
                        managersController.socket.Emit("updateMoneyCount", new JSONObject(JsonUtility.ToJson(
                            new SendUpdateMoneyData(managersController.stateManager.networkIdentity.GetID(), 50)
                        )));

                        shopMoney -= 50;
                        managersController.socket.Emit("updateMoneyCount", new JSONObject(JsonUtility.ToJson(
                            new SendUpdateMoneyData(menuManager.currentNPCStates.networkIdentity.GetID(), -50)
                        )));

                        menuManager.RenderMoney(playerMoney);
                        RenderMoney(shopMoney);
                    }
                }
            }
        }

        void UpdateShopData(InventoryGridData itemGridData)
        {
            ChestData shopData = new ChestData(NPCId);
            shopData.items = itemGridData.items;

            managersController.socket.Emit("updateShopData", new JSONObject(JsonUtility.ToJson(shopData)));
        }

        public override void Deinit()
        {
            if (gridManager != null)
            {
                gridManager.onUpdateData.RemoveListener(UpdateData);
                gridManager.Deinit();

                if (!String.IsNullOrEmpty(NPCId))
                {
                    managersController.socket.Emit("closeShop", new JSONObject(JsonUtility.ToJson(new ChestData(NPCId))));
                }
            }

            NPCId = "";
            shopMoney = 0;
            playerMoney = 0;

            shopNameObject.GetComponent<TMP_Text>().text = "";
            shopMoneyObject.GetComponent<TMP_Text>().text = "";
        }

        public void RenderMoney(float moneyCount)
        {
            shopMoneyObject.GetComponent<TMP_Text>().text = moneyCount.ToString();
        }

        public static ShopController singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}