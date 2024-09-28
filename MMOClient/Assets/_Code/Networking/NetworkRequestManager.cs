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
    }
}

