using System.Collections.Generic;
using System.Collections;
using Project.Utility;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace BV
{
    public class PickUpManager : MonoBehaviour
    {
        private ManagersController managersController;
        private GameUIManager gameUIManager;

        public GameObject itemsContainer;
        public GameObject currentBagObject;

        private string currentbagId = "";

        [Header("Player data")]
        private string playerId;
        private List<GameObject> itemsObject = new List<GameObject>();
        private List<ItemData> itemsData = new List<ItemData>();

        public void Init(ManagersController mC)
        {
            managersController = mC;
            gameUIManager = GameUIManager.singleton;
        }

        public void OpenBag(string bagId, GameObject bagObject)
        {
            currentbagId = bagId;
            currentBagObject = bagObject;

            gameUIManager.ShowBagUI();

            playerId = managersController.stateManager.networkIdentity.GetID();
            managersController.socket.Emit("openBag", new JSONObject(JsonUtility.ToJson(new ChestData(currentbagId))));
        }

        public void SetBagData(List<InventoryItemData> items)
        {
            GameObject itemTemplate = itemsContainer.transform.GetChild(0).gameObject;
            GameObject g;

            for (int i = 0; i < items.Count; i++)
            {
                ItemData itemData = managersController.itemsManager.GetItemById(items[i].id);
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

        void ItemClicked(ItemData itemData)
        {
            int index = itemsData.FindIndex(i => i == itemData);
            if (index < 0)
            {
                Debug.Log("Something went wrong");
                return;
            }
            Debug.Log(index);

            GameObject itemUI = itemsObject[index];
            itemsObject.Remove(itemUI);
            Destroy(itemUI);

            GridManager.singleton.PickUpItem(itemData.id);
            itemsData.Remove(itemData);

            if (itemsData.Count == 0)
            {
                Destroy(currentBagObject);
                ClsoeBag();
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

        public void ClsoeBag()
        {
            Clean();
            currentBagObject = null;
            gameUIManager.HideBagUI();
        }

        public void Clean()
        {
            currentbagId = "";
            playerId = "";
            CleanItemsList();
        }

        public static PickUpManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
