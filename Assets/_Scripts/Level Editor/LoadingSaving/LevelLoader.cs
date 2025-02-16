using System.IO;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private LevelEditorManager _levelEditorManager;
    [SerializeField] LevelSaver _levelSaver;
    public static LevelData CurrentLevelData;

    private void Start()
    {
        LoadLevel();
    }

    // Forcefully loads an specific level (for testing the level editor)
    public void ForceLoadLevelData(string name)
    {
        string filePath = Path.Combine(LevelSaver.LocalLevelFolder, name + ".json");

        string jsonData = File.ReadAllText(filePath);
        LevelData levelData = JsonUtility.FromJson<LevelData>(jsonData);
        CurrentLevelData = levelData;
    }

    public void LoadLevel()
    {
        _levelEditorManager._cameraEditorElement.transform.position = CurrentLevelData.CameraPosition;
        _levelEditorManager.ChangeElementCameraSize(CurrentLevelData.CameraSize);

        foreach (SavedElement savedElement in CurrentLevelData.SavedElements)
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
        }
    }

    public void CreateNewLevel()
    {
        // Create a new LevelData object
        LevelData newLevelData = new LevelData();
        newLevelData.LevelName = _levelSaver.GenerateDefaultName();

        // Convert the LevelData object to JSON
        string jsonData = JsonUtility.ToJson(newLevelData, true);

        // Define the file path for the new level
        string filePath = Path.Combine(LevelSaver.LocalLevelFolder, newLevelData.LevelName + ".json");

        // Write the JSON data to the file
        File.WriteAllText(filePath, jsonData);

        CurrentLevelData = newLevelData;
    }
}
