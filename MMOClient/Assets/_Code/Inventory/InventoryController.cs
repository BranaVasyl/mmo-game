using System.Collections;
using System.Collections.Generic;
using Project.Utility;
using UnityEngine;

namespace BV
{
    public class InventoryController : MenuPanel
    {
        [HideInInspector]
        public ItemGrid selectedItemGrid;
        public ItemGrid SelectedItemGrid
        {
            get => selectedItemGrid;
            set
            {
                selectedItemGrid = value;
                inventoryHiglight.SetParent(value);
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

        [HideInInspector]
        public InventoryHiglight inventoryHiglight;

        private JSONObject inventoryData;

        public static InventoryController singleton;
        private void Awake()
        {
            singleton = this;
            inventoryHiglight = GetComponent<InventoryHiglight>();
        }

        public override void Init(JSONObject pD)
        {
            base.Init(pD);
            inventoryData = pD["inventoryData"];
        }

        public void RegisterGrid(ItemGrid itemGrid)
        {
            allActiveGrids.Add(itemGrid);

            if (inventoryData == null)
            {
                return;
            }

            JSONObject gridData = null;
            for (int i = 0; i < inventoryData.Count; i++)
            {
                if (inventoryData[i]["gridId"].ToString().RemoveQuotes() == itemGrid.gridId)
                {
                    gridData = inventoryData[i];
                    break;
                }
            }

            if (gridData == null)
            {
                return;
            }

            SetGridData(itemGrid, gridData["items"]);
        }

        public void RemoveItemGrid(ItemGrid itemGrid)
        {
            ItemGrid itemOnList = allActiveGrids.Find(i => i.gridId == itemGrid.gridId);
        }

        void SetGridData(ItemGrid itemGrid, JSONObject items)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                JSONObject item = items[i];

                string itemId = item["id"].ToString().RemoveQuotes();
                ItemData itemData = allItems.Find(x => x.id == itemId);

                if (itemData == null)
                {
                    continue;
                }

                InventoryItem inventoryItem = CreateInventoryItem();
                inventoryItem.Set(itemData);

                int x = item["position"]["x"].JSONObjectToInt();
                int y = item["position"]["y"].JSONObjectToInt();

                itemGrid.PlaceItem(inventoryItem, x, y);
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
                inventoryHiglight.Show(false);
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
                    inventoryHiglight.Show(true);
                    inventoryHiglight.SetSize(itemToHighlight);
                    inventoryHiglight.SetPosition(selectedItemGrid, itemToHighlight);
                }
                else
                {
                    inventoryHiglight.Show(false);
                }
            }
            else
            {
                inventoryHiglight.Show(selectedItemGrid.BoundryCheck(positionOnGrid.x, positionOnGrid.y, selectedItem.WIDTH, selectedItem.HEIGHT));
                inventoryHiglight.SetSize(selectedItem);
                inventoryHiglight.SetPosition(selectedItemGrid, selectedItem, positionOnGrid.x, positionOnGrid.y);
            }
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
            bool complete = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, ref overlapItem);
            if (complete)
            {
                rectTransform = null;
                selectedItem = null;

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
            oldPosition = new Vector2Int();
            oldItemGrid = null;
        }
    }
}
