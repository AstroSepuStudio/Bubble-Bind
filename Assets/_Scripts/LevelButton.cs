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
        _levelWindowHandler.ActivateWindow(_levelPath);
    }
}
