using System.IO;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public static string LevelName;
    [SerializeField] private LevelEditorManager _levelEditorManager;
    [SerializeField] LevelSaver _levelSaver;
    private LevelData _loadedLevelData;

    private void Start()
    {
        LoadLevel();
    }

    public LevelData GetLevelData() { return _loadedLevelData; }

    public void LoadLevel()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "PlayerLevelData");
        string filePath = Path.Combine(folderPath, LevelName + ".json");

        if (!File.Exists(filePath))
        {
            CreateNewLevel();
            Debug.LogWarning($"Level file {LevelName}.json not found, starting with an empty level.");
            return;
        }

        string json = File.ReadAllText(filePath);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        _loadedLevelData = levelData;

        _levelEditorManager._cameraEditorElement.transform.position = levelData.CameraPosition;
        _levelEditorManager.ChangeElementCameraSize(levelData.CameraSize);

        foreach (SavedElement savedElement in levelData.SavedElements)
        {
            if (savedElement.DataIndex < 0 || savedElement.DataIndex >= _levelEditorManager._elementDataBase.EditorElementDatas.Length)
            {
                Debug.LogWarning("Invalid element index in saved level data.");
                continue;
            }

            EditorElementData elementData = _levelEditorManager._elementDataBase.EditorElementDatas[savedElement.DataIndex];
            EditorElement instantiatedElement = _levelEditorManager.InstantiateElement(elementData, true, Vector3.zero);
            instantiatedElement.ElementIndex = savedElement.ElementIndex;
            instantiatedElement.transform.SetPositionAndRotation(savedElement.position, savedElement.rotation);
            instantiatedElement.transform.localScale = savedElement.scale;
            instantiatedElement.ElementFloatValues = savedElement.ElementFloatValues;
            instantiatedElement.ElementIntegerValues = savedElement.ElementIntegerValues;
            instantiatedElement.ElementVectors = savedElement.ElementVectors;
            instantiatedElement.Physics = savedElement.Physics;
        }

        foreach (var item in _levelEditorManager._instancedElements)
        {
            item.SetUpElement(_levelEditorManager);
        }

        if (_levelEditorManager._isPlayTesting)
        {
            _levelEditorManager.InitializeAllElements();
            Debug.Log($"Play Testing {LevelName}");
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

        _loadedLevelData = newLevelData;
    }
}
