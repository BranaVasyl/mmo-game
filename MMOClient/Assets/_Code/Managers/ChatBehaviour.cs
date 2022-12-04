using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using SocketIO;
using System.Text.RegularExpressions;
using BV;

namespace Project.Networking
{
    public class ChatBehaviour : MonoBehaviour
    {
        private ManagersController managersController;

        [SerializeField]
        private TMP_Text chatText = null;

        [SerializeField]
        private TMP_InputField inputField = null;

        public void Init(ManagersController mC)
        {
            managersController = mC;
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
            managersController.socket.Emit("sendMessage", new JSONObject(JsonUtility.ToJson(sendData)));
        }

        public static ChatBehaviour singleton;
        private void Awake()
        {
            singleton = this;
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
