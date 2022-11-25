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
        public QuestManager questManager;
        public GameUIManager gameUIManager;
        public DialogManager dialogManager;
        public WeatherManager weatherManager;
        public ItemsManager itemsManager;
        public GridManager gridManager;
        public NotificationManager notificationManager;

        [Header("Current client")]
        [HideInInspector]
        public SocketIOComponent socket;
        [HideInInspector]
        public GameObject currentPlayerGameObject;
        [HideInInspector]
        public StateManager stateManager;
        [HideInInspector]
        public PlayerData playerData;

        void Start()
        {
            chatBehaviour = ChatBehaviour.singleton;
            menuManager = MenuManager.singleton;
            pieMenuManager = PieMenuManager.singleton;
            damageManager = DamageManager.singleton;
            questManager = QuestManager.singleton;
            gameUIManager = GameUIManager.singleton;
            dialogManager = DialogManager.singleton;
            weatherManager = WeatherManager.singleton;
            itemsManager = ItemsManager.singleton;
            gridManager = GridManager.singleton;
            notificationManager = NotificationManager.singleton;
        }

        public void Init(SocketIOComponent soc, StateManager sM, PlayerData pD)
        {
            socket = soc;

            currentPlayerGameObject = sM.gameObject;
            stateManager = sM;
            playerData = pD;

            InitManagers();
        }

        private void InitManagers()
        {
            chatBehaviour.Init(this);
            gameUIManager.Init();
            menuManager.Init(this);
            pieMenuManager.Init(this);
            damageManager.Init(this);
            questManager.Init();
            weatherManager.Init();
            itemsManager.Init();
            gridManager.Init();
            notificationManager.Init();
        }

        public static ManagersController singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
