using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Project.Networking;
using SocketIO;

namespace BV
{
    public class MenuManager : MonoBehaviour
    {
        private SocketIOComponent socket;

        public GameObject gameMenu;
        public List<MenuPanel> menuPanels;
        private bool isOpen;
        private MenuPanel currentPanel;
        private int curPanelIndex = 0;
        private JSONObject playerData;

        [Header("Header Section")]
        public GameObject header;
        public GameObject panelName;

        [Header("Inventory Section")]
        public GameObject inventory;

        void Start()
        {
            CloseMenu();
        }

        public void Init(SocketIOComponent soc, JSONObject pD)
        {
            socket = soc;
            playerData = pD;

            menuPanels.ForEach(panel => panel.Init(playerData));
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
        }

        public void CloseMenu()
        {
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