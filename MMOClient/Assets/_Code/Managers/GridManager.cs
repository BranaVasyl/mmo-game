using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;

namespace BV
{
    public class GridManager : MonoBehaviour
    {
        private ItemGrid startItemGrid;
        private Vector2Int startGridPosition;
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

        private List<InventoryGridData> inventoryData = new List<InventoryGridData>();

        [Header("Grid Events and Callback")]
        private NewPlayerControls inputActions;
        [HideInInspector]
        public UnityEvent<InventoryGridData, InventoryGridData, InventoryItem> onUpdateData = new UnityEvent<InventoryGridData, InventoryGridData, InventoryItem>();

        public delegate Task<bool> CanUpdateGridDelegate(ItemGrid startGrid, ItemGrid targetGrid, InventoryItem selectedItem, bool placeItemMode);
        [HideInInspector]
        public List<CanUpdateGridDelegate> canUpdateGridCallback = new List<CanUpdateGridDelegate>();

        [Header("Loader")]
        private bool loadInProcsess = false;

        public void Init()
        {
            itemsManager = ItemsManager.singleton;

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
                }
                else
                {
                    inventoryData[index] = iData[i];
                }

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

                string itemId = item.id;
                ItemData itemData = itemsManager.GetItemById(itemId);

                if (itemData == null)
                {
                    continue;
                }

                InventoryItem inventoryItem = CreateInventoryItem();
                inventoryItem.Set(itemData);
                if (item.rotated)
                {
                    inventoryItem.Rotate();
                }

