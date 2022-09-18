using System.Collections;
using System.Collections.Generic;
using Project.Utility;
using UnityEngine;
using System;
using SocketIO;

namespace BV
{
    public class InventoryController : MenuPanel
    {
        private SocketIOComponent socket;
        private ItemGrid startItemGrid;
        [HideInInspector]
        public ItemGrid selectedItemGrid;
        public ItemGrid SelectedItemGrid
        {
            get => selectedItemGrid;
            set
            {
                selectedItemGrid = value;
                correctInventoryHiglight.SetParent(value);
                incorrectInventoryHiglight.SetParent(value);
            }
        }
        private List<ItemGrid> allActiveGrids = new List<ItemGrid>();

        InventoryItem selectedItem;
        InventoryItem overlapItem;
        RectTransform rectTransform;

        [Header("Inventory Stats")]
        [SerializeField] List<ItemData> allItems;
        [SerializeField] GameObject itemPrefab;
        public Transform canvasTransform;

        public InventoryHiglight correctInventoryHiglight;
        public InventoryHiglight incorrectInventoryHiglight;
        private List<InventoryGridData> inventoryData;

        private CharacterManager characterManager;
        private InventoryManager inventoryManager;

        void OnDisable()
        {
            selectedItemGrid = null;
            correctInventoryHiglight.SetParent(null);
            incorrectInventoryHiglight.SetParent(null);
        }

        public static InventoryController singleton;
        public override void Init(SocketIOComponent soc, PlayerData playerData)
        {
            singleton = this;
            base.Init(soc, playerData);
            socket = soc;
            inventoryData = playerData.inventoryData;
        }

        public void RegisterCharacterListener(CharacterManager characterMan)
        {
            characterManager = characterMan;
            inventoryManager = characterManager.gameObject.GetComponent<InventoryManager>();

            UpdateEquip(true);
        }

        public void RegisterGrid(ItemGrid itemGrid)
        {
            allActiveGrids.Add(itemGrid);

            if (inventoryData == null)
            {
                return;
            }

            InventoryGridData gridData = inventoryData.Find(x => x.gridId == itemGrid.gridId);
            if (gridData == null)
            {
                return;
            }

            SetGridData(itemGrid, gridData.items);
        }

        public void RemoveItemGrid(ItemGrid itemGrid)
        {
            ItemGrid itemOnList = allActiveGrids.Find(i => i.gridId == itemGrid.gridId);

            if (itemOnList != null)
            {
                allActiveGrids.Remove(itemOnList);
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
                item = allItems.Find(x => x.id == itemId) as ItemWeaponData;
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
                item = allItems.Find(x => x.id == itemId) as ItemWeaponData;
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
                item = allItems.Find(x => x.id == itemId) as Spell;
            }

            inventoryManager.UpdateQuickSpell(id, item);
        }

        #endregion

