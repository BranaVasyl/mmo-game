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
    public class ChatBehaviour : Singleton<ChatBehaviour>
    {
        [SerializeField]
        private TMP_Text chatText = null;

        [SerializeField]
        private TMP_InputField inputField = null;


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
            NetworkClient.Instance.Emit("sendMessage", new JSONObject(JsonUtility.ToJson(sendData)));
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