                itemGrid.PlaceItem(inventoryItem, item.position.x, item.position.y);
            }
        }

        private void OnUpdateGridData(ItemGrid startGrid, ItemGrid selectedGrid)
        {
            InventoryGridData startGridData = null;
            InventoryGridData targetGridData = null;

            if (startGrid != null)
            {
                startGridData = UpdateGridData(startGrid);
            }

            if (startGrid != null && selectedGrid != null && startGrid.gridId != selectedGrid.gridId)
            {
                targetGridData = UpdateGridData(selectedGrid);
            }

            onUpdateData.Invoke(startGridData, targetGridData, selectedItem);
        }

        private InventoryGridData UpdateGridData(ItemGrid itemGrid)
        {
            if (itemGrid == null)
            {
                return null;
            }

            int index = inventoryData.FindIndex(s => s.gridId == itemGrid.gridId);
            if (index == -1)
            {
                return null;
            }

            InventoryGridData inventoryGridData = new InventoryGridData(itemGrid.gridId, new Vector2Int(itemGrid.gridSizeWidth, itemGrid.gridSizeHeight), itemGrid.supportedItemType);

            List<InventoryItem> alreadyCheckedItems = new List<InventoryItem>();
            InventoryItem[,] inventoryItem = itemGrid.inventoryItemSlot;
            for (int i = 0; i < inventoryItem.GetLength(0); i++)
            {
                for (int j = 0; j < inventoryItem.GetLength(1); j++)
                {
                    if (inventoryItem[i, j] != null)
                    {
                        InventoryItem curInventoryItem = inventoryItem[i, j];
                        if (alreadyCheckedItems.Find(x => x == curInventoryItem) != null)
                        {
                            continue;
                        }

                        inventoryGridData.items.Add(new InventoryItemData(curInventoryItem.itemData.id, curInventoryItem.onGridPositionX, curInventoryItem.onGridPositionY, curInventoryItem.rotated));
                        alreadyCheckedItems.Add(curInventoryItem);
                    }
                }
            }

            inventoryData[index] = inventoryGridData;
            return inventoryGridData;
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
                PickUpItem(tileGridPosition);
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
        private async void HandleHighlight()
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
                bool canPlace = await CanSetInPlace(false);
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

        private async Task<bool> CanSetInPlace(bool placeItemMode = false)
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
                }
            );

            if (!result)
            {
                return result;
            }

            Vector2Int tileGridPosition = GetTileGridPosition();
            result = selectedItemGrid.CanPlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y);

            for (int i = 0; i < canUpdateGridCallback.Count; i++)
            {
                if (result == false)
                {
                    break;
                }

                result = await canUpdateGridCallback[i](startItemGrid, selectedItemGrid, selectedItem, placeItemMode);
            }

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
            InventoryItem inventoryItem = Instantiate(itemsManager.itemPrefab).GetComponent<InventoryItem>();

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
            MenuManager.singleton.ToogleLoader(true);

            bool canPlace = await CanSetInPlace(true);
            if (canPlace)
            {
                bool complete = saveSelectGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, ref overlapItem);
                if (complete)
                {
                    if (overlapItem != null)
                    {
                        selectedItem = overlapItem;
                        overlapItem = null;
                        saveStartGrid.PlaceItem(selectedItem, startGridPosition.x, startGridPosition.y, ref overlapItem);
                    }

                    rectTransform = null;
                    OnUpdateGridData(saveStartGrid, saveSelectGrid);
                    startItemGrid = null;
                    selectedItem = null;
                }
            }
            else
            {
                RevertItemPosition(saveStartGrid, saveSelectGrid);
            }

            loadInProcsess = false;
            MenuManager.singleton.ToogleLoader(false);
        }

        public void PickUpItem(string itemId)
        {
            ItemData? item = itemsManager.GetItemById(itemId);
            if (item == null)
            {
                return;
            }

            ref List<InventoryGridData> inventoryData = ref ManagersController.singleton.playerData.inventoryData;
            int gridIndex = inventoryData.FindIndex(el =>
            {
                for (int i = 0; i < el.supportedItemType.Count; i++)
                {
                    if (el.supportedItemType[i] == item.itemType)
                    {
                        return true;
                    }
                }

                return false;
            });
            if (gridIndex == -1)
            {
                return;
            }
            InventoryGridData gridData = inventoryData[gridIndex];

            bool rotated = false;
            bool[,] inventoryDataMatrix = GenerateInventoryDataMatrix(gridData);

            Vector2Int? itemPosition = FindSpaceForObject(item.width, item.height, gridData, inventoryDataMatrix);
            if (itemPosition == null)
            {
                rotated = true;
                itemPosition = FindSpaceForObject(item.height, item.width, gridData, inventoryDataMatrix);
            }

            if (itemPosition == null)
            {
                Debug.Log("Немає місця");
                return;
            }

            if (item.itemType == ItemType.quest)
            {
                string notificationTitle = "Отримано: Hовий квестовий предмет";
                string notificationSubtitle = item.itemName;
                Sprite notificationIcon = item.itemIcon;
                NotificationActionType action = NotificationActionType.alert;

                NotificationManager.singleton.AddNewNotification(new NotificationData(notificationTitle, notificationSubtitle, notificationIcon));
            }

            InventoryItemData newItemData = new InventoryItemData(itemId, itemPosition.Value.x, itemPosition.Value.y, rotated);
            inventoryData[gridIndex].items.Add(newItemData);

        }

        private bool[,] GenerateInventoryDataMatrix(InventoryGridData gridData)
        {
            bool[,] array = new bool[gridData.gridSize.x, gridData.gridSize.y];

            for (int i = 0; i < gridData.items.Count; i++)
            {
                ItemData? selectItem = itemsManager.GetItemById(gridData.items[i].id);
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

        private void PickUpItem(Vector2Int tileGridPosition)
        {
            selectedItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
            if (selectedItem == null)
            {
                return;
            }

            startItemGrid = selectedItemGrid;
            startGridPosition = new Vector2Int(selectedItem.onGridPositionX, selectedItem.onGridPositionY);

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
        }

        public void Deinit()
        {
            RevertItemPosition(startItemGrid, selectedItemGrid);

            selectedItemGrid = null;
            correctInventoryHiglight.SetParent(null);
            incorrectInventoryHiglight.SetParent(null);
            inventoryData = new List<InventoryGridData>();
            canUpdateGridCallback = new List<CanUpdateGridDelegate>();
        }

        public static GridManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
