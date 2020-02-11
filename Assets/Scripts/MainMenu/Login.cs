using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    // Cached references
    public InputField emailInputField;
    public InputField passwordInputField;
    public Button loginButton;
    public Button logoutButton;
    public Button playGameButton;
    public Text messageBoardText;
    public Player playerManager;

    private string httpServerAddress;

    private void Start()
    {
        httpServerAddress = playerManager.GetHttpServer();
    }

    public void OnLoginButtonClicked()
    {
        TryLogin();
    }

    private void GetToken()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServerAddress + "/Token", "POST");

        WWWForm dataToSend = new WWWForm();
        dataToSend.AddField("grant_type", "password");
        dataToSend.AddField("username", emailInputField.text);
        dataToSend.AddField("password", passwordInputField.text);

        httpClient.uploadHandler = new UploadHandlerRaw(dataToSend.data);
        httpClient.downloadHandler = new DownloadHandlerBuffer();

        httpClient.SetRequestHeader("Accept", "application/json");
        httpClient.certificateHandler = new ByPassCertificate();
        httpClient.SendWebRequest();

        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }


        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            Debug.Log(httpClient.error);
        }
        else
        {
            string jsonResponse = httpClient.downloadHandler.text;
            AuthorizationToken authToken = JsonUtility.FromJson<AuthorizationToken>(jsonResponse);
            playerManager.Token = authToken.access_token;
        }

        httpClient.Dispose();
    }

    private void TryLogin()
    {
        if (string.IsNullOrEmpty(playerManager.Token))
        {
            GetToken();
        }
        UnityWebRequest httpClient = new UnityWebRequest(httpServerAddress + "/api/Account/UserId", "GET");
        httpClient.SetRequestHeader("Authorization", "bearer " + playerManager.Token);
        httpClient.SetRequestHeader("Accept", "application/json");

        httpClient.downloadHandler = new DownloadHandlerBuffer();
        httpClient.certificateHandler = new ByPassCertificate();
        httpClient.SendWebRequest();


        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            Debug.Log(httpClient.error);
        }
        else
        {
            playerManager.PlayerId = httpClient.downloadHandler.text;
            messageBoardText.text += "\nWelcome " + playerManager.PlayerId + ". You are logged in!";
            loginButton.interactable = false;
            playGameButton.interactable = true;
            logoutButton.interactable = true;
        }

        httpClient.Dispose();
    }

    public void OnLogoutButtonClicked()
    {
        TryLogout();
    }

    private void TryLogout()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServerAddress + "/api/Account/Logout", "POST");
        httpClient.SetRequestHeader("Authorization", "bearer " + playerManager.Token);
        httpClient.certificateHandler = new ByPassCertificate();
        httpClient.SendWebRequest();

        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            Debug.Log(httpClient.error);
        }
        else
        {
            messageBoardText.text += $"\n{httpClient.responseCode} Bye bye {playerManager.PlayerId}.";
            playerManager.Token = string.Empty;
            playerManager.PlayerId = string.Empty;
            loginButton.interactable = true;
            playGameButton.interactable = false;
            logoutButton.interactable = false;
        }
    }
}
