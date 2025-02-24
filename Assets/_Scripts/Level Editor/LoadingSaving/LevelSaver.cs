using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelSaver : MonoBehaviour
{
    public static string LocalLevelFolder { get { return Path.Combine(Application.persistentDataPath, "PlayerLevelData"); } }
    public static string OnlineLevelFolder { get { return Path.Combine(Application.persistentDataPath, "DownloadedLevelData"); } }

    [SerializeField] private LevelEditorManager _levelEditorManager;
    [SerializeField] LevelLoader _levelLoader;

    public string GenerateDefaultName()
    {
        int index = 0;
        string fileName = $"newLevel ({index})";
        while (File.Exists(Path.Combine(LocalLevelFolder, fileName + ".json")))
        {
            fileName = $"newLevel ({index})";
            index++;
        }

        return fileName;
    }

    public void SaveLevel()
    {
        // Get LevelData
        string encryptedJson = File.ReadAllText(LevelLoader.CurrentLevelPath);
        string json = EncryptionUtility.Decrypt(encryptedJson);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        // Ensure the folder exists
        if (!Directory.Exists(LocalLevelFolder))
            Directory.CreateDirectory(LocalLevelFolder);

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

        json = JsonUtility.ToJson(levelData, true);
        encryptedJson = EncryptionUtility.Encrypt(json);

        File.WriteAllText(LevelLoader.CurrentLevelPath, encryptedJson);
        Debug.Log($"Level saved to: {LevelLoader.CurrentLevelPath}");
    }

    public void SaveLevel(LevelData levelData, string oldName)
    {
        string filePath;

        if (!oldName.Equals(levelData.LevelName))
        {
            filePath = Path.Combine(LocalLevelFolder, oldName + ".json");

            File.Delete(filePath);
        }

        filePath = Path.Combine(LocalLevelFolder, levelData.LevelName + ".json");

        // Ensure the folder exists
        if (!Directory.Exists(LocalLevelFolder))
            Directory.CreateDirectory(LocalLevelFolder);

        string json = JsonUtility.ToJson(levelData, true);
        string encryptedJson = EncryptionUtility.Encrypt(json);
        File.WriteAllText(filePath, encryptedJson);
    }
}
