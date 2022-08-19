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

        [SerializeField]
        private GameObject chatManager;
        [SerializeField]
        private InventoryController inventoryController;


        public static string ClientID { get; private set; }

        private Dictionary<string, NetworkIdentity> serverObjects;

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
        }

        private void setupEvents()
        {
            On("open", (E) =>
            {
                Debug.Log("Connection made to the server");
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
                string id = E.data["id"].ToString().RemoveQuotes();
                float x = E.data["position"]["x"].JSONObjectToFloat();
                float y = E.data["position"]["y"].JSONObjectToFloat();
                float z = E.data["position"]["z"].JSONObjectToFloat();

                float xr = E.data["rotation"]["x"].JSONObjectToFloat();
                float yr = E.data["rotation"]["y"].JSONObjectToFloat();
                float zr = E.data["rotation"]["z"].JSONObjectToFloat();

                GameObject go = Instantiate(playerPrefab, networkContainer);
                go.name = string.Format("Player ({0})", id);

                go.transform.position = new Vector3(x, y, z);
                go.transform.rotation = Quaternion.Euler(xr, yr, zr);

                NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                ni.SetControllerID(id);
                ni.SetSocketReference(this);
                serverObjects.Add(id, ni);

                if (chatManager)
                {
                    ChatBehaviour chatBehaviour = chatManager.GetComponent<ChatBehaviour>();
                    chatBehaviour.SetSocketReference(this);
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
                string id = E.data["id"].ToString().RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                StateManager stateManager = ni.GetComponent<StateManager>();

                bool isDead = E.data["isDead"].b;
                if (ni.IsControlling())
                {
                    if (!stateManager.isDead && isDead)
                    {
                        stateManager.isDead = isDead;
                        stateManager.EnableRagdoll();
                    }
                    return;
                }


                Vector3 newPosition = new Vector3(E.data["position"]["x"].JSONObjectToFloat(), E.data["position"]["y"].JSONObjectToFloat(), E.data["position"]["z"].JSONObjectToFloat());
                Quaternion newRotation = Quaternion.Euler(E.data["rotation"]["x"].JSONObjectToFloat(), E.data["rotation"]["y"].JSONObjectToFloat(), E.data["rotation"]["z"].JSONObjectToFloat());

                float horizontal = E.data["horizontal"].JSONObjectToFloat();
                float vertical = E.data["vertical"].JSONObjectToFloat();
                bool run = E.data["run"].b;
                bool walk = E.data["walk"].b;
                bool twoHanded = E.data["isTwoHanded"].b;
                string currentAnimation = E.data["currentAnimation"].ToString().RemoveQuotes();

                if (stateManager.currentAnimation != currentAnimation)
                {
                    stateManager.PlayAnimation(currentAnimation);
                }

                stateManager.UpdateState(newPosition, newRotation, horizontal, vertical, run, walk, twoHanded, isDead);
            });

            On("updateAi", (E) =>
            {
                string id = E.data["id"].ToString().RemoveQuotes();

                float x = E.data["position"]["x"].JSONObjectToFloat();
                float y = E.data["position"]["y"].JSONObjectToFloat();
                float z = E.data["position"]["z"].JSONObjectToFloat();

                float xr = E.data["rotation"]["x"].JSONObjectToFloat();
                float yr = E.data["rotation"]["y"].JSONObjectToFloat();
                float zr = E.data["rotation"]["z"].JSONObjectToFloat();
                bool isDead = E.data["isDead"].b;
                bool move = E.data["move"].b;
                bool isInteracting = E.data["isInteracting"].b;
                string currentAnimation = E.data["currentAnimation"].ToString().RemoveQuotes();
                string tempAnimationId = E.data["tempAnimationId"].ToString().RemoveQuotes();

                bool isControlling = false;

                string playerSpawnedId = E.data["playerSpawnedId"].ToString().RemoveQuotes();
                if (playerSpawnedId.Length > 0)
                {
                    isControlling = ClientID == playerSpawnedId;
                }

                NetworkIdentity ni = serverObjects[id];
                ni.setIsControling(isControlling);

                EnemyManager enemyManager = ni.GetComponent<EnemyManager>();
                enemyManager.UpdateState(new Vector3(x, y, z), Quaternion.Euler(xr, yr, zr), move, isInteracting, currentAnimation, tempAnimationId, isDead);
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
                if (chatManager == null)
                {
                    return;
                }

                ChatBehaviour chatBehaviour = chatManager.GetComponent<ChatBehaviour>();

                string id = E.data["id"].ToString().RemoveQuotes();
                string message = E.data["message"].ToString().RemoveQuotes();
                chatBehaviour.SendMessage(id, message);
            });

            On("setInventoryData", (E) =>
            {
                Debug.Log(E.data);
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

    [Serializable]
    public class Player
    {
        public string id;
        public Position position;
        public Rotation rotation;
        public float vertical;
        public float horizontal;
        public bool isDead;
        public bool run;
        public bool walk;
        public bool isTwoHanded;
        public string currentAnimation;
    }

    [Serializable]
    public class Position
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class Rotation
    {
        public float x;
        public float y;
        public float z;
    }
}