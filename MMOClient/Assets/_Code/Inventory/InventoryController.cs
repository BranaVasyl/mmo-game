using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using Project.Utility;
using UnityEngine;
using SocketIO;
using System;
using Project.Networking;
using System.Linq;

namespace BV
{
    public class InventoryController : MenuPanel
    {
        private SampleSceneManager managersController;
        private MenuManager menuManager;

        private ItemsManager itemsManager;
        private GridManager gridManager;

        private InventoryManager inventoryManager;

        public override void Init(SampleSceneManager mC, MenuManager mM)
        {
            singleton = this;

            managersController = mC;
            menuManager = mM;

            itemsManager = ItemsManager.singleton;
            gridManager = GridManager.singleton;
        }

        public void RegisterCharacterListener(InventoryManager iM)
        {
            inventoryManager = iM;
        }

        public override void Open()
        {
            gridManager.SetData(managersController.playerInventoryData);
            gridManager.SetData(managersController.playerEquipData);

            gridManager.onUpdateData.AddListener(UpdateData);
        }

        public void UpdateData(InventoryGridData startGridData, InventoryGridData targetGridData, InventoryItem selectedItem)
        {
            if (startGridData != null)
            {
                menuManager.UpdateInventoryData(startGridData);
                UpdatePlayerEquipData(startGridData);
            }

            if (targetGridData != null)
            {
                menuManager.UpdateInventoryData(targetGridData);
                UpdatePlayerEquipData(targetGridData);
            }
        }

        private void UpdatePlayerEquipData(InventoryGridData itemGridData)
        {
            int index = managersController.playerEquipData.FindIndex(s => s.gridId == itemGridData.gridId);
            if (index == -1)
            {
                return;
            }

            managersController.playerEquipData[index] = itemGridData;

            List<InventoryGridData> itemList = new List<InventoryGridData>();
            itemList.Add(itemGridData);

            inventoryManager.SetPlayerEquip(itemList);

            NetworkClient.Instance.Emit("syncPlayerEquipData", new JSONObject(JsonUtility.ToJson(new SendInventoryData(managersController.playerEquipData))));
        }

        public override void Deinit()
        {
            if (gridManager != null)
            {
                gridManager.onUpdateData.RemoveListener(UpdateData);
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
