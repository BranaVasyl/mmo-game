using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using Project.Utility;
using UnityEngine;
using SocketIO;
using System;

namespace BV
{
    public class InventoryController : MenuPanel
    {
        private SocketIOComponent socket;
        private ItemsManager itemsManager;
        private GridManager gridManager;

        [HideInInspector]
        public List<InventoryGridData> inventoryData;
        private InventoryManager inventoryManager;

        public static InventoryController singleton;
        public override void Init(SocketIOComponent soc, PlayerData playerData)
        {
            singleton = this;

            socket = soc;
            inventoryData = playerData.inventoryData;

            itemsManager = ItemsManager.singleton;
            gridManager = GridManager.singleton;
        }

        public void RegisterCharacterListener(CharacterManager characterMan)
        {
            inventoryManager = characterMan.gameObject.GetComponent<InventoryManager>();

            if (inventoryData.Count == 0)
            {
                return;
            }

            UpdateLeftHand();
            UpdateRightHand();
            UpdateQuickSpell(0);
            UpdateQuickSpell(1);
            UpdateQuickSpell(2);
            UpdateQuickSpell(3);
        }

        public override void Open(MenuManagerOptions options)
        {
            gridManager.SetData(inventoryData);
            gridManager.onUpdateData.AddListener(UpdateData);
        }

        public void UpdateData(InventoryGridData startGridData, InventoryGridData targetGridData)
        {
            if (startGridData != null)
            {
                UpdateInventoryData(startGridData);
                UpdateEquip(startGridData);
            }

            if (targetGridData != null)
            {
                UpdateInventoryData(targetGridData);
                UpdateEquip(targetGridData);
            }
        }

        #region Update Equip
        private void UpdateLeftHand()
        {
            InventoryGridData inventoryGrid = inventoryData.Find(x => x.gridId == "leftHandGrid");
            if (inventoryGrid == null)
            {
                return;
            }

            List<InventoryItemData> items = inventoryGrid.items;
            ItemWeaponData item = null;
            if (items.Count > 0)
            {
                string itemId = items[0].id;
                item = itemsManager.allItems.Find(x => x.id == itemId) as ItemWeaponData;
            }

            inventoryManager.UpdateLeftHand(item);
        }

        private void UpdateRightHand()
        {
            InventoryGridData inventoryGrid = inventoryData.Find(x => x.gridId == "rightHandGrid");
            if (inventoryGrid == null)
            {
                return;
            }

            List<InventoryItemData> items = inventoryGrid.items;
            ItemWeaponData item = null;
            if (items.Count > 0)
            {
                string itemId = items[0].id;
                item = itemsManager.allItems.Find(x => x.id == itemId) as ItemWeaponData;
            }

            inventoryManager.UpdateRightHand(item);
        }

        private void UpdateQuickSpell(int id)
        {
            InventoryGridData inventoryGrid = inventoryData.Find(x => x.gridId == "quickSpellGrid" + (id + 1));
            if (inventoryGrid == null)
            {
                return;
            }

            List<InventoryItemData> items = inventoryGrid.items;
            Spell item = null;
            if (items.Count > 0)
            {
                string itemId = items[0].id;
                item = itemsManager.allItems.Find(x => x.id == itemId) as Spell;
            }

            inventoryManager.UpdateQuickSpell(id, item);
        }
        #endregion
        private void UpdateEquip(InventoryGridData itemGridData)
        {
            if (itemGridData == null)
            {
                return;
            }

            switch (itemGridData.gridId)
            {
                case "leftHandGrid":
                    UpdateLeftHand();
                    break;
                case "rightHandGrid":
                    UpdateRightHand();
                    break;
                case "quickSpellGrid1":
                    UpdateQuickSpell(0);
                    break;
                case "quickSpellGrid2":
                    UpdateQuickSpell(1);
                    break;
                case "quickSpellGrid3":
                    UpdateQuickSpell(2);
                    break;
                case "quickSpellGrid4":
                    UpdateQuickSpell(3);
                    break;
            }
        }

        private void UpdateInventoryData(InventoryGridData itemGridData)
        {
            if (itemGridData == null)
            {
                return;
            }

            int index = inventoryData.FindIndex(s => s.gridId == itemGridData.gridId);
            if (index == -1)
            {
                return;
            }

            inventoryData[index] = itemGridData;
            socket.Emit("syncInventoryData", new JSONObject(JsonUtility.ToJson(new SendInventoryData(inventoryData))));
        }

        public override void Deinit()
        {
            if (gridManager != null)
            {
                gridManager.onUpdateData.RemoveListener(UpdateData);
                gridManager.Deinit();
            }
        }
    }

    [Serializable]
    public class SendInventoryData
    {
        public List<InventoryGridData> inventoryData = new List<InventoryGridData>();
        public SendInventoryData(List<InventoryGridData> data)
        {
            inventoryData = data;
        }
    }
}
