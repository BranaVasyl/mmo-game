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
        private ApplicationManager applicationManager;

        private const string PASSWORD_REGEX = "(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{8,24})";

        [Header("Login Form")]
        [SerializeField] private GameObject loginForm;
        [SerializeField] private TextMeshProUGUI alertText;

        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button createButton;

        void Awake()
        {
            applicationManager = ApplicationManager.Instance;
        }

        void Start()
        {
            SceneManagementManager.Instance.LoadLevel(SceneList.ONLINE, (levelName) => { });

            loginForm.SetActive(true);
        }

        public void OnLoginClick()
        {
            TryLogin();
        }

        public void OnCreateClick()
        {
            TryCreate();
        }

        public void OnTryJoinTheServer()
        {
            ShowProgress("Триває підключення до сервера");

            NetworkRequestManager.Instance.EmitWithTimeout(
                "joinServer",
                null,
                (response) =>
                    {
                        applicationManager.CloseModal();

                        JoinServerResponse responseData = JsonUtility.FromJson<JoinServerResponse>(response[0].ToString());

                        if (responseData.code != 0)
                        {
                            string text = "";
                            switch (responseData.code)
                            {
                                case 1:
                                    text = "Не має вільного міся на сервері, спробуй пізніше";
                                    break;
                                case 2:
                                    text = "Такий користувач уже є в ігровій сесії";
                                    break;
                                default:
                                    text = "Щось пішло не так :(";
                                    break;
                            }

                            ShowFeedback(text);
                        };
                    },
                (msg) => ShowFeedback(msg)
            );
        }

        private void TryLogin()
        {
            ShowProgress("Триває підключення до сервера");

            string username = usernameInputField.text;
            string password = passwordInputField.text;

            if (username.Length < 3 || username.Length > 24)
            {
                ShowFeedback("Не правильне ім'я користувача");
                return;
            }

            // if (!Regex.IsMatch(password, PASSWORD_REGEX))
            // {
            //     ShowFeedback("Не правильні дані");
            //     return;
            // }

            JSONObject loginData = new();
            loginData.AddField("rUsername", username);
            loginData.AddField("rPassword", password);

            NetworkRequestManager.Instance.EmitWithTimeout(
                "accountLogin",
                loginData,
                (response) =>
                    {
                        applicationManager.CloseModal();

                        LoginResponse responseData = JsonUtility.FromJson<LoginResponse>(response[0].ToString());

                        if (responseData.code == 0)
                        {
                            OnTryJoinTheServer();
                        }
                        else
                        {
                            string text = "";
                            switch (responseData.code)
                            {
                                case 1:
                                    text = "Не правильні дані";
                                    break;
                                default:
                                    text = "Щось пішло не так :(";
                                    break;
                            }

                            ShowFeedback(text);
                        }
                    },
                (msg) => ShowFeedback(msg)
            );
        }

        private void TryCreate()
        {
            ShowProgress("Триває створення користувача");

            string username = usernameInputField.text;
            string password = passwordInputField.text;

            if (username.Length < 3 || username.Length > 24)
            {
                ShowFeedback("Не правильне ім'я користувача");
                return;
            }

            if (!Regex.IsMatch(password, PASSWORD_REGEX))
            {
                ShowFeedback("Не правильні дані");
                return;
            }

            JSONObject loginData = new();
            loginData.AddField("rUsername", username);
            loginData.AddField("rPassword", password);

            NetworkRequestManager.Instance.EmitWithTimeout(
                "accountCreate",
                loginData,
                (response) =>
                    {
                        applicationManager.CloseModal();

                        CreateResponse responseData = JsonUtility.FromJson<CreateResponse>(response[0].ToString());

                        string text = "";
                        if (responseData.code == 0)
                        {
                            text = "Аккаунт (" + responseData.data.username + ") успішно створено";
                        }
                        else
                        {
                            switch (responseData.code)
                            {
                                case 0:
                                    text = "Не правильні дані";
                                    break;
                                case 2:
                                    text = "Ім'я корисувача зайняте";
                                    break;
                                case 3:
                                    text = "Пароль не надійний";
                                    break;
                                default:
                                    text = "Щось пішло не так :(";
                                    break;

                            }
                        }

                        ShowFeedback(text);
                    },
                (msg) => ShowFeedback(msg)
            );
        }

        private void ShowFeedback(string fT = "")
        {
            loginForm.SetActive(false);
            applicationManager.ShowConfirmationModal(fT, () =>
            {
                loginForm.SetActive(true);
            });
        }

        private void ShowProgress(string pT = "")
        {
            loginForm.SetActive(false);
            applicationManager.ShowInformationModal(pT);
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
