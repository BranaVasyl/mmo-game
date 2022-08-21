using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;

namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class EnemyNetworkTransform : MonoBehaviour
    {
        private NetworkIdentity networkIdentity;

        void Start()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
        }

        public void SendData()
        {
            Vector3 position = new Vector3();
            position.x = Mathf.Round(transform.position.x * 1000.0f) / 1000.0f;
            position.y = Mathf.Round(transform.position.y * 1000.0f) / 1000.0f;
            position.z = Mathf.Round(transform.position.z * 1000.0f) / 1000.0f;

            SendEnemyPositionData sendData = new SendEnemyPositionData(position, networkIdentity.GetID());
            networkIdentity.GetSocket().Emit("updateEnemyPosition", new JSONObject(JsonUtility.ToJson(sendData)));
        }
    }

    [Serializable]
    public class SendEnemyPositionData
    {
        public string id;
        public string x;
        public string y;
        public string z;

        public SendEnemyPositionData(Vector3 position, string enemyId)
        {
            id = enemyId;
            x = position.x.ToString();
            y = position.y.ToString();
            z = position.z.ToString();
        }
    }
}
