using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEditorManager : MonoBehaviour
{
    public enum LevelEditorAxis { X, Y, Universal }

    [Header("Level Managment")]
    [SerializeField] LevelLoader _levelLoader;
    [SerializeField] LevelSaver _levelSaver;

    [Header("Debug")]
    [SerializeField] bool _forceLoadLevel;
    [SerializeField] string _levelName;
    public bool _isPlayTesting;

    [Header("References")]
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] ButtonUtilities _buttonUtilities;
    public UndoRedoManager UndoRedoManager;
    public Camera _elementCamera;
    public EditorElement _cameraEditorElement;
    [SerializeField] LineRenderer _elementCameraFrustumRenderer;

    [SerializeField] Transform _elementsContent;
    [SerializeField] GameObject _elementButtonPrefab;
    [SerializeField] MoveModeBehaviour _moveMode;
    [SerializeField] RotateModeBehaviour _rotateMode;
    [SerializeField] ScaleModeBehaviour _scaleMode;

    public EditorElementDataBase _elementDataBase;

    [Header("Camera Movement")]
    [SerializeField] Camera _editorCamera;
    [SerializeField] private bool _isDraggingCamera;
    [SerializeField] private bool _hasStartedDragging; // Tracks if the player has started dragging
    [SerializeField] private Vector3 _initialMousePosition;
    [SerializeField] private float _dragThreshold = 0.1f; // Minimum distance to consider it a drag

    [Header("Events")]
    public Action<EditorElement> OnElementSelected;
    public Action<EditorElement> OnElementDeselected;
    public Action<EditorElement> OnElementDestroyed;

    // Existing variables
    [Header("Snapping")]
    [SerializeField] Image _snapButtonImage;
    [SerializeField] TMP_InputField _snapValueIF;
    [SerializeField] TMP_InputField _snapAngleIF;
    public bool _snapping;
    public float _snapValue = 1f;
    public float _snappingAngle = 15f;

    [Header("Box Selection")]
    [SerializeField] private LineRenderer _boxSelectionRenderer;
    private bool _isBoxSelecting;
    private Vector3 _boxSelectionStart;
    private Vector3 _boxSelectionEnd;

    List<ElementButton> _elementButtons = new();
    public List<EditorElement> _instancedElements = new();
    public List<EditorElement> _selectedElements = new();

    EditorElement _elementHit;
    EditorElementData _selectedData;
    bool _swipe;
    bool _modifierConsumed;

    private void Awake()
    {
        if (_isPlayTesting) return;

        if (_forceLoadLevel)
            LevelLoader.LevelName = _levelName;

        _snapValueIF.onEndEdit.AddListener((value) => ChangeSnapValue());
        _snapAngleIF.onEndEdit.AddListener((value) => ChangeSnapAngle());
    }

    private void Start()
    {
        if (_isPlayTesting) return;

        UndoRedoManager = new();

        foreach (var elementData in _elementDataBase.EditorElementDatas)
        {
            GameObject tmp = Instantiate(_elementButtonPrefab, _elementsContent);
            ElementButton button = tmp.GetComponent<ElementButton>();
            button.SetUp(this, elementData);
            _elementButtons.Add(button);
        }

        _playerInput.actions["Select"].started += OnClick;
        _playerInput.actions["Select"].canceled += OnClickReleased;
        _playerInput.actions["Duplicate"].started += DuplicateSelectedElements;
        _playerInput.actions["Scroll"].started += ChangeEditorCameraSize;
        _playerInput.actions["Snap"].canceled += SwitchSnappingState;
        _playerInput.actions["Undo"].started += Undo;
        _playerInput.actions["Redo"].started += Redo;
    }

    public void RemoveEventSubscriptions()
    {
        _playerInput.actions["Select"].started -= OnClick;
        _playerInput.actions["Select"].canceled -= OnClickReleased;
        _playerInput.actions["Duplicate"].started -= DuplicateSelectedElements;
        _playerInput.actions["Scroll"].started -= ChangeEditorCameraSize;
        _playerInput.actions["Snap"].canceled -= SwitchSnappingState;
        _playerInput.actions["Undo"].started -= Undo;
        _playerInput.actions["Redo"].started -= Redo;
    }

    void OnClick(InputAction.CallbackContext context)
    {
        // Evitar la selección si el mouse está sobre UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);

        // Check if the hit object is a MoveArrow, RotationCenter, or ScalingHandles
        if (hit && (
            hit.transform.CompareTag("MoveArrow") || 
            hit.transform.CompareTag("RotatingCenter") || 
            hit.transform.CompareTag("ScalingHandles")))
        {
            // Handle specific interactions (e.g., move, rotate, scale)
            if (hit.transform.CompareTag("MoveArrow"))
                StartElementMovement(hit);
            else if (hit.transform.CompareTag("RotatingCenter"))
                StartElementRotation(hit);
            else if (hit.transform.CompareTag("ScalingHandles"))
                StartElementScaling(hit);

            return; // Exit early to prevent camera movement
        }

        // Start tracking the initial mouse position for potential dragging
        _initialMousePosition = _editorCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _hasStartedDragging = false; // Reset dragging state
    }

    void OnClickReleased(InputAction.CallbackContext context)
    {
        // Evitar la selección si el mouse está sobre UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Reset dragging states
            _isDraggingCamera = false;
            _hasStartedDragging = false;
            _isBoxSelecting = false;
            _boxSelectionRenderer.enabled = false;
            return;
        }

        // If the player didn't start dragging, handle element selection
        if (!_hasStartedDragging &&
            !_moveMode._isDragging &&
            !_rotateMode._isRotating &&
            !_scaleMode._isScaling &&
            !_isBoxSelecting)
        {
            Vector3 mousPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousPos, Vector2.zero);

            if (hit && hit.transform.CompareTag("EditorElement"))
            {
                SelectElement(hit);
            }
            else
            {
                InstantiateElement(_selectedData, false, mousPos);
            }
        }

        if (_isBoxSelecting)
        {
            _boxSelectionEnd = _editorCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            DetectElementsInBox(_boxSelectionStart, _boxSelectionEnd);
            _isBoxSelecting = false;
            _boxSelectionRenderer.enabled = false;
        }

        // Reset dragging states
        _isDraggingCamera = false;
        _hasStartedDragging = false;

        if (_moveMode._isDragging)
            _moveMode.StopDragging();
        if (_rotateMode._isRotating)
            _rotateMode.StopRotating();
        if (_scaleMode._isScaling)
            _scaleMode.StopScaling();
    }

    private void Update()
    {
        if (_isPlayTesting) return;

        if (_moveMode._isDragging ||
            _rotateMode._isRotating ||
            _scaleMode._isScaling)
            return;

        if (_isDraggingCamera)
        {
            MoveCamera();
        }
        else if (Mouse.current.leftButton.isPressed && EventSystem.current.IsPointerOverGameObject())
        {
            _initialMousePosition = _editorCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        else if (_isBoxSelecting)
        {
            Vector3 currentMousePosition = _editorCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _boxSelectionEnd = currentMousePosition;
            DrawBox(_boxSelectionStart, _boxSelectionEnd);
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            // Check if the player has started dragging
            Vector3 currentMousePosition = _editorCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            float distance = Vector3.Distance(_initialMousePosition, currentMousePosition);

            if (distance > _dragThreshold)
            {
                if (_swipe)
                {
                    // Start box selection
                    _isBoxSelecting = true;
                    _boxSelectionStart = _initialMousePosition;
                    DrawBox(_boxSelectionStart, _boxSelectionStart);
                    _boxSelectionRenderer.enabled = true;
                    _boxSelectionRenderer.positionCount = 4;
                }
                else
                {
                    _hasStartedDragging = true;
                    _isDraggingCamera = true;
                }
            }
        }
    }

    public EditorElement InstantiateElement(EditorElementData data, bool ignoreSetUp, Vector3 position)
    {
        if (data == null)
            return null;

        // Find the next available index
        int newIndex = GetNextAvailableIndex();
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);
        position.z = 0;

        GameObject temp = Instantiate(data.ElementPrefab, position, Quaternion.identity);
        EditorElement element = temp.GetComponent<EditorElement>();
        element._data = data;
        element.ElementIndex = newIndex;

        if (!ignoreSetUp)
            element.SetUpElement(this);
        _instancedElements.Add(element);
        
        return element;
    }

    public void DestroySelectedElements()
    {
        // Check if there are any selected elements
        if (_selectedElements.Count == 0)
            return;

        // Create a temporary list to avoid modifying the list while iterating
        List<EditorElement> elementsToDestroy = new (_selectedElements);

        // Destroy each selected element
        foreach (var element in elementsToDestroy)
        {
            if (element.transform == _cameraEditorElement.transform)
                continue;

            // Remove the element from the instanced elements list
            _instancedElements.Remove(element);

            // Remove the element from the selected elements list
            _selectedElements.Remove(element);

            // Invoke the OnElementDeselected event
            OnElementDestroyed?.Invoke(element);

            // Destroy the GameObject
            Destroy(element.gameObject);
        }
    }

    void DuplicateSelectedElements(InputAction.CallbackContext context)
    {
        _modifierConsumed = true;
        DuplicateSelectedElements();
    }

    public void DuplicateSelectedElements()
    {
        // Check if there are any selected elements
        if (_selectedElements.Count == 0)
        {
            Debug.LogWarning("No elements are selected to duplicate.");
            return;
        }

        // Create a list to store the duplicated elements
        List<EditorElement> duplicatedElements = new List<EditorElement>();

        // Duplicate each selected element
        foreach (var element in _selectedElements)
        {
            if (element.transform == _cameraEditorElement.transform)
                continue;

            // Find the next available index
            int newIndex = GetNextAvailableIndex();

            // Instantiate a duplicate of the element
            GameObject duplicate = Instantiate(element.gameObject, element.transform.position, element.transform.rotation);

            // Optionally, offset the duplicate to avoid overlapping with the original
            duplicate.transform.position += new Vector3(_snapValue, _snapValue, 0f); // Adjust the offset as needed

            // Get the EditorElement component from the duplicate
            if (duplicate.TryGetComponent<EditorElement>(out var duplicateElement))
            {
                // Give the duplicate element a new index
                duplicateElement.ElementIndex = newIndex;

                // Add the duplicate to the instanced elements list
                _instancedElements.Add(duplicateElement);

                // Add the duplicate to the duplicated elements list
                duplicatedElements.Add(duplicateElement);
            }
        }

        // Deselect all currently selected elements
        DeselectElements();

        // Select the duplicated elements
        foreach (var duplicate in duplicatedElements)
        {
            duplicate.SelectElement();
            _selectedElements.Add(duplicate);
            OnElementSelected?.Invoke(duplicate);
        }
    }

    public void StartLevel()
    {
        _levelSaver.SaveLevel();
        RemoveEventSubscriptions();
        SceneManager.LoadScene("LevelTesting");
    }

    public void InitializeAllElements()
    {
        foreach (var element in _instancedElements)
        {
            element.InitializeElement();
        }
    }

    public void Undo()
    {
        UndoRedoManager.Undo();
    }

    public void Undo(InputAction.CallbackContext context)
    {
        _modifierConsumed = true;
        UndoRedoManager.Undo();
    }

    public void Redo()
    {
        UndoRedoManager.Redo();
    }

    public void Redo(InputAction.CallbackContext context)
    {
        _modifierConsumed = true;
        UndoRedoManager.Redo();
    }

    #region Player Preferences
    void ChangeSnapValue()
    {
        _snapValue = float.Parse(_snapValueIF.text);
    }

    void ChangeSnapAngle()
    {
        _snappingAngle = float.Parse(_snapAngleIF.text);
    }

    public void UpdateSnapWindow()
    {
        _snapAngleIF.text = _snappingAngle.ToString();
        _snapValueIF.text = _snapValue.ToString();
    }
    #endregion

    #region Utility
    public LevelEditorAxis GetAxisFromString(string axis)
    {
        switch (axis)
        {
            case "X":
                return LevelEditorAxis.X;
            case "Y":
                return LevelEditorAxis.Y;
            case "U":
                return LevelEditorAxis.Universal;
            default:
                Debug.LogWarning("Unknown movement axis: " + axis);
                return LevelEditorAxis.Universal;
        }
    }
    
    private int GetNextAvailableIndex()
    {
        // Find the smallest available index
        for (int i = 0; i < _instancedElements.Count + 1; i++)
        {
            bool indexInUse = false;
            foreach (var element in _instancedElements)
            {
                if (element.ElementIndex == i)
                {
                    indexInUse = true;
                    break;
                }
            }

            if (!indexInUse)
            {
                return i; // Return the first available index
            }
        }

        return _instancedElements.Count; // If no gaps, return the next index
    }
    
    public EditorElement GetElementByIndex(int index)
    {
        // Iterate through all instanced elements
        foreach (var element in _instancedElements)
        {
            if (element.ElementIndex == index)
            {
                return element; // Return the element with the matching index
            }
        }

        // If no element is found with the specified index, return null
        Debug.LogWarning($"No element found with index: {index}");
        return null;
    }
    #endregion

    #region Camera

    private void MoveCamera()
    {
        // Get the current mouse position in world coordinates
        Vector3 currentMousePosition = _editorCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Calculate the difference between the initial and current mouse positions
        Vector3 delta = _initialMousePosition - currentMousePosition;

        // Move the camera in the opposite direction of the mouse movement
        _editorCamera.transform.position += delta;
    }

    public void ChangeEditorCameraSize(float amount)
    {
        _editorCamera.orthographicSize += amount;

        if (_editorCamera.orthographicSize < 1f)
            _editorCamera.orthographicSize = 1f;
        if (_editorCamera.orthographicSize > 100f)
            _editorCamera.orthographicSize = 100f;
    }

    void ChangeEditorCameraSize(InputAction.CallbackContext context)
    {
        _editorCamera.orthographicSize -= _playerInput.actions["Scroll"].ReadValue<float>();

        if (_editorCamera.orthographicSize < 1f)
            _editorCamera.orthographicSize = 1f;
        if (_editorCamera.orthographicSize > 100f)
            _editorCamera.orthographicSize = 100f;
    }

    public void ChangeElementCameraSize(float size)
    {
        _elementCamera.orthographicSize = size;

        if (_elementCameraFrustumRenderer != null)
        {
            Vector3 position = new Vector3(-1.775f * size, -1 * size, 0);
            _elementCameraFrustumRenderer.SetPosition(0, position);

            position.x = 1.775f * size;
            _elementCameraFrustumRenderer.SetPosition(1, position);

            position.y = 1 * size;
            _elementCameraFrustumRenderer.SetPosition(2, position);

            position.x = -1.775f * size;
            _elementCameraFrustumRenderer.SetPosition(3, position);
        }        

        if (_elementCamera.orthographicSize < 1f)
            _elementCamera.orthographicSize = 1f;
        if (_elementCamera.orthographicSize > 100f)
            _elementCamera.orthographicSize = 100f;
    }

    #endregion

    #region Selection Logic
    public void SelectElementButton(EditorElementData data)
    {
        foreach (ElementButton btn in _elementButtons)
        {
            if (btn.GetEditorElementData() == _selectedData)
            {
                btn.Deselect();
                break;
            }
        }

        if (_selectedData == data)
        {
            _selectedData = null;
            return;
        }

        _selectedData = data;
    }
    
    public void SwitchSwipeMode()
    {
        _swipe = !_swipe;
    }

    public void DeselectElements(bool ignoreActionRegistration = false)
    {
        List<EditorElement> elementsToDeselect = new (_selectedElements);

        foreach (var element in elementsToDeselect)
        {
            element.DeselectElement();
            _selectedElements.Remove(element);
            OnElementDeselected?.Invoke(element);
        }

        if (ignoreActionRegistration) return;
        UndoRedoManager.RecordBulkDeselectAction(this, elementsToDeselect);
    }

    public void SelectElement(RaycastHit2D hit)
    {
        if (hit.transform == _cameraEditorElement.transform)
            _elementHit = _cameraEditorElement;
        else
        {
            // Get the element hit from the instanced elements
            for (int i = 0; i < _instancedElements.Count; i++)
            {
                if (hit.transform == _instancedElements[i].transform)
                    _elementHit = _instancedElements[i];
            }
        }
        
        if (_elementHit == null)
        {
            Debug.LogWarning("Element Hit does NOT have an Editor Element script attached");
            return;
        }

        if (_swipe)
        {
            if (_selectedElements.Contains(_elementHit))
            {
                _elementHit.DeselectElement();
                _selectedElements.Remove(_elementHit);
                OnElementDeselected?.Invoke(_elementHit);

                UndoRedoManager.RecordDeselectAction(this, _elementHit);
                return;
            }

            _elementHit.SelectElement();
            _selectedElements.Add(_elementHit);
            OnElementSelected?.Invoke(_elementHit);

            UndoRedoManager.RecordSelectAction(this, _elementHit);
        }
        else
        {
            // Non-swipe mode logic
            if (_selectedElements.Count > 1 || !_selectedElements.Contains(_elementHit))
            {
                UndoRedoManager.RecordExclusiveSelectAction(this, _elementHit, _selectedElements);

                // deselect all elements and select the hit element
                DeselectElements(true);

                _elementHit.SelectElement();
                _selectedElements.Add(_elementHit);
                OnElementSelected?.Invoke(_elementHit);
            }
            else
            {
                // If the hit element is the only selected element, deselect it
                _elementHit.DeselectElement();
                _selectedElements.Remove(_elementHit);
                OnElementDeselected?.Invoke(_elementHit);

                UndoRedoManager.RecordDeselectAction(this, _elementHit);
            }
        }
    }

    public void SelectElement(EditorElement element)
    {
        element.SelectElement();
        _selectedElements.Add(element);
        OnElementSelected?.Invoke(element);
    }

    public void DeselectElement(EditorElement element)
    {
        element.DeselectElement();
        _selectedElements.Remove(element);
        OnElementDeselected?.Invoke(element);
    }

    private void DrawBox(Vector3 start, Vector3 end)
    {
        _boxSelectionRenderer.SetPosition(0, new Vector3(start.x, start.y, 0));
        _boxSelectionRenderer.SetPosition(1, new Vector3(end.x, start.y, 0));
        _boxSelectionRenderer.SetPosition(2, new Vector3(end.x, end.y, 0));
        _boxSelectionRenderer.SetPosition(3, new Vector3(start.x, end.y, 0));
    }

    void DetectElementsInBox(Vector3 start, Vector3 end)
    {
        Bounds bounds = new Bounds();
        bounds.SetMinMax(
            new Vector3(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y), 0),
            new Vector3(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y), 0)
        );

        List<EditorElement> selectedElements = new();
        foreach (var element in _instancedElements)
        {
            if (bounds.Contains(element.transform.position))
            {
                element.SelectElement();
                _selectedElements.Add(element);
                OnElementSelected?.Invoke(element);

                selectedElements.Add(element);
            }
        }

        UndoRedoManager.RecordBulkSelectAction(this, selectedElements);
    }

    #endregion

    #region Element Manipulation Logic

    public void SwitchSnappingState()
    {
        if (_modifierConsumed)
        {
            _modifierConsumed = false;
            return;
        }

        _snapping = !_snapping;
        _buttonUtilities.SwitchButtonState(_snapButtonImage);
    }

    void SwitchSnappingState(InputAction.CallbackContext context)
    {
        if (_modifierConsumed)
        {
            _modifierConsumed = false;
            return;
        }

        _snapping = !_snapping;
        _buttonUtilities.SwitchButtonState(_snapButtonImage);
    }
    
    public void EnableManipulationMode(GameObject mode)
    {
        mode.SetActive(true);
    }

    public void DisableManipulationMode(GameObject mode)
    {
        mode.SetActive(false);
    }

    public void StartElementMovement(RaycastHit2D hit)
    {
        _moveMode.StartDragging(hit.point, GetAxisFromString(hit.transform.gameObject.name.Split(" ")[0]));
    }

    public void StartElementRotation(RaycastHit2D hit)
    {
        _rotateMode.StartRotating(hit.point);
    }

    public void StartElementScaling(RaycastHit2D hit)
    {
        _scaleMode.StartScaling(hit.point, GetAxisFromString(hit.transform.gameObject.name.Split(" ")[0]));
    }

    #endregion
}
