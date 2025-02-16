using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _levelName;
    [SerializeField] TextMeshProUGUI _levelVerificationState;
    [SerializeField] Image _preview;

    LevelData _levelData;
    MainMenuCanvasManager _mainMenuCanvasManager;
    ParticularLevelWindowHandler _levelWindowHandler;

    public void Initialize(LevelData levelData, 
        ParticularLevelWindowHandler particularLevelWindowHandler, 
        MainMenuCanvasManager canvasManager)
    {
        _levelData = levelData;
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
        _levelWindowHandler.ActivateWindow(_levelData);
    }
}
