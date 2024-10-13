using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Project.Networking;

namespace BV
{
    public class NetworkRequestManager : Singleton<NetworkRequestManager>
    {
        public void EmitWithTimeout(string eventName, JSONObject data, Action<JSONObject> onSuccess, Action<string> onError = null, float timeout = 10.0f)
        {
            StartCoroutine(EmitWithTimeoutCoroutine(eventName, data, onSuccess, onError, timeout));
        }

        private IEnumerator EmitWithTimeoutCoroutine(string eventName, JSONObject data, Action<JSONObject> onSuccess, Action<string> onError, float timeout)
        {
            bool isResponseReceived = false;

            NetworkClient.Instance.Emit(eventName, data, (response) =>
            {
                if (isResponseReceived)
                {
                    return;
                };

                isResponseReceived = true;
                onSuccess?.Invoke(response);
            });

            float elapsedTime = 0f;
            while (!isResponseReceived)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime > timeout)
                {
                    onError?.Invoke("Не вдалося підключитися до сервера");
                    yield break;
                }

                yield return null;
            }
        }

        public void EmitWithTimeoutAll(List<NetworkEvent> events, System.Action onRequestSuccess, System.Action<string> onError = null, float timeout = 10.0f)
        {
            StartCoroutine(EmitWithTimeoutAllCoroutine(events, onRequestSuccess, onError, timeout));
        }

        private IEnumerator EmitWithTimeoutAllCoroutine(List<NetworkEvent> events, System.Action onRequestSuccess, System.Action<string> onError, float timeout)
        {
            List<bool> responsesReceived = new List<bool>(new bool[events.Count]);

            for (int i = 0; i < events.Count; i++)
            {
                int index = i;
                NetworkEvent networkEvent = events[i];

                NetworkClient.Instance.Emit(networkEvent.EventName, networkEvent.Data, (response) =>
                {
                    if (!responsesReceived[index])
                    {
                        responsesReceived[index] = true;
                        networkEvent.OnSuccess?.Invoke(response);
                    }
                });
            }

            float elapsedTime = 0f;
            while (true)
            {
                elapsedTime += Time.deltaTime;

                if (responsesReceived.TrueForAll(response => response))
                {
                    onRequestSuccess?.Invoke();
                    yield break;
                }

                if (elapsedTime > timeout)
                {
                    onError?.Invoke("Не вдалося підключитися до сервера");
                    yield break;
                }

                yield return null;
            }
        }
    }

    [Serializable]
    public class NetworkEvent
    {
        public string EventName { get; set; }
        public JSONObject Data { get; set; }
        public Action<JSONObject> OnSuccess { get; set; }
        public Action<JSONObject> OnError { get; set; }

        public NetworkEvent(string eventName, JSONObject data, Action<JSONObject> onSuccess, Action<JSONObject> onError = null)
        {
            EventName = eventName;
            Data = data;
            OnSuccess = onSuccess;
            OnError = onError;
        }
    }
}

