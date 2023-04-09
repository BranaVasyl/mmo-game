using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class Login : MonoBehaviour
{
    private const string PASSWORD_REGEX = "(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{8,24})";
    [SerializeField] private string loginEndpoint = "http://127.0.0.1:13756/account/login";
    [SerializeField] private string createEndpoint = "http://127.0.0.1:13756/account/create";

    [SerializeField] private TextMeshProUGUI alertText;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button createButton;
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;

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

        WWWForm form = new WWWForm();
        form.AddField("rUsername", username);
        form.AddField("rPassword", password);

        UnityWebRequest request = UnityWebRequest.Post(loginEndpoint, form);
        var handler = request.SendWebRequest();

        float startTime = 0;
        while (!handler.isDone)
        {
            startTime += Time.deltaTime;

            if (startTime > 10.0f)
            {
                break;
            }

            yield return null;
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

            if (response.code == 0)
            {
                Debug.Log(response.data);
                ActivateButtons(false);
                alertText.text = "Welcome " + ((response.data.adminFlag == 1) ? " Admin" : "");

                SceneManager.LoadScene("SampleScene");
            }
            else
            {
                switch (response.code)
                {
                    case 1:
                        alertText.text = "Invalid credentials";
                        ActivateButtons(true);
                        break;
                    default:
                        alertText.text = "Coruption detected";
                        ActivateButtons(false);
                        break;
                }

            }
        }
        else
        {
            alertText.text = "Error cnecting to the server ...";
            ActivateButtons(true);
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

        WWWForm form = new WWWForm();
        form.AddField("rUsername", username);
        form.AddField("rPassword", password);

        UnityWebRequest request = UnityWebRequest.Post(createEndpoint, form);
        var handler = request.SendWebRequest();

        float startTime = 0;
        while (!handler.isDone)
        {
            startTime += Time.deltaTime;

            if (startTime > 10.0f)
            {
                break;
            }

            yield return null;
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            CreateResponse response = JsonUtility.FromJson<CreateResponse>(request.downloadHandler.text);

            if (response.code == 0)
            {
                alertText.text = "Account has been created :" + response.data.username;
            }
            else
            {
                switch (response.code)
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
        }
        else
        {
            alertText.text = "Error cnecting to the server ...";
        }

        ActivateButtons(true);
        yield return null;
    }

    private void ActivateButtons(bool toogle)
    {
        loginButton.interactable = toogle;
        createButton.interactable = toogle;
    }
}
