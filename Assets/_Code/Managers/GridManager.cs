using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using System.Linq;

namespace BV
{
    public class GridManager : Singleton<GridManager>
    {
        public static readonly string[] INVENTORY_GRID_LIST =
{
            "otherGrid",
            "craftGrid",
            "questGrid",
            "alchemyGrid",
            "spellGrid",
            "foodGrid",
            "elixirGrid",
            "butterGrid",
            "armorGrid",
            "weaponGrid"
        };

        public static readonly string[] EQUIP_GRID_LIST =
        {
            "rightHandGrid",
            "leftHandGrid",
            "quickSpellGrid1",
            "quickSpellGrid2",
            "quickSpellGrid3",
            "quickSpellGrid4",
            "pocketsGrid1",
            "pocketsGrid2",
            "pocketsGrid3",
            "pocketsGrid4"
        };

        public static readonly string[] CHEST_GRID_LIST =
        {
            "chestGrid"
        };

        public static readonly string[] SHOP_GRID_LIST =
        {
            "shopGrid"
        };


        private ItemGrid startItemGrid;
        private Vector2Int startGridPosition;
        private bool startGridRotated;

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
        private ItemsManager itemsManager;

        public Transform canvasTransform;

        public InventoryHiglight correctInventoryHiglight;
        public InventoryHiglight incorrectInventoryHiglight;
        public GameObject itemPrefab;

        public List<InventoryGridData> gridSettings;
        private List<InventoryGridData> inventoryData = new List<InventoryGridData>();

        [Header("Grid Events and Callback")]
        private NewPlayerControls inputActions;

        [HideInInspector]
        public delegate bool CanPlaceItemDelegate(ItemGrid startGrid, ItemGrid targetGrid, InventoryItem selectedItem);
        [HideInInspector]
        public List<CanPlaceItemDelegate> canPlaceItemCallback = new List<CanPlaceItemDelegate>();

        [HideInInspector]
        public delegate Task<bool> UpdateItemPositionDelegate(UpdateItemPositionData itemUpdateData);
        [HideInInspector]
        public List<UpdateItemPositionDelegate> updateItemPositionCallback = new List<UpdateItemPositionDelegate>();

        [Header("Loader")]
        private bool loadInProcsess = false;

        public void Start()
        {
            itemsManager = ItemsManager.Instance;

            if (inputActions == null)
            {
                inputActions = new NewPlayerControls();
                GetInput();
            }

            inputActions.Enable();
        }

        public InventoryGridData? GetGridSetting(string gridId)
        {
            return inventoryData.Find(i => i.gridId == gridId);
        }

        public void RegisterGrid(ItemGrid itemGrid)
        {
            allActiveGrids.Add(itemGrid);
            SetGridData(itemGrid);
        }

        public void RemoveItemGrid(ItemGrid itemGrid)
        {
            ItemGrid itemOnList = allActiveGrids.Find(i => i.gridId == itemGrid.gridId);

            if (itemOnList != null)
            {
                allActiveGrids.Remove(itemOnList);
            }
        }

        public void SetData(List<InventoryGridData> iData)
        {
            for (int i = 0; i < iData.Count; i++)
            {
                int index = inventoryData.FindIndex(s => s.gridId == iData[i].gridId);
                if (index == -1)
                {
                    inventoryData.Add(iData[i]);
                    index = inventoryData.Count - 1;
                }
                else
                {
                    inventoryData[index] = iData[i];
                }

                InventoryGridData setting = gridSettings.Find(el => el.gridId == iData[i].gridId);
                if (setting == null)
                {
                    continue;
                }

                inventoryData[index].gridSize = setting.gridSize;
                inventoryData[index].supportedItemType = setting.supportedItemType;

                ItemGrid currentGrid = allActiveGrids.Find(x => x.gridId == iData[i].gridId);
                if (currentGrid != null)
                {
                    SetGridData(currentGrid);
                }
            }
        }

        void SetGridData(ItemGrid itemGrid)
        {
            if (inventoryData == null)
            {
                return;
            }

            itemGrid.CleanGrid();
            InventoryGridData gridData = inventoryData.Find(x => x.gridId == itemGrid.gridId);
            if (gridData == null)
            {
                return;
            }

            List<InventoryItemData> items = gridData.items;
            for (int i = 0; i < items.Count; i++)
            {
                InventoryItemData item = items[i];

                string itemId = item.item.code;
                ItemData itemData = itemsManager.GetItemById(itemId);

                if (itemData == null)
                {
                    continue;
                }

                InventoryItem inventoryItem = CreateInventoryItem();
                inventoryItem.id = item.id;
                inventoryItem.Set(itemData);
                if (item.rotated)
                {
                    inventoryItem.Rotate();
                }

                itemGrid.PlaceItem(inventoryItem, item.position.x, item.position.y);
            }
        }

