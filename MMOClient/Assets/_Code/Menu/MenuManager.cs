using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Project.Networking;

namespace BV
{
    public class MenuManager : MonoBehaviour
    {
        public GameObject gameMenu;
        private bool isOpen;
        NetworkIdentity networkIdentity;
        public List<MenuPanel> menuPanels;
        private MenuPanel currentPanel;
        private int curPanelIndex = 0;

        [Header("Header Section")]
        public GameObject header;
        public GameObject panelName;

        [Header("Inventory Section")]
        public GameObject inventory;
        InventoryController inventoryController;

        void Start() {
            CloseMenu();
        }

        public void Init(NetworkIdentity nI)
        {
            networkIdentity = nI;
        }

        public bool IsOpen()
        {
            return isOpen;
        }

        public void OpenMenu()
        {
            gameMenu.SetActive(true);
            isOpen = true;

            currentPanel = menuPanels[curPanelIndex];
            panelName.GetComponent<TMP_Text>().text = currentPanel.name;
            currentPanel.gameObject.SetActive(true);
            currentPanel.Init(networkIdentity);
        }

        public void CloseMenu()
        {
            inventoryController = inventory.GetComponent<InventoryController>();

            panelName.GetComponent<TMP_Text>().text = "";
            for (int i = 0; i < menuPanels.Count; i++)
            {
                menuPanels[i].gameObject.SetActive(false);
            }

            gameMenu.SetActive(false);
            isOpen = false;
        }

        public void NextPanel()
        {
            CloseMenu();

            if (curPanelIndex == menuPanels.Count - 1)
            {
                curPanelIndex = 0;
            }
            else
            {
                curPanelIndex++;
            }

            OpenMenu();
        }

        public void PrevPanel()
        {
            CloseMenu();

            if (curPanelIndex == 0)
            {
                curPanelIndex = menuPanels.Count - 1;
            }
            else
            {
                curPanelIndex--;
            }

            OpenMenu();
        }

        public static MenuManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
