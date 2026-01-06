using System.Collections.Generic;
using System.Collections;
using Project.Networking;
using UnityEngine;
using SocketIO;
using System;
using TMPro;
using Project.Networking;

namespace BV
{
    public class MenuManager : Singleton<MenuManager>
    {
        public GameObject gameMenu;
        public List<MenuPanel> menuPanels;
        private List<MenuPanel> activePanels = new List<MenuPanel>();

        private bool isOpen;
        private MenuPanel currentPanel;
        private int curPanelIndex = 0;

        [Header("Header Section")]
        public GameObject header;
        public GameObject panelName;
        public GameObject prevArrowNavigation;
        public GameObject nextArrowNavigation;
        public GameObject panelMoney;

        void Start()
        {
            CloseMenu();
            menuPanels.ForEach(panel => panel.Init( this));
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
            ChestController.singleton.currentChestId = chestId;

            List<string> activatePanel = new List<string>();
            activatePanel.Add("chest");

            OpenMenu(activatePanel);
        }

        public void OpenShop(CharacterManager states)
        {
            ShopController.singleton.characterId = states.networkIdentity.GetID();
            ShopController.singleton.characterName = states.displayedName;

            List<string> activatePanel = new List<string>();
            activatePanel.Add("shop");

            OpenMenu(activatePanel);
        }

        public void CloseMenu()
        {
            CloseActivePanels();
            panelMoney.GetComponent<TMP_Text>().text = "";

            activePanels = new List<MenuPanel>();

            prevArrowNavigation.SetActive(false);
            nextArrowNavigation.SetActive(false);

            gameMenu.SetActive(false);
            isOpen = false;
        }

        private void OpenPanel()
        {
            currentPanel = activePanels[curPanelIndex];

            panelName.GetComponent<TMP_Text>().text = currentPanel.panelName;

            //@todo get money in server
            // RenderMoney(managersController.currentPlayerGameObject.GetComponent<StateManager>().money);

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

        private void CloseAllPanels()
        {
            panelName.GetComponent<TMP_Text>().text = "";
            for (int i = 0; i < menuPanels.Count; i++)
            {
                menuPanels[i].Deinit();
                menuPanels[i].gameObject.SetActive(false);
            }
        }

        private void CloseActivePanels()
        {
            panelName.GetComponent<TMP_Text>().text = "";
            for (int i = 0; i < activePanels.Count; i++)
            {
                activePanels[i].Deinit();
                activePanels[i].gameObject.SetActive(false);
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

        public static MenuManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
