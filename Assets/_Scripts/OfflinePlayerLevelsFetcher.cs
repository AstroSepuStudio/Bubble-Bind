using System.IO;
using UnityEngine;

public class OfflinePlayerLevelsFetcher : MonoBehaviour
{
    public GameObject _levelButtonPrefab; // The prefab to instantiate for each level
    public Transform _contentTransform; // The parent transform to instantiate the prefabs under
    [SerializeField] LevelSaver _levelSaver;
    [SerializeField] ParticularLevelWindowHandler _levelWindowHandler;

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

        string folderPath = Path.Combine(Application.persistentDataPath, "PlayerLevelData");
        // Get all .json files in the persistent data folder
        string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

        foreach (string filePath in jsonFiles)
        {
            // Read the JSON data from the file
            string jsonData = File.ReadAllText(filePath);

            // Instantiate the prefab
            GameObject levelInstance = Instantiate(_levelButtonPrefab, _contentTransform);

            // Get the component that will handle the initialization
            LevelButton levelInitializer = levelInstance.GetComponent<LevelButton>();

            if (levelInitializer != null)
            {
                // Pass the JSON data to the prefab for initialization
                levelInitializer.Initialize(jsonData, _levelWindowHandler);
            }
            else
            {
                Debug.LogError("LevelInitializer component not found on the prefab.");
            }
        }
    }

    public void CreateNewLevel()
    {
        // Create a new LevelData object
        LevelData newLevelData = new LevelData();
        newLevelData.LevelName = _levelSaver.GenerateDefaultName();

        // Convert the LevelData object to JSON
        string jsonData = JsonUtility.ToJson(newLevelData, true);

        string folderPath = Path.Combine(Application.persistentDataPath, "PlayerLevelData");
        // Define the file path for the new level
        string filePath = Path.Combine(folderPath, newLevelData.LevelName + ".json");

        // Write the JSON data to the file
        File.WriteAllText(filePath, jsonData);

        // Instantiate the prefab for the new level
        GameObject levelInstance = Instantiate(_levelButtonPrefab, _contentTransform);

        // Get the LevelInitializer component
        LevelButton levelInitializer = levelInstance.GetComponent<LevelButton>();

        if (levelInitializer != null)
        {
            // Pass the JSON data to the prefab for initialization
            levelInitializer.Initialize(jsonData, _levelWindowHandler);
        }
    }
}
