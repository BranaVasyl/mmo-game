using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using Project.Utility;
using System;
using BV;
using Project.Scriptable;
using UnityEngine.Events;

namespace Project.Networking
{
    public class NetworkClient : SocketIOComponent
    {
        [Header("Network client")]
        [SerializeField]
        private Transform networkContainer;
        [SerializeField]
        private GameObject playerPrefab;
        [SerializeField]
        private ServerObjects serverSpawnables;

        public static string SessionID { get; private set; }

        private Dictionary<string, NetworkIdentity> serverObjects;

        [HideInInspector]
        private bool isConnected = false;

        [HideInInspector]
        public UnityEvent<GameObject, PlayerData> onPlayerSpawned;

        private static NetworkClient instance;
        public static NetworkClient Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("SessionManager not found. Make sure it exists in the scene.");
                }
                return instance;
            }
        }

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            Init();
            setupEvents();

            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                DestroyImmediate(this);
                return;
            }
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
        }

        private void Init()
        {
            serverObjects = new Dictionary<string, NetworkIdentity>();
        }

        private void setupEvents()
        {
            On("open", (E) =>
            {
                if (!isConnected)
                {
                    Debug.Log("Connection made to the server");
                    isConnected = true;
                }
            });

            On("register", (E) =>
            {
                SessionID = E.data["id"].ToString().RemoveQuotes();
                Debug.LogFormat("Our Client's ID ({0})", SessionID);
            });

            On("loadScene", (E) =>
            {
                if (E.data.HasField("sceneName"))
                {
                    string sceneNameKey = E.data["sceneName"].ToString().RemoveQuotes();

                    if (SceneList.sceneMapping.TryGetValue(sceneNameKey, out string sceneName))
                    {
                        SceneManagementManager.Instance.ReplaceLevel(sceneName, (levelName) => Emit("sceneLoaded"), true);
                    }
                    else
                    {
                        Debug.LogError($"Scene with the key {sceneNameKey} not found in the list.");
                    }
                }
                else
                {
                    Debug.LogError("SceneName not found in the event data.");
                }
            });

            On("spawn", (E) =>
            {
                //Handling all spawning all players
                //Passed Data
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(E.data["playerData"].ToString());

                GameObject go = Instantiate(playerPrefab, networkContainer);
                go.name = string.Format("Player ({0})", playerData.id);
                go.transform.position = playerData.position;
                go.transform.rotation = Quaternion.Euler(playerData.rotation.x, playerData.rotation.y, playerData.rotation.z);

                CharacterData characterData = playerData.characterData;
                GameObject character = CharactersController.Instance.CreateCharacter(characterData, go.transform);

                if (!character)
                {
                    Destroy(go);
                    return;
                }

                go.GetComponent<InventoryManager>().SetPlayerEquip(playerData.playerEquipData);

                NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                ni.SetControllerID(playerData.id);

                if (E.data["myself"].ToString() == "true")
                {
                    ni.setIsControling(true);
                    CameraManager.Instance.gameObject.transform.position = playerData.position;
                    onPlayerSpawned.Invoke(go, playerData);
                }

                serverObjects.Add(playerData.id, ni);
            });

            On("disconnected", (E) =>
            {
                string id = E.data["id"].ToString().RemoveQuotes();

                GameObject go = serverObjects[id].gameObject;
                Destroy(go); //Remove from game
                serverObjects.Remove(id); //Remove from memory
            });

            On("updatePlayers", (E) =>
            {
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(E.data.ToString());
                NetworkIdentity ni = serverObjects[playerData.id];
                StateManager stateManager = ni.GetComponent<StateManager>();

                if (ni.IsControlling())
                {
                    stateManager.health = playerData.health;
                    stateManager.money = playerData.money;

                    if (!stateManager.isDead && playerData.isDead)
                    {
                        stateManager.isDead = playerData.isDead;
                        stateManager.EnableRagdoll();
                    }
                    return;
                }

                stateManager.UpdateState(playerData);
            });

            On("updateAi", (E) =>
            {
                EnemyData enemyData = JsonUtility.FromJson<EnemyData>(E.data.ToString());
                if (enemyData.playerSpawnedId.Length > 0)
                {
                    enemyData.isControlling = SessionID == enemyData.playerSpawnedId;
                }

                NetworkIdentity ni = serverObjects[enemyData.id];
                ni.setIsControling(enemyData.isControlling);

                EnemyManager enemyManager = ni.GetComponent<EnemyManager>();
                enemyManager.UpdateState(enemyData);
            });

            On("updateWeather", (E) =>
            {
                if (WeatherManager.singleton != null)
                {
                    float timeOfDay = E.data.HasField("timeOfDay") ? E.data["timeOfDay"].JSONObjectToFloat() : 0f;
                    float orbitSpeed = E.data.HasField("orbitSpeed") ? E.data["orbitSpeed"].JSONObjectToFloat() : 0f;
                    float timeMultiplier = E.data.HasField("timeMultiplier") ? E.data["timeMultiplier"].JSONObjectToFloat() : 0f;

                    WeatherManager.singleton.UpdateWeatherData(timeOfDay, orbitSpeed, timeMultiplier);
                }
            });

            On("serverSpawn", (E) =>
            {
                string name = E.data["name"].str;
                string id = E.data["id"].ToString().RemoveQuotes();
                float x = E.data["position"]["x"].JSONObjectToFloat();
                float y = E.data["position"]["y"].JSONObjectToFloat();
                float z = E.data["position"]["z"].JSONObjectToFloat();

                float xr = E.data["rotation"]["x"].JSONObjectToFloat();
                float yr = E.data["rotation"]["y"].JSONObjectToFloat();
                float zr = E.data["rotation"]["z"].JSONObjectToFloat();
                Debug.LogFormat("Server wants us to spawn a '{0}'", name);

                if (!serverObjects.ContainsKey(id))
                {
                    ServerObjectData sod = serverSpawnables.GetObjectByName(name);
                    var spawnedObject = Instantiate(sod.Prefab, networkContainer);
                    spawnedObject.name = string.Format("{0} ({1})", name, id);

                    spawnedObject.transform.position = new Vector3(x, y, z);
                    spawnedObject.transform.rotation = Quaternion.Euler(xr, yr, zr);
                    var ni = spawnedObject.GetComponent<NetworkIdentity>();
                    ni.SetControllerID(id);

                    serverObjects.Add(id, ni);
                }
            });

            On("serverUnspawn", (E) =>
            {
                string id = E.data["id"].ToString().RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                serverObjects.Remove(id);
                DestroyImmediate(ni.gameObject);
            });

            On("sendMessage", (E) =>
            {
                string id = E.data["id"].ToString().RemoveQuotes();
                string message = E.data["message"].ToString().RemoveQuotes();
                ChatBehaviour.singleton.SendMessage(id, message);
            });

            On("triggerKillEvent", (E) =>
            {
                string id = E.data["id"].ToString().RemoveQuotes();
                DamageManager.singleton.OnKillEvent(id);
                NotificationManager.singleton.AddNewMessage("Ви вбили: " + id);
            });

            On("setBagData", (E) =>
            {
                InventoryGridData bagData = JsonUtility.FromJson<InventoryGridData>(E.data.ToString());
                PickUpManager.singleton.SetBagData(bagData.items);
            });

            On("setChestData", (E) =>
            {
                InventoryGridData gridData = JsonUtility.FromJson<InventoryGridData>(E.data.ToString());
                ChestController.singleton.SetChestData(gridData);
            });

            On("setShopData", (E) =>
            {
                InventoryGridData gridData = JsonUtility.FromJson<InventoryGridData>(E.data.ToString());
                float money = E.data["money"].JSONObjectToFloat();

                ShopController.singleton.SetShopData(gridData, money);
            });
        }
    }
}