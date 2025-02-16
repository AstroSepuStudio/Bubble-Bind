using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        // Add listener for when the value of the Dropdown changes
        _levelDifficultyDropdown.onValueChanged.AddListener((value) => DropdownValueChanged());
        _levelNameIF.onEndEdit.AddListener((value) => ChangeLevelName());
        _levelDescriptionIF.onEndEdit.AddListener((value) => ChangeLevelDescription());
    }

    public void ActivateWindow(GameObject window)
    {
        if (window == _window)
            ActivateWindow(LevelLoader.CurrentLevelData);
    }

    public void ActivateWindow(LevelData levelData)
    {
        LevelLoader.CurrentLevelData = levelData;
        _oldLevelName = "";

        _levelName.SetText(levelData.LevelName);
        _levelDescription.SetText(levelData.LevelDescription);
        _levelDifficulty.SetText(levelData.Level_Difficulty.ToString());

        _levelNameIF.text = levelData.LevelName;
        _levelDescriptionIF.text = levelData.LevelDescription;

        // Find the index of the target enum value
        int index = System.Array.IndexOf(_difficulties, LevelLoader.CurrentLevelData.Level_Difficulty);

        // Set the dropdown value to the found index
        _levelDifficultyDropdown.value = index;
        _levelDifficultyDropdown.RefreshShownValue();

        BuildLevelPreview();
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
        string folderPath = Path.Combine(Application.persistentDataPath, "PlayerLevelData");
        string filePath = Path.Combine(folderPath, LevelLoader.CurrentLevelData.LevelName + ".json");

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Delete the file
            File.Delete(filePath);
        }

        _offlinePlayerLevelsFetcher.FetchAndInstantiateLevels();

        DeactivateWindow(true);
    }

    void ChangeLevelName()
    {
        _oldLevelName = LevelLoader.CurrentLevelData.LevelName.ToString();

        LevelLoader.CurrentLevelData.LevelName = _levelNameIF.text;
    }

    void ChangeLevelDescription()
    {
        LevelLoader.CurrentLevelData.LevelDescription = _levelDescriptionIF.text;
    }

    void DropdownValueChanged()
    {
        // Get the selected enum value
        LevelLoader.CurrentLevelData.Level_Difficulty = (LevelData.LevelDifficulty)_levelDifficultyDropdown.value;
    }

    void BuildLevelPreview()
    {
        for (int i = 0; i < LevelLoader.CurrentLevelData.SavedElements.Count; i++)
        {
            SavedElement savedElement = LevelLoader.CurrentLevelData.SavedElements[i];
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
        _levelSaver.SaveLevel(LevelLoader.CurrentLevelData, _oldLevelName);
        _offlinePlayerLevelsFetcher.FetchAndInstantiateLevels();
    }
}
