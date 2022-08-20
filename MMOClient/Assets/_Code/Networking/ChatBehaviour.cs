using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using SocketIO;

namespace Project.Networking
{
    public class ChatBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject chatUI = null;

        [SerializeField]
        private TMP_Text chatText = null;

        [SerializeField]
        private TMP_InputField inputField = null;

        private SocketIOComponent socket;

        public static ChatBehaviour singleton;
        private void Awake()
        {
            singleton = this;
        }

        void Start()
        {
            chatUI.gameObject.SetActive(true);
        }

        public void Init(SocketIOComponent soc)
        {
            socket = soc;
        }

        public void SendMessage(string id, string message)
        {
            string newMessage = "";
            if (chatText.text.Length > 0)
            {
                newMessage += "\n\n";
            }

            newMessage += string.Format("[{0}]: {1}", id, message);

            chatText.text += newMessage;
        }

        public void OnSendMessage(string message)
        {
            inputField.text = "";
            SendMessageData sendData = new SendMessageData(message);
            socket.Emit("sendMessage", new JSONObject(JsonUtility.ToJson(sendData)));
        }

    }

    [Serializable]
    public class SendMessageData
    {
        public string message;

        public SendMessageData(string sendMessage)
        {
            message = sendMessage;
        }
    }
}
