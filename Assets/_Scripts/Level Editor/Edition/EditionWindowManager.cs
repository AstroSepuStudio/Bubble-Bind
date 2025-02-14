using UnityEngine;
using TMPro;
using System.Collections.Generic;
using static UnityEditor.Progress;

public class EditionWindowManager : MonoBehaviour
{
    [SerializeField] LevelEditorManager _levelEditorManager;

    [Header("UI References")]
    [SerializeField] private GameObject _editorWindow;
    [SerializeField] private GameObject _particularElementEditPanel;
    [SerializeField] private TextMeshProUGUI _idText;

    [Header("Transform Input Fields")]
    [SerializeField] private TMP_InputField _positionXInput;
    [SerializeField] private TMP_InputField _positionYInput;
    [SerializeField] private TMP_InputField _rotationZInput;
    [SerializeField] private TMP_InputField _scaleXInput;
    [SerializeField] private TMP_InputField _scaleYInput;

    [SerializeField] List<ElementWindowPair> _elementsWindows;

    private GameObject _activeElementWindow;

    private void Start()
    {
        // Add listeners to the input fields
        _positionXInput.onEndEdit.AddListener((value) => UpdateSelectedElementsPosition());
        _positionYInput.onEndEdit.AddListener((value) => UpdateSelectedElementsPosition());
        _rotationZInput.onEndEdit.AddListener((value) => UpdateSelectedElementsRotation());
        _scaleXInput.onEndEdit.AddListener((value) => UpdateSelectedElementsScale());
        _scaleYInput.onEndEdit.AddListener((value) => UpdateSelectedElementsScale());

        foreach (var item in _levelEditorManager._elementDataBase.EditorElementDatas)
        {
            if (item.ElementEditionWindow == null) continue;

            ElementWindowPair pair = new();
            pair.ElementData = item;

            pair.InstancedEditionWindow = Instantiate(item.ElementEditionWindow, _particularElementEditPanel.transform);

            pair.ElementEditingUtils_ = pair.InstancedEditionWindow.GetComponent<ElementEditingUtils>();
            pair.InstancedEditionWindow.SetActive(false);

            _elementsWindows.Add(pair);
        }

        foreach (var item in _elementsWindows)
        {
            item.ElementEditingUtils_.Initialize(_levelEditorManager);
        }
    }

    public void OpenEditorWindow()
    {
        // Update the transform panel with the selected elements' values
        if (_levelEditorManager._selectedElements.Count > 0)
        {
            UpdateTransformPanel();

            // Show the window
            _editorWindow.SetActive(true);

            // Update the particular element edit panel
            UpdateParticularElementEditPanel();
        }
    }

    private void UpdateTransformPanel()
    {
        if (_levelEditorManager._selectedElements.Count == 1)
            _idText.SetText($"ID: {_levelEditorManager._selectedElements[0].ElementIndex}");
        else
            _idText.SetText("ID: -");

        // Initialize variables to store values
        Vector3 position = _levelEditorManager._selectedElements[0].transform.position;
        Vector3 rotation = _levelEditorManager._selectedElements[0].transform.eulerAngles;
        Vector3 scale = _levelEditorManager._selectedElements[0].transform.localScale;

        // Check if all selected elements have the same position, rotation, and scale
        bool samePositionX = true, samePositionY = true;
        bool sameRotationZ = true;
        bool sameScaleX = true, sameScaleY = true;

        foreach (var element in _levelEditorManager._selectedElements)
        {
            // Compare position
            if (!Mathf.Approximately(element.transform.position.x, position.x)) samePositionX = false;
            if (!Mathf.Approximately(element.transform.position.y, position.y)) samePositionY = false;

            // Compare rotation
            if (!Mathf.Approximately(element.transform.eulerAngles.z, rotation.z)) sameRotationZ = false;

            // Compare scale
            if (!Mathf.Approximately(element.transform.localScale.x, scale.x)) sameScaleX = false;
            if (!Mathf.Approximately(element.transform.localScale.y, scale.y)) sameScaleY = false;
        }

        // Update position input fields
        _positionXInput.text = samePositionX ? (Mathf.Round(position.x * 100)/100).ToString() : "-";
        _positionYInput.text = samePositionY ? (Mathf.Round(position.y * 100) / 100).ToString() : "-";

        // Update rotation input fields
        _rotationZInput.text = sameRotationZ ? (Mathf.Round(rotation.z * 100) / 100).ToString() : "-";

        // Update scale input fields
        _scaleXInput.text = sameScaleX ? (Mathf.Round(scale.x * 100) / 100).ToString() : "-";
        _scaleYInput.text = sameScaleY ? (Mathf.Round(scale.y * 100) / 100).ToString() : "-";
    }

    private void UpdateSelectedElementsPosition()
    {
        if (_levelEditorManager._selectedElements.Count == 0) return;

        // Parse the input values
        float x = float.Parse(_positionXInput.text);
        float y = float.Parse(_positionYInput.text);

        // Apply the new position to all selected elements
        foreach (var element in _levelEditorManager._selectedElements)
        {
            element.transform.position = new Vector3(x, y, element.transform.position.z);
        }
    }

    private void UpdateSelectedElementsRotation()
    {
        if (_levelEditorManager._selectedElements.Count == 0) return;

        // Parse the input values
        float z = float.Parse(_rotationZInput.text);

        // Apply the new rotation to all selected elements
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (element == _levelEditorManager._cameraEditorElement) continue;
            element.transform.eulerAngles = new Vector3(0, 0, z);
        }
    }

    private void UpdateSelectedElementsScale()
    {
        if (_levelEditorManager._selectedElements.Count == 0) return;

        // Parse the input values
        float x = float.Parse(_scaleXInput.text);
        float y = float.Parse(_scaleYInput.text);

        // Apply the new scale to all selected elements
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (element == _levelEditorManager._cameraEditorElement) continue;
            element.transform.localScale = new Vector3(x, y, 1);
        }
    }

    private void UpdateParticularElementEditPanel()
    {
        // Check if all selected elements are of the same type
        bool allSameType = true;
        _particularElementEditPanel.SetActive(false);

        EditorElementData firstElementType = _levelEditorManager._selectedElements[0]._data;

        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (element._data != firstElementType)
            {
                allSameType = false;
                break;
            }
        }

        // Show the particular element edit panel only if all selected elements are of the same type
        if (allSameType)
        {
            _particularElementEditPanel.SetActive(true);

            foreach (var pair in _elementsWindows)
            {
                if (pair.ElementData == firstElementType)
                {
                    pair.InstancedEditionWindow.SetActive(true);
                    pair.ElementEditingUtils_.UpdateWindow(pair.ElementData);
                    _activeElementWindow = pair.InstancedEditionWindow;
                    break;
                }
            }
        }
    }

    public void DisableElementEditionWindow()
    {
        if (_activeElementWindow == null) return;

        _activeElementWindow.SetActive(false);
    }
}
