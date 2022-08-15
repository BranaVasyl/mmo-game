using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;

namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkTransform : MonoBehaviour
    {
        [SerializeField]
        [GreyOut]
        private Vector3 oldPosition;

        private NetworkIdentity networkIdentity;
        private Player player;

        private float stillCounter = 0;

        void Start()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
            oldPosition = transform.position;
            player = new Player();
            player.position = new Position();
            player.position.x = 0;
            player.position.y = 0;
            player.position.z = 0;

            if (!networkIdentity.IsControlling())
            {
                enabled = false;
            }
        }

        void Update()
        {
            if (networkIdentity.IsControlling())
            {
                if (oldPosition != transform.position)
                {
                    oldPosition = transform.position;
                    stillCounter = 0;
                    sendData();
                }
                else
                {
                    stillCounter += Time.deltaTime;

                    if (stillCounter >= 1)
                    {
                        stillCounter = 0;
                        sendData();
                    }
                }
            }
        }

        private void sendData()
        {
            // //Update player information
            player.position.x = Mathf.Round(transform.position.x * 1000.0f) / 1000.0f;
            player.position.y = Mathf.Round(transform.position.y * 1000.0f) / 1000.0f;
            player.position.z = Mathf.Round(transform.position.z * 1000.0f) / 1000.0f;

            SendPositionData sendData = new SendPositionData(player);
            networkIdentity.GetSocket().Emit("updatePosition", new JSONObject(JsonUtility.ToJson(sendData)));
        }
    }

    [Serializable]
    public class SendPositionData
    {
        public string x;
        public string y;
        public string z;

        public SendPositionData(Player player)
        {
            x = player.position.x.ToString();
            y = player.position.y.ToString();
            z = player.position.z.ToString();

        }
    }
}
