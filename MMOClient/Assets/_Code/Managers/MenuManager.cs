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
        private List<MenuPanel> activePanels;

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

        //@todo filter menuPanels by array id
        public void OpenMenu(List<string> panelsId = null)
        {
            gameMenu.SetActive(true);
            isOpen = true;

            if (panelsId == null)
            {
                activePanels = menuPanels;
            }
            else
            {
                for (int i = 0; i < panelsId.Count; i++)
                {
                    MenuPanel panel = menuPanels.Find(x => x.panelId == panelsId[i]);
                    if (panel != null)
                    {
                        activePanels.Add(panel);
                    }
                }

                curPanelIndex = 0;
            }

            if (activePanels.Count == 0)
            {
                return;
            }

            OpenPanel();
        }

        public void CloseMenu()
        {
            ClosePanels();

            activePanels = new List<MenuPanel>();
            gameMenu.SetActive(false);
            isOpen = false;
        }

        private void OpenPanel()
        {
            currentPanel = activePanels[curPanelIndex];
            panelName.GetComponent<TMP_Text>().text = currentPanel.panelName;
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

        public static MenuManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
