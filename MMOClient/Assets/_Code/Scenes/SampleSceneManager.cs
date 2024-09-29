using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using Project.Networking;
using Project.Utility;

namespace BV
{
    public class SampleSceneManager : MonoBehaviour
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

            for (var i = 0; i < pD.inventoryData.Count; i++)
            {
                InventoryGridData inventoryData = playerInventoryData.Find(el => el.gridId == pD.inventoryData[i].gridId);
                if (inventoryData != null)
                {
                    inventoryData.items = pD.inventoryData[i].items;
                }

                InventoryGridData equipData = playerEquipData.Find(el => el.gridId == pD.playerEquipData[i].gridId);
                if (equipData != null)
                {
                    equipData.items = pD.playerEquipData[i].items;
                }
            }

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

        public static SampleSceneManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
