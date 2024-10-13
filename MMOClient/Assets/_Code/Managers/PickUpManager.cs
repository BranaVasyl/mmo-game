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

        [Header("Bag data")]
        private GameObject currentBagObject;
        private string currentbagId = "";

        [Header("Player data")]
        private List<GameObject> itemsObject = new List<GameObject>();
        private List<ItemData> itemsData = new List<ItemData>();

        public void Init(SampleSceneManager mC)
        {
            managersController = mC;
            gameUIManager = GameUIManager.singleton;
        }

        public void OpenBag(string bagId, GameObject bagObject)
        {
            currentbagId = bagId;
            currentBagObject = bagObject;

            gameUIManager.ShowBagUI();

            JSONObject bagData = new();
            bagData.AddField("id", currentbagId);

            NetworkClient.Instance.Emit("openBag", bagData);
        }

        public void SetBagData(List<InventoryItemData> items)
        {
            GameObject itemTemplate = itemsContainer.transform.GetChild(0).gameObject;
            GameObject g;

            for (int i = 0; i < items.Count; i++)
            {
                ItemData itemData = ItemsManager.Instance.GetItemById(items[i].item.code);
                if (itemData == null)
                {
                    continue;
                }

                g = Instantiate(itemTemplate, itemsContainer.transform);
                g.transform.GetChild(0).GetComponent<Image>().sprite = itemData.smallIcon == null ? itemData.icon : itemData.smallIcon;
                g.transform.GetChild(1).GetComponent<TMP_Text>().text = itemData.name;
                g.transform.GetChild(2).GetComponent<TMP_Text>().text = itemData.type.ToString();

                itemsData.Add(itemData);
                itemsObject.Add(g);
                g.SetActive(true);
            }

            for (int i = 0; i < itemsObject.Count; i++)
            {
                itemsObject[i].GetComponent<Button>().AddEventListener(itemsData[i], ItemClicked);
            }
        }

        private async Task<bool> PickUpItem(string itemId)
        {
            return true;

            // bool requestStatus = false;
            // bool result = false;

            // SendChestPickUpData sendData = new SendChestPickUpData(NetworkClient.SessionID, currentbagId, itemId, 1);
            // NetworkClient.Instance.Emit("chestPickUp", new JSONObject(JsonUtility.ToJson(sendData)), (response) =>
            // {
            //     var data = response[0];
            //     result = data["result"].ToString() == "true";

            //     requestStatus = true;
            // });

            // while (!requestStatus)
            // {
            //     await Task.Yield();
            // }

            // return result;
        }

        async void ItemClicked(ItemData item)
        {
            //request
            bool result = false;
            result = await PickUpItem(item.id);
            if (!result)
            {
                return;
            }

            result = GridManager.singleton.PickUpItem(item);
            if (!result)
            {
                return;
            }

            int index = itemsData.FindIndex(i => i == item);

            GameObject itemUI = itemsObject[index];
            itemsObject.Remove(itemUI);
            Destroy(itemUI);

            itemsData.Remove(item);

            if (itemsData.Count == 0)
            {
                Debug.Log("Debu Remove Object");
                //Destroy(currentBagObject);
                CloseBag();
            }
        }

        public async void TakeAllItems()
        {
            List<ItemData> completed = new List<ItemData>();
            foreach (ItemData item in itemsData)
            {
                //request
                bool result = false;
                result = await PickUpItem(item.id);
                if (!result)
                {
                    continue;
                }

                result = GridManager.singleton.PickUpItem(item);
                if (!result)
                {
                    continue;
                }

                completed.Add(item);
            }

            foreach (ItemData item in completed)
            {
                int index = itemsData.FindIndex(i => i == item);

                GameObject itemUI = itemsObject[index];
                itemsObject.Remove(itemUI);
                Destroy(itemUI);

                itemsData.Remove(item);
            }

            if (itemsData.Count == 0)
            {
                Debug.Log("Debu Remove Object");
                //Destroy(currentBagObject);
                CloseBag();
            }
        }

        private void CleanItemsList()
        {
            for (int i = 0; i < itemsObject.Count; i++)
            {
                Destroy(itemsObject[i]);
            }

            itemsObject = new List<GameObject>();
            itemsData = new List<ItemData>();
        }

        public void CloseBag()
        {
            if (!String.IsNullOrEmpty(currentbagId))
            {
                JSONObject bagData = new();
                bagData.AddField("id", currentbagId);

                NetworkClient.Instance.Emit("closeBag", bagData);
            }

            Clean();
            currentBagObject = null;
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