        private void UpdateEquip(bool updateAll = false)
        {
            if (inventoryData.Count == 0)
            {
                return;
            }

            string startGridId = startItemGrid != null ? startItemGrid.gridId : "";
            string targetGridId = selectedItemGrid != null ? selectedItemGrid.gridId : "";

            if (updateAll)
            {
                UpdateLeftHand();
                UpdateRightHand();
                UpdateQuickSpell(0);
                UpdateQuickSpell(1);
                UpdateQuickSpell(2);
                UpdateQuickSpell(3);
                return;
            }

            switch (startGridId)
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

            switch (targetGridId)
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

        private void UpdateInventoryData()
        {
            List<InventoryItem> checkedItem = new List<InventoryItem>();
            List<InventoryGridData> newInventoryData = new List<InventoryGridData>();

            for (int k = 0; k < inventoryData.Count; k++)
            {
                ItemGrid itemGrid = allActiveGrids.Find(x => x.gridId == inventoryData[k].gridId);
                if (itemGrid == null)
                {
                    newInventoryData.Add(inventoryData[k]);
                    continue;
                }

                InventoryGridData inventoryGridData = new InventoryGridData(itemGrid.gridId);

                InventoryItem[,] inventoryItem = itemGrid.inventoryItemSlot;
                for (int i = 0; i < inventoryItem.GetLength(0); i++)
                {
                    for (int j = 0; j < inventoryItem.GetLength(1); j++)
                    {
                        if (inventoryItem[i, j] != null)
                        {
                            InventoryItem curInventoryItem = inventoryItem[i, j];
                            if (checkedItem.Find(x => x == curInventoryItem) != null)
                            {
                                continue;
                            }

                            inventoryGridData.items.Add(new InventoryItemData(curInventoryItem.itemData.id, curInventoryItem.onGridPositionX, curInventoryItem.onGridPositionY));
                            checkedItem.Add(curInventoryItem);
                        }
                    }
                }

                newInventoryData.Add(inventoryGridData);
            }

            inventoryData = newInventoryData;
            socket.Emit("syncInventoryData", new JSONObject(JsonUtility.ToJson(new SendInventoryData(newInventoryData))));
        }

        void SetGridData(ItemGrid itemGrid, List<InventoryItemData> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                InventoryItemData item = items[i];

                string itemId = item.id;
                ItemData itemData = allItems.Find(x => x.id == itemId);

                if (itemData == null)
                {
                    continue;
                }

                InventoryItem inventoryItem = CreateInventoryItem();
                inventoryItem.Set(itemData);

                itemGrid.PlaceItem(inventoryItem, item.position.x, item.position.y);
            }
        }

        private void Update()
        {
            if (clickTimer > 0)
            {
                clickTimer -= Time.deltaTime;
            }

            ItemIconDrag();

            if (lt_input)
            {
                lt_input = false;
                if (selectedItem == null)
                {
                    CreateRandomItem();
                }
            }

            if (rt_input)
            {
                rt_input = false;
                InsertRandomItem();
            }

            if (x_input)
            {
                x_input = false;
                RotateItem();
            }

            if (selectedItemGrid == null)
            {
                rb_input = false;
                correctInventoryHiglight.Show(false);
                incorrectInventoryHiglight.Show(false);
                return;
            }

            HandleHighlight();

            if (rb_input)
            {
                rb_input = false;
                LeftMouseButtonPress();
            }
        }

        Vector2Int oldPosition;
        ItemGrid oldItemGrid;
        InventoryItem itemToHighlight;
        private void HandleHighlight()
        {
            Vector2Int positionOnGrid = GetTileGridPosition();
            if (oldPosition == positionOnGrid && selectedItemGrid == oldItemGrid)
            {
                return;
            }

            oldPosition = positionOnGrid;
            oldItemGrid = selectedItemGrid;

            if (selectedItem == null)
            {
                if (selectedItemGrid != null)
                {
                    itemToHighlight = selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);
                }

                if (itemToHighlight != null)
                {
                    correctInventoryHiglight.Show(true);
                    correctInventoryHiglight.SetSize(itemToHighlight);
                    correctInventoryHiglight.SetPosition(selectedItemGrid, itemToHighlight);

                    incorrectInventoryHiglight.Show(false);
                }
                else
                {
                    correctInventoryHiglight.Show(false);
                    incorrectInventoryHiglight.Show(false);
                }
            }
            else
            {
                bool canPlace = CanSetInPlace();
                if (canPlace)
                {
                    correctInventoryHiglight.Show(selectedItemGrid.BoundryCheck(positionOnGrid.x, positionOnGrid.y, selectedItem.WIDTH, selectedItem.HEIGHT));
                    correctInventoryHiglight.SetSize(selectedItem);
                    correctInventoryHiglight.SetPosition(selectedItemGrid, selectedItem, positionOnGrid.x, positionOnGrid.y);

                    incorrectInventoryHiglight.Show(false);
                }
                else
                {
                    incorrectInventoryHiglight.Show(selectedItemGrid.BoundryCheck(positionOnGrid.x, positionOnGrid.y, selectedItem.WIDTH, selectedItem.HEIGHT));
                    incorrectInventoryHiglight.SetSize(selectedItem);
                    incorrectInventoryHiglight.SetPosition(selectedItemGrid, selectedItem, positionOnGrid.x, positionOnGrid.y);

                    correctInventoryHiglight.Show(false);
                }
            }
        }