        private void OnUpdateInventoryData(ItemGrid startGrid, ItemGrid selectedGrid, InventoryItem item)
        {
            if (item == null)
            {
                Debug.LogError("Item is null");
                return;
            }

            if (startGrid != null)
            {
                int startGridIndex = inventoryData.FindIndex(s => s.gridId == startGrid.gridId);
                if (startGridIndex != -1)
                {
                    inventoryData[startGridIndex].items.RemoveAll(i => i.id == item.id);
                }
            }

            if (selectedGrid != null)
            {
                int selectedGridIndex = inventoryData.FindIndex(s => s.gridId == selectedGrid.gridId);
                if (selectedGridIndex != -1)
                {
                    InventoryItemData newItemData = new InventoryItemData(
                        item.id,
                        item.onGridPositionX,
                        item.onGridPositionY,
                        item.rotated,
                        item.itemData.id
                    );

                    inventoryData[selectedGridIndex].items.Add(newItemData);
                }
            }
        }

        new private void Update()
        {
            if (loadInProcsess)
            {
                return;
            }

            SimulateDragEvent();
            ItemIconDrag();

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

            if (dragEnd)
            {
                if (selectedItemGrid != null)
                {
                    Vector2Int tileGridPosition = GetTileGridPosition();
                    PlaceItem(tileGridPosition);
                }
                else
                {
                    RevertItemPosition(startItemGrid, selectedItemGrid);
                }

                forceHighlight = true;
            }

            if (selectedItemGrid == null)
            {
                correctInventoryHiglight.Show(false);
                incorrectInventoryHiglight.Show(false);
                return;
            }

            if (dragStart)
            {
                Vector2Int tileGridPosition = GetTileGridPosition();
                StartDragItem(tileGridPosition);
                OnMouseExitItem();
            }

            HandleHighlight();
        }

        private bool rb_input = false;
        private bool lt_input = false;
        private bool rt_input = false;
        private bool x_input = false;

        private bool dragStart = false;
        private bool dragged = false;
        private bool dragEnd = false;
        void SimulateDragEvent()
        {
            if (dragEnd)
            {
                dragEnd = false;
            }

            if (dragged && !rb_input)
            {
                dragEnd = true;
                dragged = false;
            }

            if (dragStart)
            {
                dragStart = false;
                dragged = true;
            }

            if (rb_input && !dragged)
            {
                dragStart = true;
            }
        }

        void GetInput()
        {
            //LTInput
            inputActions.PlayerActions.X.performed += inputActions => x_input = true;
            inputActions.PlayerActions.X.canceled += inputActions => x_input = false;

            //LTInput
            inputActions.PlayerActions.LT.performed += inputActions => lt_input = true;
            inputActions.PlayerActions.LT.canceled += inputActions => lt_input = false;

            //RTInput
            inputActions.PlayerActions.RT.performed += inputActions => rt_input = true;
            inputActions.PlayerActions.RT.canceled += inputActions => rt_input = false;

            //RBInput
            inputActions.PlayerActions.RB.performed += inputActions => rb_input = true;
            inputActions.PlayerActions.RB.canceled += inputActions => rb_input = false;
        }

        Vector2Int oldPosition;
        ItemGrid oldItemGrid;
        InventoryItem itemToHighlight;
        InventoryItem lastItemHihlight;
        bool forceHighlight = false;
        private async void HandleHighlight()
        {
            Vector2Int positionOnGrid = GetTileGridPosition();
            if (oldPosition == positionOnGrid && selectedItemGrid == oldItemGrid && !forceHighlight)
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
                    TooltipManager.Instance.ShowEmptyEquipTolltip(selectedItemGrid, canvasTransform.GetComponent<Canvas>().scaleFactor);
                }

                if ((itemToHighlight != lastItemHihlight) || forceHighlight)
                {
                    OnMouseExitItem();
                    lastItemHihlight = itemToHighlight;
                    if (lastItemHihlight != null)
                    {
                        OnMouseHoverItem(lastItemHihlight);
                    }
                }
            }
            else
            {
                bool success = CanPlaceItem();
                if (success)
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

                if ((lastItemHihlight != null) || forceHighlight)
                {
                    OnMouseExitItem();
                }

            }

