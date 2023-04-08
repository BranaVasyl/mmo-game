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
        private ManagersController managersController;
        private MenuManager menuManager;

        private ItemsManager itemsManager;
        private GridManager gridManager;

        private InventoryManager inventoryManager;

        public override void Init(ManagersController mC, MenuManager mM)
        {
            singleton = this;

            managersController = mC;
            menuManager = mM;

            itemsManager = ItemsManager.singleton;
            gridManager = GridManager.singleton;
        }

        public void RegisterCharacterListener(CharacterManager characterMan)
        {
            inventoryManager = characterMan.gameObject.GetComponent<InventoryManager>();

            if (managersController.playerEquipData.Count == 0)
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
                UpdateEquip(startGridData);
            }

            if (targetGridData != null)
            {
                menuManager.UpdateInventoryData(targetGridData);
                UpdatePlayerEquipData(targetGridData);
                UpdateEquip(targetGridData);
            }
        }

        #region Update Equip
        private void UpdateLeftHand()
        {
            InventoryGridData inventoryGrid = managersController.playerEquipData.Find(x => x.gridId == "leftHandGrid");
            if (inventoryGrid == null)
            {
                return;
            }

            List<InventoryItemData> items = inventoryGrid.items;
            ItemWeaponData item = null;
            if (items.Count > 0)
            {
                string itemId = items[0].id;
                item = itemsManager.GetItemById(itemId) as ItemWeaponData;
            }

            inventoryManager.UpdateLeftHand(item);
        }

        private void UpdateRightHand()
        {
            InventoryGridData inventoryGrid = managersController.playerEquipData.Find(x => x.gridId == "rightHandGrid");
            if (inventoryGrid == null)
            {
                return;
            }

            List<InventoryItemData> items = inventoryGrid.items;
            ItemWeaponData item = null;
            if (items.Count > 0)
            {
                string itemId = items[0].id;
                item = itemsManager.GetItemById(itemId) as ItemWeaponData;
            }

            inventoryManager.UpdateRightHand(item);
        }

        private void UpdateQuickSpell(int id)
        {
            InventoryGridData inventoryGrid = managersController.playerEquipData.Find(x => x.gridId == "quickSpellGrid" + (id + 1));
            if (inventoryGrid == null)
            {
                return;
            }

            List<InventoryItemData> items = inventoryGrid.items;
            Spell item = null;
            if (items.Count > 0)
            {
                string itemId = items[0].id;
                item = itemsManager.GetItemById(itemId) as Spell;
            }

            inventoryManager.UpdateQuickSpell(id, item);
        }
        #endregion

        private void UpdatePlayerEquipData(InventoryGridData itemGridData)
        {
            int index = managersController.playerEquipData.FindIndex(s => s.gridId == itemGridData.gridId);
            if (index == -1)
            {
                return;
            }

            managersController.playerEquipData[index] = itemGridData;
            managersController.socket.Emit("syncPlayerEquipData", new JSONObject(JsonUtility.ToJson(new SendInventoryData(managersController.playerEquipData))));
        }

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
