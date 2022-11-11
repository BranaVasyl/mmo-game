using System.Collections.Generic;
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

        private NewPlayerControls inputActions;
        [HideInInspector]
        public UnityEvent<ItemGrid, ItemGrid> onUpdateData = new UnityEvent<ItemGrid, ItemGrid>();

        public delegate bool CanUpdateGridDelegate(ItemGrid startGrid, ItemGrid targetGrid);
        [HideInInspector]
        public List<CanUpdateGridDelegate> canUpdateGridCallback = new List<CanUpdateGridDelegate>();

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
                ItemData itemData = itemsManager.allItems.Find(x => x.id == itemId);

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

        private void OnUpdateGridData()
        {
            onUpdateData.Invoke(startItemGrid, selectedItemGrid);
        }

        new private void Update()
        {
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
                    RevertItemPosition();
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

            for (int i = 0; i < canUpdateGridCallback.Count; i++)
            {
                if (result == false)
                {
                    break;
                }

                result = canUpdateGridCallback[i](startItemGrid, selectedItemGrid);
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

            int selectedItemID = UnityEngine.Random.Range(0, itemsManager.allItems.Count);
            inventoryItem.Set(itemsManager.allItems[selectedItemID]);
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

        private void RevertItemPosition()
        {
            if (startItemGrid == null || selectedItem == null)
            {
                return;
            }

            startItemGrid.PlaceItem(selectedItem, startGridPosition.x, startGridPosition.y, ref overlapItem);
            rectTransform = null;
            selectedItem = null;
            OnUpdateGridData();
            startItemGrid = null;
        }

        private void PlaceItem(Vector2Int tileGridPosition)
        {
            bool canPlace = CanSetInPlace();
            if (!canPlace)
            {
                RevertItemPosition();
                return;
            }

            bool complete = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, ref overlapItem);
            if (complete)
            {
                if (overlapItem != null)
                {
                    selectedItem = overlapItem;
                    overlapItem = null;
                    startItemGrid.PlaceItem(selectedItem, startGridPosition.x, startGridPosition.y, ref overlapItem);
                }

                rectTransform = null;
                OnUpdateGridData();
                startItemGrid = null;
                selectedItem = null;
            }
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
            RevertItemPosition();

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
