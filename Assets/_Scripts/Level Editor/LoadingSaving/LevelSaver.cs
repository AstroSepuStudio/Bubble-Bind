using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelSaver : MonoBehaviour
{
    [SerializeField] private LevelEditorManager _levelEditorManager;
    [SerializeField] LevelLoader _levelLoader;

    public string GenerateDefaultName()
    {
        int index = 0;
        string fileName = $"newLevel ({index})";
        string folderPath = Path.Combine(Application.persistentDataPath, "PlayerLevelData");
        while (File.Exists(Path.Combine(folderPath, fileName + ".json")))
        {
            fileName = $"newLevel ({index})";
            index++;
        }

        return fileName;
    }

    public void SaveLevel()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "PlayerLevelData");
        string filePath = Path.Combine(folderPath, LevelLoader.LevelName + ".json");

        // Ensure the folder exists
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        LevelData levelData = _levelLoader.GetLevelData();
        if (levelData != null)
            levelData.SavedElements.Clear();
        else
            levelData = new();

        levelData.CameraPosition = _levelEditorManager._cameraEditorElement.transform.position;
        levelData.CameraSize = _levelEditorManager._elementCamera.orthographicSize;
        // Iterate through all elements in _elementsData to get the index
        for (int i = 0; i < _levelEditorManager._elementDataBase.EditorElementDatas.Length; i++)
        {
            EditorElementData data = _levelEditorManager._elementDataBase.EditorElementDatas[i];

            // Iterate through all instanced elements
            foreach (var element in _levelEditorManager._instancedElements)
            {
                if (element._data == data)
                {
                    SavedElement savedElement = new()
                    {
                        ElementIndex = element.ElementIndex,
                        DataIndex = i,
                        position = element.transform.position,
                        rotation = element.transform.rotation,
                        scale = element.transform.localScale,
                        ElementIntegerValues = new List<int>(element.ElementIntegerValues),
                        ElementFloatValues = new List<float>(element.ElementFloatValues),
                        ElementVectors = new List<Vector3>(element.ElementVectors),
                        Physics = element.Physics
                    };

                    levelData.SavedElements.Add(savedElement);
                }
            }
        }

        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Level saved to: {filePath}");
    }

    public void SaveLevel(LevelData levelData, string oldName)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "PlayerLevelData");
        string filePath = Path.Combine(folderPath, oldName + ".json");

        // Ensure the folder exists
        if (Directory.Exists(folderPath))
            File.Delete(filePath);
        else
            return;

        folderPath = Path.Combine(Application.persistentDataPath, "PlayerLevelData");
        filePath = Path.Combine(folderPath, levelData.LevelName + ".json");

        // Ensure the folder exists
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Level saved to: {filePath}");
    }
}
