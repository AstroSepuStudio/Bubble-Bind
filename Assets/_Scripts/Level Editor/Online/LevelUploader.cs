using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class LevelUploadRequest
{
    public string nombrelvl;
    public string descripcion;
    public string dificultad;
    public string datos_cifrados;
}

public class LevelUploader : MonoBehaviour
{
    // URL del endpoint del backend
    [SerializeField] string backendUrl = "http://localhost/BubbleBindBackend/PHP/api/savelvl.php";
    [SerializeField] AuthenticationManager _authenticationManager;
    [SerializeField] MainMenuCanvasManager _mainCanvasManager;

    public void UploadCurrentLevelToBackend()
    {
        if (_authenticationManager.IsLoggedIn())
            StartCoroutine(UploadLevelToBackend());
        else
            _mainCanvasManager.SendAlertMessage("You need to log in in order to upload a level");
    }

    // Función para cargar un nivel cifrado y enviarlo al backend
    IEnumerator UploadLevelToBackend()
    {
        if (!File.Exists(LevelLoader.CurrentLevelPath))
        {
            _mainCanvasManager.SendAlertMessage("The JSON file does not exist: " + LevelLoader.CurrentLevelPath);
            yield break;
        }

        string encryptedJson = File.ReadAllText(LevelLoader.CurrentLevelPath);
        string json = EncryptionUtility.Decrypt(encryptedJson);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        // 2. Crear los datos para enviar al backend
        LevelUploadRequestData requestData = new()
        {
            nombrelvl = levelData.LevelName,
            descripcion = levelData.LevelDescription,
            dificultad = levelData.Level_Difficulty.ToString(),
            datos_cifrados = encryptedJson
        };

        string jsonData = JsonUtility.ToJson(requestData);

        // 3. Configurar la solicitud HTTP POST
        using (UnityWebRequest request = new UnityWebRequest(backendUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + _authenticationManager.GetToken());

            // 4. Enviar la solicitud
            yield return request.SendWebRequest();

            LevelUploadResponse response = JsonUtility.FromJson<LevelUploadResponse>(request.downloadHandler.text);

            if (request.result == UnityWebRequest.Result.Success)
            {
                _mainCanvasManager.SendAlertMessage(response.message);
            }
            else
            {
                _mainCanvasManager.SendAlertMessage("Error while uploading the level: " + request.error);
            }
        }
    }
}

[System.Serializable]
public class LevelUploadRequestData
{
    public string nombrelvl;
    public string descripcion;
    public string dificultad;
    public string datos_cifrados;
}

[System.Serializable]
public class LevelUploadResponse
{
    public string message;
}
