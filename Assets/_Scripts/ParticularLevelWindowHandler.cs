using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static LevelData;

public class ParticularLevelWindowHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject _window;
    [SerializeField] MainMenuCanvasManager _mainMenuCanvasManager;
    [SerializeField] OfflinePlayerLevelsFetcher _offlinePlayerLevelsFetcher;
    [SerializeField] LevelSaver _levelSaver;
    [SerializeField] EditorElementDataBase _editorElementDataBase;

    [Header("Information")]
    [SerializeField] TextMeshProUGUI _levelName;
    [SerializeField] TextMeshProUGUI _levelDescription;
    [SerializeField] TextMeshProUGUI _levelDifficulty;

    [Header("Editing")]
    [SerializeField] TMP_InputField _levelNameIF;
    [SerializeField] TMP_InputField _levelDescriptionIF;
    [SerializeField] TMP_Dropdown _levelDifficultyDropdown;

    [Header("Preview")]
    [SerializeField] GameObject elementUIPrefab; // Prefab with an Image component
    [SerializeField] Transform elementUIParent; // Parent object to hold the instantiated images
    [SerializeField] List<Image> _instancedImages;
    [SerializeField] float _scaleMultiplier;
    [SerializeField] float _positionMultiplier;

    string _oldLevelName;
    LevelData.LevelDifficulty[] _difficulties;

    bool _initialized = false;

    void Start()
    {
        Initialize();
        _mainMenuCanvasManager.OnMainMenuRestored.AddListener(ActivateWindow);
    }

    void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        // Clear any existing options
        _levelDifficultyDropdown.ClearOptions();

        // Get all enum values
        _difficulties = (LevelData.LevelDifficulty[])System.Enum.GetValues(typeof(LevelData.LevelDifficulty));

        // Convert enum values to strings and add them to the dropdown
        foreach (LevelData.LevelDifficulty value in _difficulties)
        {
            _levelDifficultyDropdown.options.Add(new TMP_Dropdown.OptionData(value.ToString()));
        }

        // Optionally, set the default value
        _levelDifficultyDropdown.value = 0;
        _levelDifficultyDropdown.RefreshShownValue();
    }

    public void ActivateWindow(GameObject window)
    {
        if (window == _window)
            ActivateWindow(LevelLoader.CurrentLevelPath);
    }

    public void ActivateWindow(string levelPath)
    {
        // Get LevelData
        string encryptedJson = File.ReadAllText(levelPath);
        string json = EncryptionUtility.Decrypt(encryptedJson);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        LevelLoader.CurrentLevelPath = levelPath;
        _oldLevelName = levelData.LevelName;

        _levelName.SetText(levelData.LevelName);
        _levelDescription.SetText(levelData.LevelDescription);
        _levelDifficulty.SetText(levelData.Level_Difficulty.ToString());

        _levelNameIF.text = levelData.LevelName;
        _levelDescriptionIF.text = levelData.LevelDescription;

        // Find the index of the target enum value
        int index = System.Array.IndexOf(_difficulties, levelData.Level_Difficulty);

        // Set the dropdown value to the found index
        _levelDifficultyDropdown.value = index;
        _levelDifficultyDropdown.RefreshShownValue();

        BuildLevelPreview(levelData);
    }

    public void DeactivateWindow(bool levelDeleted)
    {
        _window.SetActive(false);

        foreach (var item in _instancedImages)
        {
            item.gameObject.SetActive(false);
        }

        if (!levelDeleted)
            SaveChanges();
    }

    public void StartLevel()
    {
        SaveChanges();

        SceneManager.LoadScene("LevelTesting");
    }

    public void EditLevel()
    {
        SaveChanges();

        SceneManager.LoadScene("LevelEditor");
    }

    public void DeleteLevel()
    {
        // Check if the file exists
        if (File.Exists(LevelLoader.CurrentLevelPath))
        {
            // Delete the file
            File.Delete(LevelLoader.CurrentLevelPath);
        }

        _offlinePlayerLevelsFetcher.FetchAndInstantiateLevels();

        DeactivateWindow(true);
    }

    void BuildLevelPreview(LevelData levelData)
    {
        for (int i = 0; i < levelData.SavedElements.Count; i++)
        {
            SavedElement savedElement = levelData.SavedElements[i];
            EditorElementData elementData = _editorElementDataBase.EditorElementDatas[savedElement.DataIndex];

            if (i < _instancedImages.Count)
            {
                _instancedImages[i].gameObject.SetActive(true);

                _instancedImages[i].sprite = elementData.ElementIcon;
                _instancedImages[i].rectTransform.anchoredPosition = savedElement.position * _positionMultiplier;
                _instancedImages[i].rectTransform.localRotation = savedElement.rotation;
                _instancedImages[i].rectTransform.localScale = savedElement.scale * elementData.SpriteScale * _scaleMultiplier;
                continue;
            }

            // Instantiate the UI Image prefab and get references of interest
            GameObject elementUI = Instantiate(elementUIPrefab, elementUIParent);
            Image image = elementUI.GetComponent<Image>();
            RectTransform rectTransform = elementUI.GetComponent<RectTransform>();

            _instancedImages.Add(image);

            image.sprite = elementData.ElementIcon;
            rectTransform.anchoredPosition = savedElement.position * _positionMultiplier; // Use the saved position
            rectTransform.localRotation = savedElement.rotation; // Use the saved rotation
            rectTransform.localScale = savedElement.scale * elementData.SpriteScale * _scaleMultiplier; // Use the saved scale
        }
    }

    void SaveChanges()
    {
        // Get LevelData
        string encryptedJson = File.ReadAllText(LevelLoader.CurrentLevelPath);
        string json = EncryptionUtility.Decrypt(encryptedJson);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        levelData.LevelName = _levelNameIF.text;
        levelData.LevelDescription = _levelDescriptionIF.text;
        levelData.Level_Difficulty = (LevelDifficulty)_levelDifficultyDropdown.value;

        _levelSaver.SaveLevel(levelData, _oldLevelName);
        _offlinePlayerLevelsFetcher.FetchAndInstantiateLevels();
    }
}
