using System.Collections.Generic;
using System.Collections;
using Project.Networking;
using UnityEngine;
using SocketIO;
using System;
using TMPro;

namespace BV
{
    public class MenuManager : MonoBehaviour
    {
        [HideInInspector]
        public ManagersController managersController;

        public GameObject gameMenu;
        public List<MenuPanel> menuPanels;
        private List<MenuPanel> activePanels;

        private bool isOpen;
        private MenuPanel currentPanel;
        private int curPanelIndex = 0;
        private PlayerData playerData;

        [Header("Header Section")]
        public GameObject header;
        public GameObject panelName;
        public GameObject prevArrowNavigation;
        public GameObject nextArrowNavigation;
        public GameObject panelMoney;

        [Header("Chest Data")]
        public string currentChestId = "";

        [Header("Chest Data")]
        public CharacterManager currentNPCStates;

        void Start()
        {
            CloseMenu();
        }

        public void Init(ManagersController mC)
        {
            managersController = mC;
            menuPanels.ForEach(panel => panel.Init(managersController, this));
        }

        public bool IsOpen()
        {
            return isOpen;
        }

        public void OpenMenu(List<string> panelsId = null)
        {
            gameMenu.SetActive(true);
            isOpen = true;

            if (panelsId == null)
            {
                panelsId = new List<string>() { "inventory", "stats", "journal" };
            }

            for (int i = 0; i < panelsId.Count; i++)
            {
                MenuPanel panel = menuPanels.Find(x => x.panelId == panelsId[i]);
                if (panel != null)
                {
                    activePanels.Add(panel);
                }
            }

            curPanelIndex = 0;
            if (activePanels.Count == 0)
            {
                return;
            }

            if (activePanels.Count > 1)
            {
                prevArrowNavigation.SetActive(true);
                nextArrowNavigation.SetActive(true);
            }

            OpenPanel();
        }

        public void OpenChest(string chestId)
        {
            currentChestId = chestId;

            List<string> activatePanel = new List<string>();
            activatePanel.Add("chest");
            activatePanel.Add("inventory");

            OpenMenu(activatePanel);
        }

        public void OpenShop(CharacterManager states)
        {
            currentNPCStates = states;

            List<string> activatePanel = new List<string>();
            activatePanel.Add("shop");

            OpenMenu(activatePanel);
        }

        public void CloseMenu()
        {
            ClosePanels();
            panelMoney.GetComponent<TMP_Text>().text = "";

            activePanels = new List<MenuPanel>();

            prevArrowNavigation.SetActive(false);
            nextArrowNavigation.SetActive(false);

            gameMenu.SetActive(false);
            isOpen = false;

            currentChestId = "";
            currentNPCStates = null;
        }

        private void OpenPanel()
        {
            currentPanel = activePanels[curPanelIndex];

            panelName.GetComponent<TMP_Text>().text = currentPanel.panelName;
            RenderMoney(managersController.stateManager.money);

            currentPanel.gameObject.SetActive(true);

            currentPanel.Open();
        }

        private void CloseActivePanel()
        {
            currentPanel = activePanels[curPanelIndex];
            panelName.GetComponent<TMP_Text>().text = "";
            currentPanel.Deinit();
            currentPanel.gameObject.SetActive(false);
        }

        private void ClosePanels()
        {
            panelName.GetComponent<TMP_Text>().text = "";
            for (int i = 0; i < menuPanels.Count; i++)
            {
                menuPanels[i].Deinit();
                menuPanels[i].gameObject.SetActive(false);
            }
        }

        public void NextPanel()
        {
            if (activePanels.Count <= 1)
            {
                return;
            }

            CloseActivePanel();

            if (curPanelIndex == activePanels.Count - 1)
            {
                curPanelIndex = 0;
            }
            else
            {
                curPanelIndex++;
            }

            OpenPanel();
        }

        public void PrevPanel()
        {
            if (activePanels.Count <= 1)
            {
                return;
            }

            CloseActivePanel();

            if (curPanelIndex == 0)
            {
                curPanelIndex = activePanels.Count - 1;
            }
            else
            {
                curPanelIndex--;
            }

            OpenPanel();
        }

        public void RenderMoney(float moneyCount)
        {
            panelMoney.GetComponent<TMP_Text>().text = moneyCount.ToString();
        }

        #region Global Functions
        public void UpdateInventoryData(InventoryGridData itemGridData)
        {
            if (itemGridData == null)
            {
                return;
            }

            int index = managersController.playerData.inventoryData.FindIndex(s => s.gridId == itemGridData.gridId);
            if (index == -1)
            {
                return;
            }

            managersController.playerData.inventoryData[index] = itemGridData;
            managersController.socket.Emit("syncInventoryData", new JSONObject(JsonUtility.ToJson(new SendInventoryData(managersController.playerData.inventoryData))));
        }
        #endregion


        public static MenuManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }

    [Serializable]
    public class SendUpdateMoneyData
    {
        public string id;
        public float addition;

        public SendUpdateMoneyData(string i, float a)
        {
            id = i;
            addition = a;
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
