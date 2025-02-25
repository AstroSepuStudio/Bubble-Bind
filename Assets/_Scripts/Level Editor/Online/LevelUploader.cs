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
    private string backendUrl = "http://localhost/BubbleBindBackend/PHP/api/savelvl.php";
    [SerializeField] string jwtToken = "TU_TOKEN_JWT";

    public void UploadCurrentLevelToBackend()
    {
        StartCoroutine(UploadLevelToBackend());
    }

    public IEnumerator UploadLevelToBackend()
    {
        if (!File.Exists(LevelLoader.CurrentLevelPath))
        {
            Debug.LogError("El archivo JSON no existe: " + LevelLoader.CurrentLevelPath);
            yield break;
        }

        string encryptedJson = File.ReadAllText(LevelLoader.CurrentLevelPath);
        string json = EncryptionUtility.Decrypt(encryptedJson);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        if (levelData == null || string.IsNullOrEmpty(levelData.LevelName) ||
            string.IsNullOrEmpty(levelData.LevelDescription) || string.IsNullOrEmpty(encryptedJson))
        {
            Debug.LogError("Error: Alguno de los campos está vacío.");
            yield break;
        }

        var requestData = new LevelUploadRequest
        {
            nombrelvl = levelData.LevelName,
            descripcion = levelData.LevelDescription,
            dificultad = levelData.Level_Difficulty.ToString(),
            datos_cifrados = encryptedJson
        };

        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(backendUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + jwtToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Respuesta del servidor: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error al enviar el nivel: " + request.error + "\nRespuesta: " + request.downloadHandler.text);
            }
        }
    }
}
