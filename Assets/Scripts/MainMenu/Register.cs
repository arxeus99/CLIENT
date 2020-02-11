using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;

public class Register : MonoBehaviour
{
    // Cached references
    public InputField emailInputField;
    public InputField passwordInputField;
    public InputField confirmPasswordInputField;
    public Button registerButton;
    public Text messageBoardText;
    public Player playerManager;

    string httpServer;

    private void Start()
    {
        playerManager = FindObjectOfType<Player>();
        httpServer = playerManager.GetHttpServer();
    }

    public void OnRegisterButtonClick()
    {
        StartCoroutine(Registrar());
    }

    private IEnumerator Registrar()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Account/Register", "POST");
        if (string.IsNullOrEmpty(emailInputField.text))
        {
            throw new NullReferenceException("Email can't be void");
        }
        else if (string.IsNullOrEmpty(passwordInputField.text))
        {
            throw new NullReferenceException("Password can't be void");
        }
        else if (passwordInputField.text != confirmPasswordInputField.text)
        {
            throw new Exception("Passwords don't match");
        }

        AspNetUserRegister newUser = new AspNetUserRegister();
        newUser.Email = emailInputField.text;
        newUser.Password = passwordInputField.text;
        newUser.ConfirmPassword = confirmPasswordInputField.text;

        string jsonData = JsonUtility.ToJson(newUser);
        byte[] dataToSend = Encoding.UTF8.GetBytes(jsonData);
        httpClient.uploadHandler = new UploadHandlerRaw(dataToSend);

        httpClient.SetRequestHeader("Content-Type", "application/json");

        httpClient.certificateHandler = new ByPassCertificate();
        yield return httpClient.SendWebRequest();  // Blocking call

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            //throw new Exception("OnRegisterButtonClick: Network Error");
            Debug.Log(httpClient.isNetworkError);
            Debug.Log(httpClient.isHttpError);
            Debug.Log(httpClient.error);
        }

        messageBoardText.text += "\n" + httpClient.responseCode;

        httpClient.Dispose();
    }

}
