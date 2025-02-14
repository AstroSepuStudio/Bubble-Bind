using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveModeBehaviour : MonoBehaviour
{
    [SerializeField] LevelEditorManager _levelEditorManager;
    [SerializeField] Transform _arrowsTransform;
    [SerializeField] Transform _childTransform;

    private Camera _mainCamera;
    public bool _isDragging;
    private Vector3 _initialMousePosition;
    Vector3 _childInitialPosition;

    private LevelEditorManager.LevelEditorAxis _movementAxis;
    List<Vector3> _originalPositions = new();
    List<Vector3> _newPositions = new();

    private void OnEnable()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        // Position the arrows at the center of all selected elements
        PositionArrowsAtSelectedElementsCenter();

        _levelEditorManager.OnElementSelected += PositionArrowsAtSelectedElementsCenter;
        _levelEditorManager.OnElementDeselected += PositionArrowsAtSelectedElementsCenter;
        _levelEditorManager.OnElementDestroyed += PositionArrowsAtSelectedElementsCenter;
    }

    private void Update()
    {
        if (_isDragging)
        {
            HandleArrowDrag();
        }
    }

    public void PositionArrowsAtSelectedElementsCenter(EditorElement editorElement = null)
    {
        if (_levelEditorManager == null || _arrowsTransform == null || _levelEditorManager._selectedElements.Count == 0)
        {
            _arrowsTransform.gameObject.SetActive(false);
            return;
        }

        _arrowsTransform.gameObject.SetActive(true);
        // Calculate the center of all selected elements
        Vector3 center = Vector3.zero;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            center += element.transform.position;
        }
        center /= _levelEditorManager._selectedElements.Count;

        // Position the arrows at the center
        _arrowsTransform.position = center;
    }

    public void StartDragging(Vector3 mousePosition, LevelEditorManager.LevelEditorAxis axis)
    {
        _isDragging = true;
        _initialMousePosition = mousePosition;

        _childInitialPosition = _childTransform.position;

        _originalPositions.Clear();
        // Store the initial positions of all selected elements
        foreach (var element in _levelEditorManager._selectedElements)
        {
            _originalPositions.Add(element.transform.position);
            element.transform.SetParent(_childTransform);
        }

        _movementAxis = axis;
    }

    public void StopDragging()
    {
        _newPositions.Clear();

        // Store the initial positions of all selected elements
        foreach (var element in _levelEditorManager._selectedElements)
        {
            _newPositions.Add(element.transform.position);
            element.transform.SetParent(null);
        }
        _isDragging = false;

        _levelEditorManager.UndoRedoManager.RecordMoveAction(this,
            _levelEditorManager._selectedElements,
            _originalPositions,
            _newPositions);
    }

    private void HandleArrowDrag()
    {
        if (_levelEditorManager._selectedElements.Count == 0) return;

        // Calculate mouse movement delta
        Vector3 mouseDelta = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - _initialMousePosition;
        mouseDelta.z = 0;

        // Apply snapping if enabled
        if (_levelEditorManager._snapping)
        {
            float snapValue = _levelEditorManager._snapValue;
            mouseDelta.x = Mathf.Round(mouseDelta.x / snapValue) * snapValue;
            mouseDelta.y = Mathf.Round(mouseDelta.y / snapValue) * snapValue;
        }

        Vector3 newPosition = _childInitialPosition;

        switch (_movementAxis)
        {
            case LevelEditorManager.LevelEditorAxis.X:
                newPosition.x += mouseDelta.x;
                break;
            case LevelEditorManager.LevelEditorAxis.Y:
                newPosition.y += mouseDelta.y;
                break;
            case LevelEditorManager.LevelEditorAxis.Universal:
                newPosition += mouseDelta;
                break;
        }

        _childTransform.position = newPosition;

        // Move the arrows to the new center
        PositionArrowsAtSelectedElementsCenter();
    }
}
