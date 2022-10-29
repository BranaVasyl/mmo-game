using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using SocketIO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using BV;

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

        public void Show()
        {
            chatUI.SetActive(true);
        }

        public void Hide()
        {
            chatUI.SetActive(false);
        }


        public void Init(SocketIOComponent soc)
        {
            socket = soc;
            Show();
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
            Match matches = Regex.Match(message, @"(.*)\((.*)\)");
            if (matches.Groups.Count == 3)
            {
                switch (matches.Groups[1].Value)
                {
                    case "setTime":
                        inputField.text = "";
                        SetWorldTime(matches.Groups[2].Value);
                        return;
                        break;
                    default:
                        break;
                }
            }

            inputField.text = "";
            SendMessageData sendData = new SendMessageData(message);
            socket.Emit("sendMessage", new JSONObject(JsonUtility.ToJson(sendData)));
        }

        private void SetWorldTime(string options)
        {
            int hour = 0;
            int minute = 0;

            string[] param = options.Split(",");
            for (int i = 0; i < param.Length; i++)
            {
                if (i == 0)
                {
                    int temp = int.Parse(param[i]);
                    if (temp > 24 || temp < 0)
                    {
                        temp = 0;
                    }

                    hour = temp;
                }

                if (i == 1)
                {
                    int temp = int.Parse(param[i]);
                    if (temp > 60 || temp < 0)
                    {
                        temp = 0;
                    }

                    minute = temp * 100 / 60;
                }
            }

            WeatherManager.singleton.timeOfDay = System.Convert.ToSingle(hour + "," + minute);
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
