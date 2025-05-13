using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using Project.Utility;
using UnityEngine;
using SocketIO;
using System;
using TMPro;
using Project.Networking;

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
        public string characterId;
        public string characterName;
        private float shopMoney = 0;
        private float playerMoney = 0;

        public override void Init(SampleSceneManager mC, MenuManager mM)
        {
            singleton = this;

            managersController = mC;
            menuManager = mM;

            gridManager = GridManager.singleton;
            inventoryController = InventoryController.singleton;
        }

        public override void Open()
        {
            if (string.IsNullOrEmpty(characterId))
            {
                return;
            }

            shopNameObject.GetComponent<TMP_Text>().text = characterName;

            JSONObject shopData = new();
            shopData.AddField("id", characterId);

            List<NetworkEvent> events = new List<NetworkEvent>();

            events.Add(
                new NetworkEvent(
                    "shopOpen",
                    shopData,
                    (response) =>
                        {
                            InventoryGridDataListWrapper gridDataWrapper = JsonUtility.FromJson<InventoryGridDataListWrapper>(response[0].ToString());
                            float money = response[0]["money"].JSONObjectToFloat();
                            SetShopData(gridDataWrapper.data, money);
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
                            playerMoney = response[0]["money"].JSONObjectToFloat();
                            menuManager.RenderMoney(playerMoney);

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

                        gridManager.canPlaceItemCallback.Add(CanPlaceItemCallback);
                        gridManager.updateItemPositionCallback.Add(UpdateItemPositionCallback);
                    },
                (msg) =>
                    {
                        ApplicationManager.Instance.CloseSpinerLoader();
                        ApplicationManager.Instance.ShowConfirmationModal("Не вдалося відкрити магазин", () =>
                            {
                                menuManager.CloseMenu();
                            });
                    }
            );
        }

        private bool CanPlaceItemCallback(ItemGrid startGrid, ItemGrid targetGrid, InventoryItem selectedItem)
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

            return result;
        }

        private async Task<bool> UpdateItemPositionCallback(ItemGrid startGrid, ItemGrid targetGrid, InventoryItem selectedItem, Vector2Int position)
        {
            bool requestStatus = false;
            bool result = false;

            UpdateItemPositionData itemPosition = new UpdateItemPositionData(startGrid.gridId, targetGrid.gridId, selectedItem.id, position, selectedItem.rotated);
            JSONObject sendData = new JSONObject(JsonUtility.ToJson(itemPosition));

            sendData.AddField("characterId", characterId);

            NetworkRequestManager.Instance.EmitWithTimeout(
                new NetworkEvent(
                    "shopChange",
                    sendData,
                    (response) =>
                        {
                            result = response[0]["result"].ToString() == "true";
                            requestStatus = true;

                            if (result)
                            {
                                playerMoney = response[0]["playerMoney"].JSONObjectToFloat();
                                menuManager.RenderMoney(playerMoney);

                                shopMoney = response[0]["shopMoney"].JSONObjectToFloat();
                                RenderMoney(shopMoney);
                            }
                        },
                    (msg) =>
                        {
                            requestStatus = true;
                            ApplicationManager.Instance.CloseSpinerLoader();
                            ApplicationManager.Instance.ShowConfirmationModal("Не вдалося купити предмет");
                        }
                )
            );

            while (!requestStatus)
            {
                await Task.Yield();
            }

            return result;
        }

        public void SetShopData(List<InventoryGridData> data, float money)
        {
            gridManager.SetData(data);

            shopMoney = money;
            RenderMoney(money);
        }

        public override void Deinit()
        {
            if (gridManager != null)
            {
                gridManager.Deinit();

                JSONObject shopData = new();
                shopData.AddField("characterId", characterId);
                NetworkClient.Instance.Emit("shopClose", shopData);

                NetworkClient.Instance.Emit("inventoryClose");
            }

            characterId = "";
            characterName = "";
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