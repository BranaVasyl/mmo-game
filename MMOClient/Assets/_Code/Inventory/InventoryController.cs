using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class InventoryController : MonoBehaviour
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

        InventoryItem selectedItem;
        InventoryItem overlapItem;
        RectTransform rectTransform;

        [SerializeField] List<ItemData> items;
        [SerializeField] GameObject itemPrefab;
        [SerializeField] Transform canvasTransform;

        InventoryHiglight inventoryHiglight;

        NewPlayerControls inputActions;
        float clickTimer = 0;
        bool rb_input = false;
        bool lt_input = false;
        bool rt_input = false;
        bool x_input = false;

        public static InventoryController singleton;
        private void Awake()
        {
            singleton = this;
            inventoryHiglight = GetComponent<InventoryHiglight>();
        }

        public void Init()
        {
            if (inputActions == null)
            {
                inputActions = new NewPlayerControls();
                GetInput();
            }

            inputActions.Enable();
        }

        public void Deinit()
        {
            DropInput();
            inputActions.Enable();
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
        InventoryItem itemToHighlight;
        private void HandleHighlight()
        {
            Vector2Int positionOnGrid = GetTileGridPosition();
            if (oldPosition == positionOnGrid)
            {
                return;
            }


            oldPosition = positionOnGrid;

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
                return;
            }

            selectedItemGrid.PlaceItem(itemToInsert, posOnGrid.Value.x, posOnGrid.Value.y);
        }

        private void CreateRandomItem()
        {
            InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
            selectedItem = inventoryItem;

            rectTransform = inventoryItem.GetComponent<RectTransform>();
            rectTransform.SetParent(canvasTransform);
            rectTransform.SetAsLastSibling();

            int selectedItemID = UnityEngine.Random.Range(0, items.Count);
            inventoryItem.Set(items[selectedItemID]);
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
                rectTransform.position = inputActions.Mouse.MousePosition.ReadValue<Vector2>();
            }
        }

        void GetInput()
        {
            inputActions.PlayerActions.X.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref x_input);
            inputActions.PlayerActions.RT.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref rt_input);
            inputActions.PlayerActions.LT.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref lt_input);
            inputActions.Mouse.LeftButtonDown.performed += inputActions => ClickAction(inputActions.ReadValue<float>(), ref rb_input);
        }

        void DropInput()
        {
            inputActions.PlayerActions.X.performed -= inputActions => ClickAction(inputActions.ReadValue<float>(), ref x_input);
            inputActions.PlayerActions.RT.performed -= inputActions => ClickAction(inputActions.ReadValue<float>(), ref rt_input);
            inputActions.PlayerActions.LT.performed -= inputActions => ClickAction(inputActions.ReadValue<float>(), ref lt_input);
            inputActions.Mouse.LeftButtonDown.performed -= inputActions => ClickAction(inputActions.ReadValue<float>(), ref rb_input);
        }

        void ClickAction(float b, ref bool button)
        {
            if (clickTimer > 0)
            {
                return;
            }

            if (b > 0)
            {
                clickTimer = .1f;
                button = true;
            }
            else
            {
                button = false;
            }
        }
    }
}