        private bool CanSetInPlace()
        {
            bool result = false;
            if (selectedItem == null || selectedItemGrid == null)
            {
                return result;
            }

            selectedItemGrid.supportedItemType.ForEach(i =>
                {
                    if (selectedItem.GetItemType() == i)
                    {
                        result = true;
                    }
                });

            if (!result)
            {
                return result;
            }

            Vector2Int tileGridPosition = GetTileGridPosition();
            result = selectedItemGrid.CanPlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y);
            return result;
        }

        private void InsertRandomItem()
        {
            if (!selectedItemGrid)
            {
                return;
            }

            CreateRandomItem();
            InventoryItem itemToInsert = selectedItem;
            selectedItem = null;
            InsertItem(itemToInsert);
        }

        private void InsertItem(InventoryItem itemToInsert)
        {
            Vector2Int? posOnGrid = selectedItemGrid.FindSpaceForObject(itemToInsert);

            if (posOnGrid == null)
            {
                Destroy(itemToInsert.gameObject);
                return;
            }

            selectedItemGrid.PlaceItem(itemToInsert, posOnGrid.Value.x, posOnGrid.Value.y);
        }

        private InventoryItem CreateInventoryItem()
        {
            InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();

            rectTransform = inventoryItem.GetComponent<RectTransform>();
            rectTransform.SetParent(canvasTransform);
            rectTransform.SetAsLastSibling();

            return inventoryItem;
        }

        private void CreateRandomItem()
        {
            InventoryItem inventoryItem = CreateInventoryItem();
            selectedItem = inventoryItem;

            int selectedItemID = UnityEngine.Random.Range(0, allItems.Count);
            inventoryItem.Set(allItems[selectedItemID]);
        }

        private void RotateItem()
        {
            if (selectedItem == null)
            {
                return;
            }

            selectedItem.Rotate();
        }

        private void LeftMouseButtonPress()
        {
            Vector2Int tileGridPosition = GetTileGridPosition();

            if (selectedItem == null)
            {
                PickUpItem(tileGridPosition);
            }
            else
            {
                PlaceItem(tileGridPosition);
            }
        }

        private Vector2Int GetTileGridPosition()
        {

            Vector2 position = inputActions.Mouse.MousePosition.ReadValue<Vector2>();

            if (selectedItem != null)
            {
                position.x -= (selectedItem.WIDTH - 1) * ItemGrid.boundTileSizeWidth / 2;
                position.y += (selectedItem.HEIGHT - 1) * ItemGrid.boundTileSizeHeight / 2;
            }

            return selectedItemGrid.GetTileGridPosition(position);
        }

        private void PlaceItem(Vector2Int tileGridPosition)
        {
            bool canPlace = CanSetInPlace();
            if (!canPlace)
            {
                return;
            }

            bool complete = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, ref overlapItem);
            if (complete)
            {
                rectTransform = null;
                selectedItem = null;
                UpdateInventoryData();
                UpdateEquip(false);
                startItemGrid = null;

                if (overlapItem != null)
                {
                    selectedItem = overlapItem;
                    overlapItem = null;
                    rectTransform = selectedItem.GetComponent<RectTransform>();
                    rectTransform.SetAsLastSibling();
                }
            }
        }

        private void PickUpItem(Vector2Int tileGridPosition)
        {
            selectedItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
            startItemGrid = selectedItemGrid;

            if (selectedItem != null)
            {
                rectTransform = selectedItem.GetComponent<RectTransform>();
            }
        }

        private void ItemIconDrag()
        {
            if (selectedItem != null)
            {
                selectedItem.transform.SetParent(transform);
                rectTransform.position = inputActions.Mouse.MousePosition.ReadValue<Vector2>();
            }
        }

        public void Clean()
        {
            SelectedItemGrid = null;
            oldPosition = new Vector2Int();
            oldItemGrid = null;
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
