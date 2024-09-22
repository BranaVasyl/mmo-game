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
        public TooltipManager tooltipManager;
        public PickUpManager pickUpManager;

        [Header("Current client")]
        [HideInInspector]
        public SocketIOComponent socket;
        [HideInInspector]
        public GameObject currentPlayerGameObject;
        [HideInInspector]
        public StateManager stateManager;

        [Header("Player Data")]
        public List<InventoryGridData> playerInventoryData = new List<InventoryGridData>();
        public List<InventoryGridData> playerEquipData = new List<InventoryGridData>();

        void Start()
        {
            socket = NetworkClient.Instance;

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
            tooltipManager = TooltipManager.singleton;
            pickUpManager = PickUpManager.singleton;
        }

        public void InitPlayer(StateManager sM, PlayerData pD)
        {
            currentPlayerGameObject = sM.gameObject;
            stateManager = sM;
            
            playerInventoryData = pD.inventoryData;
            playerEquipData = pD.playerEquipData;

            InitManagers();
        }

        private void InitManagers()
        {
            chatBehaviour.Init(this);
            gameUIManager.Init();
            dialogManager.Init();
            menuManager.Init(this);
            pieMenuManager.Init(this);
            damageManager.Init(this);
            questManager.Init();
            weatherManager.Init();
            itemsManager.Init();
            gridManager.Init();
            notificationManager.Init();
            tooltipManager.Init();
            pickUpManager.Init(this);
        }

        public static ManagersController singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
