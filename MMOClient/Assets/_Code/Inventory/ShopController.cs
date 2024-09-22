using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using Project.Utility;
using UnityEngine;
using SocketIO;
using System;
using TMPro;

namespace BV
{
    public class ShopController : MenuPanel
    {
        private SampleSceneManager managersController;
        private MenuManager menuManager;

        public GameObject shopNameObject;
        public GameObject shopMoneyObject;

        private GridManager gridManager;
        private InventoryController inventoryController;

        [Header("Shop data")]
        private string NPCId;
        private float shopMoney = 0;

        [Header("Player data")]
        private string playerId;
        private float playerMoney = 0;

        public override void Init(SampleSceneManager mC, MenuManager mM)
        {
            managersController = mC;
            menuManager = mM;

            gridManager = GridManager.singleton;
            inventoryController = InventoryController.singleton;
        }

        public override void Open()
        {
            gridManager.SetData(managersController.playerInventoryData);

            gridManager.onUpdateData.AddListener(UpdateData);
            gridManager.canUpdateGridCallback.Add(CanUpdateGridCallback);

            playerId = managersController.stateManager.networkIdentity.GetID();
            playerMoney = managersController.stateManager.money;

            if (menuManager.currentNPCStates == null)
            {
                return;
            }

            NPCId = menuManager.currentNPCStates.networkIdentity.GetID();
            shopNameObject.GetComponent<TMP_Text>().text = menuManager.currentNPCStates.displayedName;

            managersController.socket.Emit("openShop", new JSONObject(JsonUtility.ToJson(new ChestData(NPCId))));
        }

        private async Task<bool> CanUpdateGridCallback(ItemGrid startGrid, ItemGrid targetGrid, InventoryItem selectedItem, bool placeItemMode)
        {
            if (startGrid == null || targetGrid == null || startGrid.gridId == targetGrid.gridId)
            {
                return true;
            }

            if (startGrid.gridId != "shopGrid" && targetGrid.gridId != "shopGrid")
            {
                return true;
            }

            bool result = false;
            int operationType = startGrid.gridId == "shopGrid" ? 1 : 2;
            switch (operationType)
            {
                case 1:
                    result = playerMoney >= 50;
                    break;
                case 2:
                    result = shopMoney >= 50;
                    break;
            }

            if (placeItemMode && result)
            {
                result = await BuyItem(playerId, NPCId, selectedItem.GetItemId(), operationType);
            }

            return result;
        }

        private async Task<bool> BuyItem(string playerId, string NPCID, string itemId, int operationType)
        {
            bool requestStatus = false;
            bool result = false;

            SendTradeData sendData = new SendTradeData(playerId, NPCID, itemId, operationType);
            managersController.socket.Emit("shopTrade", new JSONObject(JsonUtility.ToJson(sendData)), (response) =>
            {
                var data = response[0];
                result = data["result"].ToString() == "true";

                if (result)
                {
                    playerMoney = data["playerMoney"].JSONObjectToFloat();
                    shopMoney = data["shopMoney"].JSONObjectToFloat();

                    menuManager.RenderMoney(playerMoney);
                    RenderMoney(shopMoney);
                }

                requestStatus = true;
            });

            while (!requestStatus)
            {
                await Task.Yield();
            }

            return result;
        }

        public void SetShopData(InventoryGridData data, float money)
        {
            if (data.items.Count != 0)
            {
                List<InventoryGridData> gridData = new List<InventoryGridData>() { data };
                gridManager.SetData(gridData);
            }

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

            playerId = "";
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