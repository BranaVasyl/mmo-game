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
        public WeatherManager weatherManager;

        [Header("Player Data")]
        public GameObject currentPlayerGameObject;

        void Start()
        {
            NetworkClient.Instance.onPlayerSpawned.AddListener((go, pD) =>
            {
                currentPlayerGameObject = go;
                InitManagers();
            });

            weatherManager = WeatherManager.singleton;
        }

        private void InitManagers()
        {
            weatherManager.Init();
        }

        public static SampleSceneManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
