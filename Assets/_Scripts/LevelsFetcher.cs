using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LevelsFetcher : MonoBehaviour
{
    public GameObject _levelButtonPrefab; // The prefab to instantiate for each level
    public Transform _contentTransform; // The parent transform to instantiate the prefabs under
    [SerializeField] AuthenticationManager _authenticationManager;
    [SerializeField] LevelSaver _levelSaver;
    [SerializeField] ParticularLevelWindowHandler _levelWindowHandler;
    [SerializeField] MainMenuCanvasManager _mainMenuCanvasManager;
    [SerializeField] GameObject _newLevelButton;

    [SerializeField] string _lvlDownloadURL = "http://localhost/BubbleBindBackend/PHP/api/getlvl.php";
    [SerializeField] string _lvlname;

    public void FetchOnlineLevels()
    {
        for (int i = 0; i < _contentTransform.childCount; i++)
        {
            Destroy(_contentTransform.GetChild(i).gameObject);
        }

        _newLevelButton.SetActive(false);

        //StartCoroutine(GetLevelByID(1));

        for (int i = 0; i < 10; i++)
        {
            StartCoroutine(GetLevelByID(i));
        }
    }

    private IEnumerator GetLevelByID(int id)
    {
        // Agregar token JWT en los encabezados si es necesario
        string token = _authenticationManager.GetToken(); // Recupera el token JWT almacenado
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("Player not authenticated");
            yield break;
        }

        // Agregar el ID como parámetro en la URL
        string url = $"{_lvlDownloadURL}?id={id}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + _authenticationManager.GetToken());

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Error fetching level: {request.error}");
                yield break;
            }

            LevelUploadRequestData response = JsonUtility.FromJson<LevelUploadRequestData>(request.downloadHandler.text);

            if (!string.IsNullOrEmpty(response.error))
            {
                Debug.LogWarning(response.error);
                yield break;
            }

            // Instantiate the prefab
            GameObject levelInstance = Instantiate(_levelButtonPrefab, _contentTransform);
            LevelButton levelInitializer = levelInstance.GetComponent<LevelButton>();

            if (levelInitializer != null)
                levelInitializer.Initialize(response, _levelWindowHandler, _mainMenuCanvasManager);
        }
    }
    
    public void FetchDownloadedLevels()
    {
        for (int i = 0; i < _contentTransform.childCount; i++)
        {
            Destroy(_contentTransform.GetChild(i).gameObject);
        }

        // Get all .json files in the persistent data folder
        string[] jsonFiles = Directory.GetFiles(LevelSaver.OnlineLevelFolder, "*.json");

        foreach (string filePath in jsonFiles)
        {
            // Instantiate the prefab
            GameObject levelInstance = Instantiate(_levelButtonPrefab, _contentTransform);
            LevelButton levelInitializer = levelInstance.GetComponent<LevelButton>();

            if (levelInitializer != null)
                levelInitializer.Initialize(filePath, _levelWindowHandler, _mainMenuCanvasManager);
        }
    }

    public void FetchPlayerLevels()
    {
        for (int i = 0; i < _contentTransform.childCount; i++)
        {
            Destroy(_contentTransform.GetChild(i).gameObject);
        }

        _newLevelButton.SetActive(true);

        // Get all .json files in the persistent data folder
        string[] jsonFiles = Directory.GetFiles(LevelSaver.LocalLevelFolder, "*.json");

        foreach (string filePath in jsonFiles)
        {
            // Instantiate the prefab
            GameObject levelInstance = Instantiate(_levelButtonPrefab, _contentTransform);
            LevelButton levelInitializer = levelInstance.GetComponent<LevelButton>();

            if (levelInitializer != null)
                levelInitializer.Initialize(filePath, _levelWindowHandler, _mainMenuCanvasManager);
        }
    }

    public void CreateNewLevel()
    {
        // Create a new LevelData object
        LevelData newLevelData = new();
        newLevelData.LevelName = _levelSaver.GenerateDefaultName();
        newLevelData.ID = _levelSaver.GetNewLocalID();

        string filePath = Path.Combine(LevelSaver.LocalLevelFolder, newLevelData.ID + ".json");

        string json = JsonUtility.ToJson(newLevelData, true);
        string encryptedJson = EncryptionUtility.Encrypt(json);
        File.WriteAllText(filePath, encryptedJson);

        // Instantiate the prefab for the new level
        GameObject levelInstance = Instantiate(_levelButtonPrefab, _contentTransform);

        // Get the LevelInitializer component
        LevelButton levelInitializer = levelInstance.GetComponent<LevelButton>();

        if (levelInitializer != null)
        {
            // Pass the JSON data to the prefab for initialization
            levelInitializer.Initialize(filePath, _levelWindowHandler, _mainMenuCanvasManager);
        }
    }
}
