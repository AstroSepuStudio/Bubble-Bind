using System.IO;
using UnityEngine;

public class OfflinePlayerLevelsFetcher : MonoBehaviour
{
    public GameObject _levelButtonPrefab; // The prefab to instantiate for each level
    public Transform _contentTransform; // The parent transform to instantiate the prefabs under
    [SerializeField] LevelSaver _levelSaver;
    [SerializeField] ParticularLevelWindowHandler _levelWindowHandler;
    [SerializeField] MainMenuCanvasManager _mainMenuCanvasManager;

    void Start()
    {
        FetchAndInstantiateLevels();
    }

    public void FetchAndInstantiateLevels()
    {
        for (int i = 0; i < _contentTransform.childCount; i++)
        {
            Destroy(_contentTransform.GetChild(i).gameObject);
        }

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

        string filePath = Path.Combine(LevelSaver.LocalLevelFolder, newLevelData.LevelName + ".json");

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
