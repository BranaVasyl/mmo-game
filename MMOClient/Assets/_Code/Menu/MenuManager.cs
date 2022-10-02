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
        private PlayerData playerData;

        [Header("Header Section")]
        public GameObject header;
        public GameObject panelName;

        void Start()
        {
            CloseMenu();
        }

        public void Init(SocketIOComponent soc, PlayerData pD)
        {
            socket = soc;
            playerData = pD;

            menuPanels.ForEach(panel => panel.Init(socket, playerData));
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
            panelName.GetComponent<TMP_Text>().text = currentPanel.panelName;
            currentPanel.gameObject.SetActive(true);

            currentPanel.Open();
        }

        public void CloseMenu()
        {
            panelName.GetComponent<TMP_Text>().text = "";
            for (int i = 0; i < menuPanels.Count; i++)
            {
                menuPanels[i].Deinit();
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
