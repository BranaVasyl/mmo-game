using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using Project.Networking;

namespace BV
{
    public class ManagersController : MonoBehaviour
    {
        public ChatBehaviour chatBehaviour;
        public MenuManager menuManager;
        public PieMenuManager pieMenuManager;
        public DamageManager damageManager;

        private SocketIOComponent socket;
        private StateManager states;
        private PlayerData playerData;

        void Start()
        {
            chatBehaviour = ChatBehaviour.singleton;
            menuManager = MenuManager.singleton;
            pieMenuManager = PieMenuManager.singleton;
            damageManager = DamageManager.singleton;
        }

        public void Init(SocketIOComponent soc, StateManager sm, PlayerData pd)
        {
            socket = soc;
            states = sm;
            playerData = pd;

            InitManagers();
        }

        private void InitManagers()
        {
            chatBehaviour.Init(socket);
            menuManager.Init(socket, playerData);
            pieMenuManager.Init(states.GetComponent<InventoryManager>());
            damageManager.Init(socket);
        }

        public static ManagersController singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
