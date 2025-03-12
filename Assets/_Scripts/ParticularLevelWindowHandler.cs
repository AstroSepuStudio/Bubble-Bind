using System;
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
    [SerializeField] LevelsFetcher _offlinePlayerLevelsFetcher;
    [SerializeField] LevelSaver _levelSaver;
    [SerializeField] EditorElementDataBase _editorElementDataBase;

    [Header("Information")]
    [SerializeField] GameObject _levelNameIMG;
    [SerializeField] GameObject _levelDescriptionIMG;
    [SerializeField] GameObject _levelDifficultyIMG;
    [SerializeField] TextMeshProUGUI _levelName;
    [SerializeField] TextMeshProUGUI _levelDescription;
    [SerializeField] TextMeshProUGUI _levelDifficulty;

    [Header("Editing")]
    [SerializeField] TMP_InputField _levelNameIF;
    [SerializeField] TMP_InputField _levelDescriptionIF;
    [SerializeField] TMP_Dropdown _levelDifficultyDropdown;
    [SerializeField] GameObject _editButton;
    [SerializeField] GameObject _uploadButton;

    [Header("Preview")]
    [SerializeField] GameObject elementUIPrefab; // Prefab with an Image component
    [SerializeField] Transform elementUIParent; // Parent object to hold the instantiated images
    [SerializeField] List<Image> _instancedImages;
    [SerializeField] float _scaleMultiplier;
    [SerializeField] float _positionMultiplier;

    string _oldLevelName;
    LevelData.LevelDifficulty[] _difficulties;

    bool _initialized = false;
    bool _isEditor;

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
        {
            string normalizedParent = Path.GetFullPath(Directory.GetParent(LevelLoader.CurrentLevelPath).ToString());
            string normalizedLocal = Path.GetFullPath(LevelSaver.LocalLevelFolder);
            string normalizedOnline = Path.GetFullPath(LevelSaver.OnlineLevelFolder);

            if (normalizedParent.Equals(normalizedLocal))
            {
                ActivateWindow(LevelLoader.CurrentLevelPath);
                _offlinePlayerLevelsFetcher.FetchPlayerLevels();
            }
            else if (normalizedParent.Equals(normalizedOnline))
            {
                ActivateWindow(LevelLoader.CurrentLevelPath, true);
                _offlinePlayerLevelsFetcher.FetchDownloadedLevels();
            }
        }
    }

    public void ActivateWindow(string levelPath, bool downloaded = false)
    {
        _isEditor = true;

        // if is downloaded (true) enable player things
        _levelNameIMG.SetActive(downloaded);
        _levelDescriptionIMG.SetActive(downloaded);
        _levelDifficultyIMG.SetActive(downloaded);

        // if is NOT downloaded (true)
        _levelNameIF.gameObject.SetActive(!downloaded);
        _levelDescriptionIF.gameObject.SetActive(!downloaded);
        _levelDifficultyDropdown.gameObject.SetActive(!downloaded);
        _editButton.SetActive(!downloaded);
        _uploadButton.SetActive(!downloaded);

        // Get LevelData
        string encryptedJson = File.ReadAllText(levelPath);
        string json = EncryptionUtility.Decrypt(encryptedJson);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        Debug.Log(LevelLoader.CurrentLevelPath);

        LevelLoader.CurrentLevelPath = levelPath;
        _oldLevelName = levelData.LevelName;

        _levelName.SetText(levelData.LevelName);
        _levelDescription.SetText(levelData.LevelDescription);
        _levelDifficulty.SetText(levelData.Level_Difficulty.ToString());

        _levelNameIF.text = levelData.LevelName;
        _levelDescriptionIF.text = levelData.LevelDescription;

        // Find the index of the target enum value
        int index = Array.IndexOf(_difficulties, levelData.Level_Difficulty);

        // Set the dropdown value to the found index
        _levelDifficultyDropdown.value = index;
        _levelDifficultyDropdown.RefreshShownValue();

        BuildLevelPreview(levelData);
    }

    public void ActivateWindow(LevelUploadRequestData downloadedLevel)
    {
        _isEditor = false;

        // Enable player options
        _levelNameIMG.SetActive(true);
        _levelDescriptionIMG.SetActive(true);
        _levelDifficultyIMG.SetActive(true);

        // Disable autor options
        _levelNameIF.gameObject.SetActive(false);
        _levelDescriptionIF.gameObject.SetActive(false);
        _levelDifficultyDropdown.gameObject.SetActive(false);
        _editButton.SetActive(false);
        _uploadButton.SetActive(false);

        // Get LevelData
        string json = EncryptionUtility.Decrypt(downloadedLevel.datos_cifrados);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        // Create file into downloaded levels path
        string filePath = Path.Combine(LevelSaver.OnlineLevelFolder, downloadedLevel.id + ".json");
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, downloadedLevel.datos_cifrados);

        LevelLoader.CurrentLevelPath = filePath;
        _oldLevelName = levelData.LevelName;

        _levelName.SetText(levelData.LevelName);
        _levelDescription.SetText(levelData.LevelDescription);
        _levelDifficulty.SetText(levelData.Level_Difficulty.ToString());

        BuildLevelPreview(levelData);
    }

    public void DeactivateWindow(bool levelDeleted)
    {
        _window.SetActive(false);

        foreach (var item in _instancedImages)
        {
            item.gameObject.SetActive(false);
        }

        if (!levelDeleted && _isEditor)
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

        string normalizedParent = Path.GetFullPath(Directory.GetParent(LevelLoader.CurrentLevelPath).ToString());
        string normalizedLocal = Path.GetFullPath(LevelSaver.LocalLevelFolder);
        string normalizedOnline = Path.GetFullPath(LevelSaver.OnlineLevelFolder);

        if (normalizedParent.Equals(normalizedLocal))
        {
            _offlinePlayerLevelsFetcher.FetchPlayerLevels();
        }
        else if (normalizedParent.Equals(normalizedOnline))
        {
            _offlinePlayerLevelsFetcher.FetchDownloadedLevels();
        }
        
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

        if (levelData.PlayerData == null) return;

        bool instanced = false;
        for (int i = 0; i < _instancedImages.Count; i++)
        {
            if (_instancedImages[i].gameObject.activeInHierarchy) continue;

            _instancedImages[i].sprite = levelData.PlayerData.ElementIcon;
            _instancedImages[i].rectTransform.anchoredPosition = levelData.PlayerPosition * _positionMultiplier;
            _instancedImages[i].rectTransform.localScale = _scaleMultiplier * levelData.PlayerData.SpriteScale * Vector3.one;
            instanced = true;
        }

        if (!instanced)
        {
            GameObject elementUI = Instantiate(elementUIPrefab, elementUIParent);
            Image image = elementUI.GetComponent<Image>();
            RectTransform rectTransform = elementUI.GetComponent<RectTransform>();

            _instancedImages.Add(image);
            image.sprite = levelData.PlayerData.ElementIcon;
            rectTransform.anchoredPosition = levelData.PlayerPosition * _positionMultiplier;
            rectTransform.localScale = _scaleMultiplier * levelData.PlayerData.SpriteScale * Vector3.one;
        }

        instanced = false;
        for (int i = 0; i < _instancedImages.Count; i++)
        {
            if (_instancedImages[i].gameObject.activeInHierarchy) continue;

            _instancedImages[i].sprite = levelData.GoalData.ElementIcon;
            _instancedImages[i].rectTransform.anchoredPosition = levelData.GoalPosition * _positionMultiplier;
            _instancedImages[i].rectTransform.localScale = _scaleMultiplier * levelData.GoalData.SpriteScale * Vector3.one;
            instanced = true;
        }

        if (!instanced)
        {
            GameObject elementUI = Instantiate(elementUIPrefab, elementUIParent);
            Image image = elementUI.GetComponent<Image>();
            RectTransform rectTransform = elementUI.GetComponent<RectTransform>();

            _instancedImages.Add(image);
            image.sprite = levelData.GoalData.ElementIcon;
            rectTransform.anchoredPosition = levelData.GoalPosition * _positionMultiplier;
            rectTransform.localScale = _scaleMultiplier * levelData.GoalData.SpriteScale * Vector3.one;
        }
    }

    public void SaveChanges()
    {
        string normalizedParent = Path.GetFullPath(Directory.GetParent(LevelLoader.CurrentLevelPath).ToString());
        string normalizedLocal = Path.GetFullPath(LevelSaver.LocalLevelFolder);

        if (normalizedParent.Equals(normalizedLocal))
        {
            // Get LevelData
            string encryptedJson = File.ReadAllText(LevelLoader.CurrentLevelPath);
            string json = EncryptionUtility.Decrypt(encryptedJson);
            LevelData levelData = JsonUtility.FromJson<LevelData>(json);

            levelData.LevelName = _levelNameIF.text;
            levelData.LevelDescription = _levelDescriptionIF.text;
            levelData.Level_Difficulty = (LevelDifficulty)_levelDifficultyDropdown.value;

            json = JsonUtility.ToJson(levelData, true);
            encryptedJson = EncryptionUtility.Encrypt(json);
            File.WriteAllText(LevelLoader.CurrentLevelPath, encryptedJson);

            _offlinePlayerLevelsFetcher.FetchPlayerLevels();
        }     
    }
}
