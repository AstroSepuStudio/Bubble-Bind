using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class AuthResponse
{
    public string message;
    public string token;
}

[System.Serializable]
public class UserCredentials
{
    public string nombre;
    public string email;
    public string password;
}

public class AuthenticationManager : MonoBehaviour
{
    [SerializeField] string LOGIN_URL = "http://localhost/BubbleBindBackend/PHP/api/login.php";
    [SerializeField] string REGISTER_URL = "http://localhost/BubbleBindBackend/PHP/api/registro.php";
    [SerializeField] string tokenFilePath;

    [SerializeField] MainMenuCanvasManager _mainCanvasManager;
    [SerializeField] GameObject _loginWindow;
    [SerializeField] GameObject _registerWindow;

    [Header("Login")]
    [SerializeField] TMP_InputField _loginMailIF;
    [SerializeField] TMP_InputField _loginPasswordIF;

    [Header("Register")]
    [SerializeField] TMP_InputField _registerNameIF;
    [SerializeField] TMP_InputField _registerMailIF;
    [SerializeField] TMP_InputField _registerPasswordIF;
    [SerializeField] TMP_InputField _registerConfirmPasswordIF;

    private void Awake()
    {
        tokenFilePath = Path.Combine(Application.persistentDataPath, $"{tokenFilePath}.json");
    }

    public void TryLogin()
    {
        if (!EmailValidator.IsValidEmail(_loginMailIF.text))
        {
            _mainCanvasManager.SendAlertMessage("Invalid email");
            return;
        }

        if (_loginPasswordIF.text.Equals(""))
        {
            _mainCanvasManager.SendAlertMessage("Password can't be null");
            return;
        }

        Login(_loginMailIF.text, _loginPasswordIF.text);
    }

    public void TryRegister()
    {
        if (_registerNameIF.text.Equals(""))
        {
            _mainCanvasManager.SendAlertMessage("Name can't be null");
            return;
        }

        if (!EmailValidator.IsValidEmail(_registerMailIF.text))
        {
            _mainCanvasManager.SendAlertMessage("Invalid email");
            return;
        }

        if (_registerPasswordIF.text.Equals(""))
        {
            _mainCanvasManager.SendAlertMessage("Password can't be null");
            return;
        }

        if (!_registerPasswordIF.text.Equals(_registerConfirmPasswordIF.text))
        {
            _mainCanvasManager.SendAlertMessage("passwords do not match");
            return;
        }

        Register(_registerNameIF.text, _registerMailIF.text, _registerPasswordIF.text);
    }

    void Register(string nombre, string email, string password)
    {
        UserCredentials credentials = new UserCredentials();
        credentials.nombre = nombre;
        credentials.email = email;
        credentials.password = password;

        StartCoroutine(SendAuthRequest(REGISTER_URL, credentials, (success, response) =>
        {
            AuthResponse authResponse = JsonUtility.FromJson<AuthResponse>(response);

            if (success)
            {
                _mainCanvasManager.SendAlertMessage(authResponse.message, () => _mainCanvasManager.CloseWindow(_registerWindow));
            }
            else
            {
                _mainCanvasManager.SendAlertMessage(authResponse.message);
            }
        }));
    }

    void Login(string email, string password)
    {
        UserCredentials credentials = new UserCredentials();
        credentials.email = email;
        credentials.password = password;

        StartCoroutine(SendAuthRequest(LOGIN_URL, credentials, (success, response) =>
        {
            AuthResponse authResponse = JsonUtility.FromJson<AuthResponse>(response);

            if (success)
            {
                if (!string.IsNullOrEmpty(authResponse.token))
                {
                    File.WriteAllText(tokenFilePath, JsonUtility.ToJson(authResponse));
                }
                _mainCanvasManager.SendAlertMessage(authResponse.message, () => _mainCanvasManager.CloseWindow(_loginWindow));
            }
            else
            {
                _mainCanvasManager.SendAlertMessage(authResponse.message);
            }
        }));

    }

    public void Logout()
    {
        if (File.Exists(tokenFilePath))
        {
            File.Delete(tokenFilePath);
        }
    }

    public bool IsLoggedIn()
    {
        return File.Exists(tokenFilePath);
    }

    public string GetToken()
    {
        if (File.Exists(tokenFilePath))
        {
            string json = File.ReadAllText(tokenFilePath);
            AuthResponse authResponse = JsonUtility.FromJson<AuthResponse>(json);
            return authResponse.token;
        }
        return null;
    }

    private IEnumerator SendAuthRequest(string url, UserCredentials postData, Action<bool, string> callback)
    {
        string json = JsonUtility.ToJson(postData);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }
}


public class EmailValidator
{
    public static bool IsValidEmail(string email)
    {
        string regex = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";

        return Regex.IsMatch(email, regex, RegexOptions.IgnoreCase);
    }
}
