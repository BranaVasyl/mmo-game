using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;

namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class EnemyNetworkRotation : MonoBehaviour
    {
        private NetworkIdentity networkIdentity;

        void Start()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
        }

        public void SendData()
        {
            Vector3 rotation = new Vector3();
            rotation.x = Mathf.Round(transform.rotation.eulerAngles.x * 1000.0f) / 1000.0f;
            rotation.y = Mathf.Round(transform.rotation.eulerAngles.y * 1000.0f) / 1000.0f;
            rotation.z = Mathf.Round(transform.rotation.eulerAngles.z * 1000.0f) / 1000.0f;

            SendEnemyRotationData sendData = new SendEnemyRotationData(rotation, networkIdentity.GetID());
            NetworkClient.Instance.Emit("updateEnemyRotation", new JSONObject(JsonUtility.ToJson(sendData)));
        }
    }

    [Serializable]
    public class SendEnemyRotationData
    {
        public string id;
        public string x;
        public string y;
        public string z;

        public SendEnemyRotationData(Vector3 rotation, string enemyId)
        {
            id = enemyId;
            x = rotation.x.ToString();
            y = rotation.y.ToString();
            z = rotation.z.ToString();
        }
    }
}
