using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LevelUploader : MonoBehaviour
{
    // URL del endpoint del backend
    private string backendUrl = "http://localhost/BubbleBindBackend/PHP/api/savelvl.php";

    // Token JWT (debes obtenerlo previamente)
    private string jwtToken = "tu_token_jwt";

    public void UploadCurrentLevelToBackend()
    {
        StartCoroutine(UploadLevelToBackend());
    }

    // Función para cargar un nivel cifrado y enviarlo al backend
    public IEnumerator UploadLevelToBackend()
    {
        // 1. Leer el archivo JSON cifrado
        if (!File.Exists(LevelLoader.CurrentLevelPath))
        {
            Debug.LogError("El archivo JSON no existe: " + LevelLoader.CurrentLevelPath);
            yield break;
        }

        // Get LevelData
        string encryptedJson = File.ReadAllText(LevelLoader.CurrentLevelPath);
        string json = EncryptionUtility.Decrypt(encryptedJson);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        // 2. Crear los datos para enviar al backend
        var requestData = new
        {
            nombre = levelData.LevelName,
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
            request.SetRequestHeader("Authorization", "Bearer " + jwtToken);

            // 4. Enviar la solicitud
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Respuesta del servidor: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error al enviar el nivel: " + request.error);
            }
        }
    }

    // Función para cargar un nivel cifrado y enviarlo al backend
    public IEnumerator UploadLevelToBackend(string filePath, string levelName, string description, string difficulty)
    {
        // 1. Leer el archivo JSON cifrado
        if (!File.Exists(filePath))
        {
            Debug.LogError("El archivo JSON no existe: " + filePath);
            yield break;
        }

        string encryptedData = File.ReadAllText(filePath);

        // 2. Crear los datos para enviar al backend
        var requestData = new
        {
            nombre = levelName,
            descripcion = description,
            dificultad = difficulty,
            datos_cifrados = encryptedData
        };

        string jsonData = JsonUtility.ToJson(requestData);

        // 3. Configurar la solicitud HTTP POST
        using (UnityWebRequest request = new UnityWebRequest(backendUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + jwtToken);

            // 4. Enviar la solicitud
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Respuesta del servidor: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error al enviar el nivel: " + request.error);
            }
        }
    }
}
