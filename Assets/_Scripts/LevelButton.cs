using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _levelName;
    [SerializeField] TextMeshProUGUI _levelVerificationState;
    [SerializeField] Image _preview;

    LevelData _levelData;
    ParticularLevelWindowHandler _levelWindowHandler;

    public void Initialize(string jsonData, ParticularLevelWindowHandler particularLevelWindowHandler)
    {
        LevelData levelData = JsonUtility.FromJson<LevelData>(jsonData);
        _levelData = levelData;
        _levelName.SetText(levelData.LevelName);
        _levelWindowHandler = particularLevelWindowHandler;

        if (levelData.IsLevelVerified)
            _levelVerificationState.SetText("Verified");
        else
            _levelVerificationState.SetText("Unverified");
    }

    public void OnButtonPressed()
    {
        _levelWindowHandler.ActivateWindow(_levelData);
    }
}
