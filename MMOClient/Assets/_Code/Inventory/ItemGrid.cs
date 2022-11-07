using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class ItemGrid : MonoBehaviour
    {
        private GridManager gridManager;
        public string gridId;

        private Canvas canvas;

        public const float tileSizeWidth = 64;
        public const float tileSizeHeight = 64;

        public static float boundTileSizeWidth = 35f;
        public static float boundTileSizeHeight = 35f;

        public GameObject placeholder;
        public InventoryItem[,] inventoryItemSlot;

        RectTransform rectTransform;

        [SerializeField]
        int gridSizeWidth = 5;
        [SerializeField]
        int gridSizeHeight = 7;
        public List<ItemType> supportedItemType;

        void Awake()
        {
            gridManager = GridManager.singleton;
            canvas = gridManager.canvasTransform.GetComponent<Canvas>();

            rectTransform = GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            boundTileSizeWidth = tileSizeWidth * canvas.scaleFactor;
            boundTileSizeHeight = tileSizeHeight * canvas.scaleFactor;
            Init(gridSizeWidth, gridSizeHeight);

            gridManager.RegisterGrid(this);
        }

        public void CleanGrid()
        {
            var children = new List<GameObject>();
            foreach (Transform child in transform) children.Add(child.gameObject);
            children.ForEach(child =>
            {
                if (child == gridManager.correctInventoryHiglight.higlighter.gameObject || child == gridManager.incorrectInventoryHiglight.higlighter.gameObject)
                {
                    return;
                }
                Destroy(child);
            });

            if (placeholder != null)
            {
                placeholder.SetActive(true);
            }
        }

        void OnDisable()
        {
            inventoryItemSlot = new InventoryItem[0, 0];
            CleanGrid();
            gridManager.RemoveItemGrid(this);
        }

        private void Init(int width, int height)
        {
            inventoryItemSlot = new InventoryItem[width, height];
            Vector2 size = new Vector2(width * tileSizeWidth, height * tileSizeHeight);
            rectTransform.sizeDelta = size;
        }

        public InventoryItem PickUpItem(int x, int y)
        {
            InventoryItem toReturn = inventoryItemSlot[x, y];

            if (toReturn == null)
            {
                return null;
            }

            CleanGridReference(toReturn);

            if (placeholder != null)
            {
                placeholder.SetActive(true);
            }
            return toReturn;
        }

        private void CleanGridReference(InventoryItem item)
        {
            for (int ix = 0; ix < item.WIDTH; ix++)
            {
                for (int iy = 0; iy < item.HEIGHT; iy++)
                {
                    inventoryItemSlot[item.onGridPositionX + ix, item.onGridPositionY + iy] = null;
                }
            }
        }

        internal InventoryItem GetItem(int x, int y)
        {
            if (x < 0 || x > inventoryItemSlot.GetLength(0) - 1)
            {
                return null;
            }

            if (y < 0 || y > inventoryItemSlot.GetLength(1) - 1)
            {
                return null;
            }

            return inventoryItemSlot[x, y];
        }

        Vector2 positionOnTheGrid = new Vector2();
        Vector2Int tileGridPosition = new Vector2Int();

        public Vector2Int GetTileGridPosition(Vector2 mousePosition)
        {
            positionOnTheGrid.x = mousePosition.x - rectTransform.position.x;
            positionOnTheGrid.y = rectTransform.position.y - mousePosition.y;

            tileGridPosition.x = (int)(positionOnTheGrid.x / boundTileSizeWidth);
            tileGridPosition.y = (int)(positionOnTheGrid.y / boundTileSizeHeight);

            return tileGridPosition;
        }

        public Vector2Int? FindSpaceForObject(InventoryItem itemToInsert)
        {
            int height = gridSizeHeight - itemToInsert.HEIGHT + 1;
            int width = gridSizeWidth - itemToInsert.WIDTH + 1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (CheckAvaibleSpace(x, y, itemToInsert.WIDTH, itemToInsert.HEIGHT) == true)
                    {
                        return new Vector2Int(x, y);
                    };
                }
            }

            return null;
        }

        public bool CanPlaceItem(InventoryItem inventoryItem, int posX, int posY)
        {
            InventoryItem overlapItem = null;
            if (BoundryCheck(posX, posY, inventoryItem.WIDTH, inventoryItem.HEIGHT) == false)
            {
                return false;
            }

            if (OverlapCheck(posX, posY, inventoryItem.WIDTH, inventoryItem.HEIGHT, ref overlapItem) == false)
            {
                overlapItem = null;
                return false;
            }
            overlapItem = null;

            return true;
        }

        public bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, ref InventoryItem overlapItem)
        {
            if (BoundryCheck(posX, posY, inventoryItem.WIDTH, inventoryItem.HEIGHT) == false)
            {
                return false;
            }

            if (OverlapCheck(posX, posY, inventoryItem.WIDTH, inventoryItem.HEIGHT, ref overlapItem) == false)
            {
                overlapItem = null;
                return false;
            }

            if (overlapItem != null)
            {
                CleanGridReference(overlapItem);
            }

            PlaceItem(inventoryItem, posX, posY);

            return true;
        }

        public void PlaceItem(InventoryItem inventoryItem, int posX, int posY)
        {
            RectTransform rectTransform = inventoryItem.GetComponent<RectTransform>();
            rectTransform.SetParent(this.rectTransform);

            for (int x = 0; x < inventoryItem.WIDTH; x++)
            {
                for (int y = 0; y < inventoryItem.HEIGHT; y++)
                {
                    inventoryItemSlot[posX + x, posY + y] = inventoryItem;
                }
            }

            inventoryItem.onGridPositionX = posX;
            inventoryItem.onGridPositionY = posY;
            Vector2 position = CalculatePositionOnGrid(inventoryItem, posX, posY);

            if (placeholder != null)
            {
                placeholder.SetActive(false);
            }
            rectTransform.localPosition = position;
        }

        public Vector2 CalculatePositionOnGrid(InventoryItem inventoryItem, int posX, int posY)
        {
            Vector2 position = new Vector2();
            position.x = posX * tileSizeWidth + tileSizeWidth * inventoryItem.WIDTH / 2;
            position.y = -(posY * tileSizeHeight + tileSizeHeight * inventoryItem.HEIGHT / 2);
            return position;
        }

        private bool OverlapCheck(int posX, int posY, int width, int height, ref InventoryItem overlapItem)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (inventoryItemSlot[posX + x, posY + y] != null)
                    {
                        if (overlapItem == null)
                        {
                            overlapItem = inventoryItemSlot[posX + x, posY + y];
                        }

                        if (overlapItem != null)
                        {
                            if (overlapItem != inventoryItemSlot[posX + x, posY + y])
                            {
                                return false;
                            }

                            if (overlapItem.WIDTH != width || overlapItem.HEIGHT != height)
                            {
                                return false;
                            }

                            if (overlapItem.onGridPositionX != posX || overlapItem.onGridPositionY != posY)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private bool CheckAvaibleSpace(int posX, int posY, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (inventoryItemSlot[posX + x, posY + y] != null)
                    {

                        return false;
                    }
                }
            }

            return true;
        }

        private bool PositionCheck(int posX, int posY)
        {
            if (posX < 0 || posY < 0)
            {
                return false;
            }

            if (posX >= gridSizeWidth || posY >= gridSizeHeight)
            {
                return false;
            }

            return true;
        }

        public bool BoundryCheck(int posX, int posY, int width, int height)
        {
            if (PositionCheck(posX, posY) == false)
            {
                return false;
            }

            posX += width - 1;
            posY += height - 1;

            if (PositionCheck(posX, posY) == false)
            {
                return false;
            }

            return true;
        }
    }
}