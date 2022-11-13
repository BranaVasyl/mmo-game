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
        public GameObject shopNameObject;

        private SocketIOComponent socket;
        private GridManager gridManager;
        private InventoryController inventoryController;

        [Header("Shop data")]
        private string shopId;
        private string shopName;

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
            gridManager.canUpdateGridCallback.Add(CanUpdateGridCallback);

            shopId = options.chestId;
            shopName = options.chestName;

            socket.Emit("openShop", new JSONObject(JsonUtility.ToJson(new ChestData(shopId))));
            shopNameObject.GetComponent<TMP_Text>().text = shopName;
        }

        private bool CanUpdateGridCallback(ItemGrid startGrid, ItemGrid targetGrid)
        {
            return true;
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
                if (startGridData.gridId == "shopGrid")
                {
                    UpdateShopData(startGridData);
                }
                else
                {
                    inventoryController.UpdateData(startGridData, null);
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
                    inventoryController.UpdateData(targetGridData, null);
                }
            }
        }

        void UpdateShopData(InventoryGridData itemGridData)
        {
            ChestData shopData = new ChestData(shopId);
            shopData.items = itemGridData.items;

            socket.Emit("updateShopData", new JSONObject(JsonUtility.ToJson(shopData)));
        }

        public override void Deinit()
        {
            if (gridManager != null)
            {
                gridManager.onUpdateData.RemoveListener(UpdateData);
                gridManager.Deinit();

                if (socket != null)
                {
                    socket.Emit("closeShop", new JSONObject(JsonUtility.ToJson(new ChestData(shopId))));
                }
            }

            shopId = "";
            shopName = "";
            shopNameObject.GetComponent<TMP_Text>().text = "";
        }

        public static ShopController singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}