using System.Collections;
using System.Collections.Generic;
using Project.Networking;
using UnityEngine;

namespace BV
{
    public class ChangeDangeChanger : MonoBehaviour, IInteractable
    {
        public string dangeId;
        public string description;

        public string GetDescription()
        {
            return description;
        }

        public void Interact(GameObject player)
        {
            NetworkClient.Instance.Emit("changeLobby", new JSONObject($"{{\"id\":\"{dangeId}\"}}"));
        }
    }
}
