using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _levelName;
    [SerializeField] TextMeshProUGUI _levelVerificationState;
    [SerializeField] Image _preview;

    string _levelPath;
    LevelUploadRequestData _downloadedLevel;
    MainMenuCanvasManager _mainMenuCanvasManager;
    ParticularLevelWindowHandler _levelWindowHandler;

    public void Initialize(string levelPath,
        ParticularLevelWindowHandler particularLevelWindowHandler,
        MainMenuCanvasManager canvasManager)
    {
        // Get LevelData
        string encryptedJson = File.ReadAllText(levelPath);
        string json = EncryptionUtility.Decrypt(encryptedJson);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        // Set up button
        _levelPath = levelPath;
        _levelName.SetText(levelData.LevelName);
        _levelWindowHandler = particularLevelWindowHandler;
        _mainMenuCanvasManager = canvasManager;

        if (levelData.IsLevelVerified)
            _levelVerificationState.SetText("Verified");
        else
            _levelVerificationState.SetText("Unverified");
    }

    public void OnButtonPressed()
    {
        _mainMenuCanvasManager.OpenWindow(_levelWindowHandler._window);

        if (_downloadedLevel != null)
        {
            _levelWindowHandler.ActivateWindow(_downloadedLevel);
            return;
        }

        string normalizedParent = Path.GetFullPath(Directory.GetParent(_levelPath).ToString());
        string normalizedLocal = Path.GetFullPath(LevelSaver.LocalLevelFolder);
        string normalizedOnline = Path.GetFullPath(LevelSaver.OnlineLevelFolder);

        if (normalizedParent.Equals(normalizedLocal))
        {
            _levelWindowHandler.ActivateWindow(_levelPath);
        }
        else if (normalizedParent.Equals(normalizedOnline))
        {
            _levelWindowHandler.ActivateWindow(_levelPath, true);
        }
    }

    public void Initialize(LevelUploadRequestData response, 
        ParticularLevelWindowHandler particularLevelWindowHandler, 
        MainMenuCanvasManager canvasManager)
    {
        // Set up button
        _downloadedLevel = response;
        _levelName.SetText(response.nombrelvl);
        _levelWindowHandler = particularLevelWindowHandler;
        _mainMenuCanvasManager = canvasManager;

        _levelVerificationState.SetText(response.dificultad);
    }
}
