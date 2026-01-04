using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using Project.Utility;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using Project.Networking;

namespace BV
{
    public class PickUpManager : MonoBehaviour
    {
        private SampleSceneManager managersController;
        private GameUIManager gameUIManager;

        public GameObject itemsContainer;
        private GridManager gridManager;

        [Header("Bag data")]
        private string currentbagId = "";

        [Header("Player data")]
        private List<GameObject> itemsObject = new List<GameObject>();
        private List<InventoryItemData> itemsData = new List<InventoryItemData>();

        public void Init(SampleSceneManager mC)
        {
            managersController = mC;
            gameUIManager = GameUIManager.singleton;

            gridManager = GridManager.singleton;
        }

        public void OpenBag(string bagId)
        {
            currentbagId = bagId;

            List<NetworkEvent> events = new List<NetworkEvent>();

            JSONObject bagData = new();
            bagData.AddField("bagId", currentbagId);

            events.Add(
                new NetworkEvent(
                    "bagOpen",
                    bagData,
                    (response) =>
                        {
                            gameUIManager.ShowBagUI();

                            InventoryGridDataListWrapper gridDataWrapper = JsonUtility.FromJson<InventoryGridDataListWrapper>(response[0].ToString());
                            SetBagData(gridDataWrapper.data);
                        }
                )
            );

            events.Add(
                new NetworkEvent(
                    "inventoryOpen",
                    null,
                    (response) =>
                        {
                            InventoryGridDataListWrapper gridDataWrapper = JsonUtility.FromJson<InventoryGridDataListWrapper>(response[0].ToString());
                            gridManager.SetData(gridDataWrapper.data);
                        }
                )
            );

            ApplicationManager.Instance.ShowSpinerLoader();
            NetworkRequestManager.Instance.EmitWithTimeoutAll(
                events,
                () =>
                    {
                        ApplicationManager.Instance.CloseSpinerLoader();
                        gridManager.updateItemPositionCallback.Add(UpdateItemPositionCallback);
                    },
                (msg) =>
                    {
                        ApplicationManager.Instance.CloseSpinerLoader();
                        ApplicationManager.Instance.ShowConfirmationModal("Не вдалося відкрити сумку", () =>
                            {
                                gameUIManager.HideBagUI();
                            });
                    }
            );
        }

        private async Task<bool> UpdateItemPositionCallback(UpdateItemPositionData itemUpdateData)
        {
            bool requestStatus = false;
            bool result = false;

            JSONObject sendData = new JSONObject(JsonUtility.ToJson(itemUpdateData));
            sendData.AddField("bagId", currentbagId);

            NetworkRequestManager.Instance.EmitWithTimeout(
                new NetworkEvent(
                    "bagChange",
                    sendData,
                    (response) =>
                        {
                            result = response[0]["result"].ToString() == "true";
                            requestStatus = true;
                        },
                    (msg) =>
                        {
                            requestStatus = true;
                            ApplicationManager.Instance.ShowConfirmationModal("Не вдалося підібрати передмет");
                        }
                )
            );

            while (!requestStatus)
            {
                await Task.Yield();
            }

            return result;
        }

        public void SetBagData(List<InventoryGridData> data)
        {
            CleanItemsList();
            
            gridManager.SetData(data);

            for (int j = 0; j < data.Count; j++)
            {
                GameObject itemTemplate = itemsContainer.transform.GetChild(0).gameObject;
                GameObject g;

                for (int i = 0; i < data[j].items.Count; i++)
                {
                    InventoryItemData inventoryItemData = data[j].items[i];
                    ItemData itemData = ItemsManager.Instance.GetItemById(inventoryItemData.item.code);
                    if (itemData == null)
                    {
                        continue;
                    }

                    g = Instantiate(itemTemplate, itemsContainer.transform);
                    g.transform.GetChild(0).GetComponent<Image>().sprite = itemData.smallIcon == null ? itemData.icon : itemData.smallIcon;
                    g.transform.GetChild(1).GetComponent<TMP_Text>().text = itemData.name;
                    g.transform.GetChild(2).GetComponent<TMP_Text>().text = itemData.type.ToString();

                    itemsData.Add(inventoryItemData);
                    itemsObject.Add(g);
                    g.SetActive(true);
                }

                for (int i = 0; i < itemsObject.Count; i++)
                {
                    itemsObject[i].GetComponent<Button>().AddEventListener(itemsData[i], ItemClicked);
                }
            }

            if (itemsData.Count == 0)
            {
                CloseBag();
            }
        }

        async void ItemClicked(InventoryItemData inventoryItem)
        {
            ItemData itemData = ItemsManager.Instance.GetItemById(inventoryItem.item.code);

            bool success = await GridManager.singleton.PickUpItem(inventoryItem);
            if (!success)
            {
                NotificationManager.singleton.AddNewMessage("Hемає місця для: " + itemData.name);
                return;
            }

            // //@todo add QuestEvent to item ... if questEvents.count do trigger events
            if (itemData.type == ItemType.quest)
            {
                string notificationTitle = "Отримано: Hовий квестовий предмет";
                string notificationSubtitle = itemData.name;
                Sprite notificationIcon = itemData.smallIcon != null ? itemData.smallIcon : itemData.icon;
                NotificationActionType action = NotificationActionType.log;

                NotificationManager.singleton.AddNewNotification(new NotificationData(notificationTitle, notificationSubtitle, notificationIcon, action));
            }
            NotificationManager.singleton.AddNewMessage("Отримано: " + itemData.name);
   
            int index = itemsData.FindIndex(i => i == inventoryItem);

            GameObject itemUI = itemsObject[index];
            itemsObject.Remove(itemUI);
            Destroy(itemUI);

            itemsData.Remove(inventoryItem);

            if (itemsData.Count == 0)
            {
                CloseBag();
            }
        }

        public async void TakeAllItems()
        {
            // List<ItemData> completed = new List<ItemData>();
            // foreach (ItemData item in itemsData)
            // {
            //     //request
            //     bool result = false;
            //     result = await PickUpItem(item.id);
            //     if (!result)
            //     {
            //         continue;
            //     }

            //     // result = GridManager.singleton.PickUpItem(item);
            //     if (!result)
            //     {
            //         continue;
            //     }

            //     completed.Add(item);
            // }

            // foreach (ItemData item in completed)
            // {
            //     int index = itemsData.FindIndex(i => i == item);

            //     GameObject itemUI = itemsObject[index];
            //     itemsObject.Remove(itemUI);
            //     Destroy(itemUI);

            //     itemsData.Remove(item);
            // }

            // if (itemsData.Count == 0)
            // {
            //     CloseBag();
            // }
        }

        private void CleanItemsList()
        {
            for (int i = 0; i < itemsObject.Count; i++)
            {
                Destroy(itemsObject[i]);
            }

            itemsObject = new List<GameObject>();
            itemsData = new List<InventoryItemData>();
        }

        public void CloseBag()
        {

            if (gridManager != null)
            {
                gridManager.Deinit();

                JSONObject bagData = new();
                bagData.AddField("bagId", currentbagId);
                NetworkClient.Instance.Emit("bagClose", bagData);

                NetworkClient.Instance.Emit("inventoryClose");
            }

            Clean();
            gameUIManager.HideBagUI();
        }

        public void Clean()
        {
            currentbagId = "";
            CleanItemsList();
        }

        public static PickUpManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