            forceHighlight = false;
        }

        private void OnMouseHoverItem(InventoryItem item)
        {
            TooltipManager.Instance.ShowInventoryItemTooltip(item, canvasTransform.GetComponent<Canvas>().scaleFactor);
        }

        public void OnMouseExitItem()
        {
            TooltipManager.Instance.HideTooltip();
        }

        private bool CanPlaceItem()
        {
            if (selectedItem == null || selectedItemGrid == null)
            {
                return false;
            }

            if (!selectedItemGrid.supportedItemType.Any(i => selectedItem.GetItemType() == i))
            {
                return false;
            }

            Vector2Int tileGridPosition = GetTileGridPosition();
            if (!selectedItemGrid.CanPlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y))
            {
                return false;
            }

            for (int i = 0; i < canPlaceItemCallback.Count; i++)
            {
                bool result = canPlaceItemCallback[i](startItemGrid, selectedItemGrid, selectedItem);
                if (!result)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> UpdateItemPosition(Vector2Int tileGridPosition)
        {
            bool result = CanPlaceItem();
            if (!result)
            {
                return result;
            }

            UpdateItemPositionData updateItemData = new UpdateItemPositionData(startItemGrid.gridId, selectedItemGrid.gridId, selectedItem.id, tileGridPosition, selectedItem.rotated);
            return await ExecuteUpdateItemPositionCallbacks(updateItemData);
        }

        private async Task<bool> ExecuteUpdateItemPositionCallbacks(UpdateItemPositionData updateItemData)
        {
            for (int i = 0; i < updateItemPositionCallback.Count; i++)
            {
                bool result = await updateItemPositionCallback[i](updateItemData);

                if (!result)
                {
                    return false;
                }
            }

            return true;
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

            int selectedItemID = UnityEngine.Random.Range(0, itemsManager.GetItemsCount());
            inventoryItem.Set(itemsManager.GetItemByIndex(selectedItemID));
        }

        private void RotateItem()
        {
            if (selectedItem == null)
            {
                return;
            }

            selectedItem.Rotate();
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

        private void RevertItemPosition(ItemGrid startGrid = null, ItemGrid selectedGrid = null)
        {
            if (startGrid == null || selectedItem == null)
            {
                return;
            }

            if (selectedItem.rotated != startGridRotated)
            {
                selectedItem.Rotate();
            }
            startGrid.PlaceItem(selectedItem, startGridPosition.x, startGridPosition.y, ref overlapItem);

            rectTransform = null;
            selectedItem = null;
            startItemGrid = null;
        }

        private async void PlaceItem(Vector2Int tileGridPosition)
        {
            ItemGrid saveStartGrid = startItemGrid;
            ItemGrid saveSelectGrid = selectedItemGrid;

            loadInProcsess = true;
            bool success = await UpdateItemPosition(tileGridPosition);
            if (success)
            {
                bool complete = saveSelectGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, ref overlapItem);
                if (complete)
                {
                    OnUpdateInventoryData(saveStartGrid, saveSelectGrid, selectedItem);

                    if (overlapItem != null)
                    {
                        selectedItem = overlapItem;
                        overlapItem = null;

                        if (selectedItem.rotated != startGridRotated)
                        {
                            selectedItem.Rotate();
                        }
                        saveStartGrid.PlaceItem(selectedItem, startGridPosition.x, startGridPosition.y, ref overlapItem);
                        OnUpdateInventoryData(saveSelectGrid, saveStartGrid, selectedItem);
                    }

                    rectTransform = null;
                    startItemGrid = null;
                    selectedItem = null;
                }
            }
            else
            {
                RevertItemPosition(saveStartGrid, saveSelectGrid);
            }

            loadInProcsess = false;
        }

        public async Task<bool> PickUpItem(InventoryItemData inventoryItem)
        {
            int startGridIndex = inventoryData.FindIndex(el =>
                el.items.Any(i => i.id == inventoryItem.id)
            );

            if (startGridIndex == -1)
            {
                return false;
            }

            ItemData itemData = ItemsManager.Instance.GetItemById(inventoryItem.item.code);
            int targetGridIndex = inventoryData.FindIndex(el =>
                el.supportedItemType.Contains(itemData.type)
            );

            if (targetGridIndex == -1)
            {
                return false;
            }
            InventoryGridData startGridData = inventoryData[startGridIndex];
            InventoryGridData targetGridData = inventoryData[targetGridIndex];

            bool rotated = false;
            bool[,] inventoryDataMatrix = GenerateInventoryDataMatrix(targetGridData);

            Vector2Int? itemPosition = FindSpaceForObject(itemData.width, itemData.height, targetGridData, inventoryDataMatrix);
            if (itemPosition == null)
            {
                rotated = true;
                itemPosition = FindSpaceForObject(itemData.height, itemData.width, targetGridData, inventoryDataMatrix);
            }

            if (itemPosition == null)
            {   
                return false;
            }

            UpdateItemPositionData updateItemData = new UpdateItemPositionData(startGridData.gridId, targetGridData.gridId, inventoryItem.id, itemPosition.Value, inventoryItem.rotated);
            bool result = await ExecuteUpdateItemPositionCallbacks(updateItemData);
            
            if (result)
            {
                startGridData.items.RemoveAll(i => i.id == inventoryItem.id);

                inventoryItem.rotated = rotated;
                inventoryItem.position = itemPosition.Value;

                targetGridData.items.Add(inventoryItem);
            }
            
            return result;
        }

        private bool[,] GenerateInventoryDataMatrix(InventoryGridData gridData)
        {
            bool[,] array = new bool[gridData.gridSize.x, gridData.gridSize.y];

            for (int i = 0; i < gridData.items.Count; i++)
            {
                ItemData? selectItem = itemsManager.GetItemById(gridData.items[i].item.code);
                if (selectItem == null)
                {
                    continue;
                }

                int itemWidth = gridData.items[i].rotated ? selectItem.height : selectItem.width;
                int itemHeight = gridData.items[i].rotated ? selectItem.width : selectItem.height;

                for (int x = 0; x < itemWidth; x++)
                {
                    for (int y = 0; y < itemHeight; y++)
                    {
                        array[gridData.items[i].position.x + x, gridData.items[i].position.y + y] = true;
                    }
                }
            }

            return array;
        }

        public Vector2Int? FindSpaceForObject(int itemWidth, int itemHeight, InventoryGridData gridData, bool[,] array)
        {
            int width = gridData.gridSize.x - itemWidth + 1;
            int height = gridData.gridSize.y - itemHeight + 1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (CheckAvaibleSpace(array, x, y, itemWidth, itemHeight) == true)
                    {
                        return new Vector2Int(x, y);
                    };
                }
            }

            return null;
        }

        private bool CheckAvaibleSpace(bool[,] array, int posX, int posY, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (array[posX + x, posY + y])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void StartDragItem(Vector2Int tileGridPosition)
        {
            selectedItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
            if (selectedItem == null)
            {
                return;
            }

            startItemGrid = selectedItemGrid;
            startGridPosition = new Vector2Int(selectedItem.onGridPositionX, selectedItem.onGridPositionY);
            startGridRotated = selectedItem.rotated;

            if (selectedItem != null)
            {
                rectTransform = selectedItem.GetComponent<RectTransform>();
            }
        }

        private void ItemIconDrag()
        {
            if (selectedItem != null)
            {
                selectedItem.transform.SetParent(canvasTransform);
                rectTransform.position = inputActions.Mouse.MousePosition.ReadValue<Vector2>();
            }
        }

        public void Clean()
        {
            SelectedItemGrid = null;
            oldPosition = new Vector2Int();
            oldItemGrid = null;

            lastItemHihlight = null;
            OnMouseExitItem();
        }

        public void Deinit()
        {
            RevertItemPosition(startItemGrid, selectedItemGrid);

            selectedItemGrid = null;
            correctInventoryHiglight.SetParent(null);
            incorrectInventoryHiglight.SetParent(null);
            inventoryData = new List<InventoryGridData>();
            canPlaceItemCallback = new List<CanPlaceItemDelegate>();
            updateItemPositionCallback = new List<UpdateItemPositionDelegate>();

            OnMouseExitItem();
        }

        public string[] GetEquipGridList()
        {
            return EQUIP_GRID_LIST;
        }

        public string[] GetInventoryGridList()
        {
            return INVENTORY_GRID_LIST;
        }

        public string[] GetCharacterGridList()
        {
            return INVENTORY_GRID_LIST.Concat(EQUIP_GRID_LIST).ToArray();
        }

        public string[] GetChestGridList()
        {
            return CHEST_GRID_LIST;
        }

        public string[] GetShopGridList()
        {
            return SHOP_GRID_LIST;
        }
    }
}
