using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.UI;
using Project.Networking;
using System;

namespace BV
{
    public class LoginSceneManager : MonoBehaviour
    {
        private const string PASSWORD_REGEX = "(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{8,24})";

        [Header("Login Form Data")]
        [SerializeField] private GameObject loginForm;
        [SerializeField] private TextMeshProUGUI alertText;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button createButton;
        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private TMP_InputField passwordInputField;

        [Header("Connection Server Data")]
        [SerializeField] private GameObject connectionForm;
        [SerializeField] private GameObject progressContainer;
        [SerializeField] private GameObject feedbackContainer;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button refreshButton;


        void Start()
        {
            loginForm.SetActive(true);
            connectionForm.SetActive(false);
            SceneManagementManager.Instance.LoadLevel(SceneList.ONLINE, (levelName) => { });
        }

        public void OnLoginClick()
        {
            alertText.text = "Sign in ...";
            ActivateButtons(false);

            StartCoroutine(TryLogin());
        }

        public void OnCreateClick()
        {
            alertText.text = "Creating account ...";
            ActivateButtons(false);

            StartCoroutine(TryCreate());
        }

        public void OnRefreshClick()
        {
            StartCoroutine(OnTryJoinTheServer());
        }

        public IEnumerator OnTryJoinTheServer()
        {
            ShowFeedback(false);

            bool isResponseReceived = false;
            float startTime = 0;
            float timeoutDuration = 10.0f;

            NetworkClient.Instance.Emit("joinServer", null, (response) =>
                {
                    isResponseReceived = true;
                    JoinServerResponse responseData = JsonUtility.FromJson<JoinServerResponse>(response[0].ToString());

                    if (responseData.code == 0)
                    {
                        SceneManagementManager.Instance.LoadLevel(SceneList.CHARACTER_CREATOR_SCENE, (levelName) =>
                            {
                                SceneManagementManager.Instance.UnloadLevel(SceneList.LOGIN_SCENE);
                            });
                    }
                    else
                    {
                        switch (responseData.code)
                        {
                            case 1:
                                feedbackText.text = "No available slots on the server";
                                break;
                            case 2:
                                feedbackText.text = "User is already connected to the server";
                                break;
                            default:
                                feedbackText.text = "Coruption detected";
                                break;
                        }

                        ShowFeedback(true);
                    };
                });

            while (!isResponseReceived)
            {
                startTime += Time.deltaTime;

                if (startTime > timeoutDuration)
                {
                    feedbackText.text = "Error cnecting to the server ...";
                    ShowFeedback(true);
                    yield break;
                }

                yield return null;
            }

            yield return null;
        }

        private IEnumerator TryLogin()
        {
            string username = usernameInputField.text;
            string password = passwordInputField.text;

            if (username.Length < 3 || username.Length > 24)
            {
                alertText.text = "Invalid username";
                ActivateButtons(true);
                yield break;
            }

            // if (!Regex.IsMatch(password, PASSWORD_REGEX))
            // {
            //     alertText.text = "Invalid credentials";
            //     ActivateButtons(true);
            //     yield break;
            // }

            JSONObject loginData = new();
            loginData.AddField("rUsername", username);
            loginData.AddField("rPassword", password);

            bool isResponseReceived = false;
            float startTime = 0;
            float timeoutDuration = 10.0f;

            NetworkClient.Instance.Emit("accountLogin", loginData, (response) =>
            {
                isResponseReceived = true;
                LoginResponse responseData = JsonUtility.FromJson<LoginResponse>(response[0].ToString());

                if (responseData.code == 0)
                {
                    ActivateButtons(false);
                    alertText.text = "Welcome " + ((responseData.data.adminFlag == 1) ? " Admin" : "");

                    loginForm.SetActive(false);
                    connectionForm.SetActive(true);
                    StartCoroutine(OnTryJoinTheServer());
                }
                else
                {
                    switch (responseData.code)
                    {
                        case 1:
                            alertText.text = "Invalid credentials";
                            break;
                        default:
                            alertText.text = "Coruption detected";
                            break;
                    }

                    ActivateButtons(true);
                }
            });

            while (!isResponseReceived)
            {
                startTime += Time.deltaTime;

                if (startTime > timeoutDuration)
                {
                    alertText.text = "Error cnecting to the server ...";
                    ActivateButtons(true);
                    yield break;
                }

                yield return null;
            }

            yield return null;
        }

        private IEnumerator TryCreate()
        {
            string username = usernameInputField.text;
            string password = passwordInputField.text;

            if (username.Length < 3 || username.Length > 24)
            {
                alertText.text = "Invalid username";
                ActivateButtons(true);
                yield break;
            }

            if (!Regex.IsMatch(password, PASSWORD_REGEX))
            {
                alertText.text = "Invalid credentials";
                ActivateButtons(true);
                yield break;
            }

            JSONObject loginData = new();
            loginData.AddField("rUsername", username);
            loginData.AddField("rPassword", password);

            bool isResponseReceived = false;
            float startTime = 0;
            float timeoutDuration = 10.0f;


            NetworkClient.Instance.Emit("accountCreate", loginData, (response) =>
            {
                isResponseReceived = true;
                CreateResponse responseData = JsonUtility.FromJson<CreateResponse>(response[0].ToString());

                if (responseData.code == 0)
                {
                    alertText.text = "Account has been created :" + responseData.data.username;
                }
                else
                {
                    switch (responseData.code)
                    {
                        case 0:
                            alertText.text = "Invalid credentails";
                            break;
                        case 2:
                            alertText.text = "Username is alredy taken";
                            break;
                        case 3:
                            alertText.text = "Password in unsafe";
                            break;
                        default:
                            alertText.text = "Coruption detected";
                            break;

                    }
                }

                ActivateButtons(true);
            });

            while (!isResponseReceived)
            {
                startTime += Time.deltaTime;

                if (startTime > timeoutDuration)
                {
                    alertText.text = "Error cnecting to the server ...";
                    ActivateButtons(true);
                    yield break;
                }

                yield return null;
            }

            yield return null;
        }

        private void ActivateButtons(bool toogle)
        {
            loginButton.interactable = toogle;
            createButton.interactable = toogle;
        }

        private void ShowFeedback(bool toogle)
        {
            progressContainer.SetActive(!toogle);
            feedbackContainer.SetActive(toogle);
        }
    }

    [Serializable]
    public class CreateResponse
    {
        public int code;
        public string msg;
        public GameAccount data;
    }

    [Serializable]
    public class LoginResponse
    {
        public int code;
        public string msg;
        public GameAccount data;
    }

    [Serializable]
    public class JoinServerResponse
    {
        public int code;
        public string msg;
    }

    [Serializable]
    public class GameAccount
    {
        public string _id;
        public int adminFlag;
        public string username;
    }
}
