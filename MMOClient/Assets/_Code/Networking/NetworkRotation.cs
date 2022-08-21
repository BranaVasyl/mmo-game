using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;
using BV;

namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkRotation : MonoBehaviour
    {
        [SerializeField]
        [GreyOut]
        private Vector3 oldRotation;

        private NetworkIdentity networkIdentity;
        private PlayerData player;

        private float stillCounter = 0;

        void Start()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
            oldRotation = transform.rotation.eulerAngles;

            player = new PlayerData();
            player.rotation = Quaternion.Euler(0, 0, 0);

            if (!networkIdentity.IsControlling())
            {
                enabled = false;
            }
        }

        void Update()
        {
            if (networkIdentity.IsControlling())
            {
                if (oldRotation != transform.rotation.eulerAngles)
                {
                    oldRotation = transform.rotation.eulerAngles;
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
            //Update player information
            player.rotation.x = Mathf.Round(transform.rotation.eulerAngles.x * 1000.0f) / 1000.0f;
            player.rotation.y = Mathf.Round(transform.rotation.eulerAngles.y * 1000.0f) / 1000.0f;
            player.rotation.z = Mathf.Round(transform.rotation.eulerAngles.z * 1000.0f) / 1000.0f;

            SendRotationData sendData = new SendRotationData(player);
            networkIdentity.GetSocket().Emit("updateRotation", new JSONObject(JsonUtility.ToJson(sendData)));
        }
    }

    [Serializable]
    public class SendRotationData
    {
        public string x;
        public string y;
        public string z;

        public SendRotationData(PlayerData player)
        {
            x = player.rotation.x.ToString();
            y = player.rotation.y.ToString();
            z = player.rotation.z.ToString();
        }
    }
}
