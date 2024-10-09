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
        public GridManager gridManager;
        public NotificationManager notificationManager;
        public TooltipManager tooltipManager;
        public PickUpManager pickUpManager;

        [Header("Player Data")]
        public GameObject currentPlayerGameObject;

        void Start()
        {
            NetworkClient.Instance.onPlayerSpawned.AddListener((go, pD) =>
            {
                currentPlayerGameObject = go;
                InitManagers();
            });

            chatBehaviour = ChatBehaviour.singleton;
            menuManager = MenuManager.singleton;
            pieMenuManager = PieMenuManager.singleton;
            damageManager = DamageManager.singleton;
            questManager = QuestManager.singleton;
            gameUIManager = GameUIManager.singleton;
            dialogManager = DialogManager.singleton;
            weatherManager = WeatherManager.singleton;
            gridManager = GridManager.singleton;
            notificationManager = NotificationManager.singleton;
            tooltipManager = TooltipManager.singleton;
            pickUpManager = PickUpManager.singleton;
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
