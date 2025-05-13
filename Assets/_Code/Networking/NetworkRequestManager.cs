using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Project.Networking;

namespace BV
{
    public class NetworkRequestManager : Singleton<NetworkRequestManager>
    {
        public void EmitWithTimeout(NetworkEvent networkEvent, float timeout = 10.0f)
        {
            StartCoroutine(EmitWithTimeoutCoroutine(networkEvent, timeout));
        }

        private IEnumerator EmitWithTimeoutCoroutine(NetworkEvent networkEvent, float timeout)
        {
            bool isResponseReceived = false;

            NetworkClient.Instance.Emit(networkEvent.eventName, networkEvent.data, (response) =>
            {
                if (isResponseReceived)
                {
                    return;
                };

                isResponseReceived = true;
                networkEvent.onSuccess?.Invoke(response);
            });

            float elapsedTime = 0f;
            while (!isResponseReceived)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime > timeout)
                {
                    networkEvent.onError?.Invoke("Не вдалося підключитися до сервера");
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

                NetworkClient.Instance.Emit(networkEvent.eventName, networkEvent.data, (response) =>
                {
                    if (!responsesReceived[index])
                    {
                        responsesReceived[index] = true;
                        networkEvent.onSuccess?.Invoke(response);
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
        public string eventName { get; set; }
        public JSONObject data { get; set; }
        public Action<JSONObject> onSuccess { get; set; }
        public Action<string> onError { get; set; }

        public NetworkEvent(string eN, JSONObject d, Action<JSONObject> oS = null, Action<string> oE = null)
        {
            eventName = eN;
            data = d;
            onSuccess = oS;
            onError = oE;
        }
    }
}

