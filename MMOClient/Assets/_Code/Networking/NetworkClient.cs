using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using Project.Utility;
using System;
using BV;
using Project.Scriptable;

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

        private ManagersController managersController;

        public static string ClientID { get; private set; }

        private Dictionary<string, NetworkIdentity> serverObjects;

        [HideInInspector]
        private bool isConnected = false;

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            initialize();
            setupEvents();
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
        }

        private void initialize()
        {
            serverObjects = new Dictionary<string, NetworkIdentity>();
            managersController = ManagersController.singleton;
        }

        private void setupEvents()
        {
            On("open", (E) =>
            {
                if (!isConnected)
                {
                    Debug.Log("Connection made to the server");
                    AttemptToJoinLobby();
                    isConnected = true;
                }
            });

            On("register", (E) =>
            {
                ClientID = E.data["id"].ToString().RemoveQuotes();
                Debug.LogFormat("Our Client's ID ({0})", ClientID);
            });

            On("spawn", (E) =>
            {
                //Handling all spawning all players
                //Passed Data
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(E.data.ToString());

                GameObject go = Instantiate(playerPrefab, networkContainer);
                go.name = string.Format("Player ({0})", playerData.id);
                go.transform.position = playerData.position;
                go.transform.rotation = Quaternion.Euler(playerData.rotation.x, playerData.rotation.y, playerData.rotation.z);

                NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                ni.SetControllerID(playerData.id);
                ni.SetSocketReference(this);

                serverObjects.Add(playerData.id, ni);

                if (ni.IsControlling() && managersController != null)
                {
                    StateManager states = go.GetComponent<StateManager>();
                    managersController.Init(this, states, playerData);
                }
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
                    enemyData.isControlling = ClientID == enemyData.playerSpawnedId;
                }

                NetworkIdentity ni = serverObjects[enemyData.id];
                ni.setIsControling(enemyData.isControlling);

                EnemyManager enemyManager = ni.GetComponent<EnemyManager>();
                enemyManager.UpdateState(enemyData);
            });

            On("updateWeather", (E) =>
            {
                float timeOfDay = E.data["timeOfDay"].JSONObjectToFloat();
                float orbitSpeed = E.data["orbitSpeed"].JSONObjectToFloat();
                float TimeMultiplier = E.data["timeMultiplier"].JSONObjectToFloat();

                managersController.weatherManager.UpdateWeatherData(timeOfDay, orbitSpeed, TimeMultiplier);
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
                    ni.SetSocketReference(this);

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
                managersController.chatBehaviour.SendMessage(id, message);
            });

            On("triggerKillEvent", (E) =>
            {
                string id = E.data["id"].ToString().RemoveQuotes();
                managersController.damageManager.OnKillEvent(id);
                managersController.notificationManager.AddNewMessage("Ви вбили: " + id);
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

        public GameObject getPlayer()
        {
            return playerPrefab;
        }

        public void AttemptToJoinLobby()
        {
            Emit("joinGame");
        }
    }
}