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
        public PieMenuManager pieMenuManager;
        public WeatherManager weatherManager;
        public TooltipManager tooltipManager;

        [Header("Player Data")]
        public GameObject currentPlayerGameObject;

        void Start()
        {
            NetworkClient.Instance.onPlayerSpawned.AddListener((go, pD) =>
            {
                currentPlayerGameObject = go;
                InitManagers();
            });

            pieMenuManager = PieMenuManager.singleton;
            weatherManager = WeatherManager.singleton;
            tooltipManager = TooltipManager.singleton;
        }

        private void InitManagers()
        {
            pieMenuManager.Init(this);
            weatherManager.Init();
            tooltipManager.Init();
        }

        public static SampleSceneManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
