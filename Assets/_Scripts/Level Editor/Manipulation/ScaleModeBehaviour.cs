using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScaleModeBehaviour : MonoBehaviour
{
    [SerializeField] private LevelEditorManager _levelEditorManager;
    [SerializeField] private Transform _scalingChild;

    private Camera _mainCamera;
    public bool _isScaling;
    private Vector3 _initialMousePosition;
    private Vector3 _initialScale;
    Vector3 _mousePosition;

    private LevelEditorManager.LevelEditorAxis _movementAxis;

    List<Vector3> _originalScales = new();
    List<Vector3> _originalPositions = new();
    List<Vector3> _newScales = new();
    List<Vector3> _newPositions = new();

    private void OnEnable()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        PositionCenterAtSelectedElementsCenter();

        _levelEditorManager.OnElementSelected += PositionCenterAtSelectedElementsCenter;
        _levelEditorManager.OnElementDeselected += PositionCenterAtSelectedElementsCenter;
        _levelEditorManager.OnElementDestroyed += PositionCenterAtSelectedElementsCenter;
    }

    private void Update()
    {
        if (_isScaling)
        {
            _mousePosition = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _mousePosition.z = 0;

            HandleScaling(_mousePosition);
        }
    }

    public void StartScaling(Vector3 mousePosition, LevelEditorManager.LevelEditorAxis axis)
    {
        if (_levelEditorManager._selectedElements.Count == 0) return;

        _isScaling = true;
        _initialMousePosition = mousePosition;
        _initialScale = _scalingChild.localScale;
        _movementAxis = axis;

        _originalScales.Clear();
        _originalPositions.Clear();

        // Parent all selected elements to _scalingChild
        foreach (var element in _levelEditorManager._selectedElements)
        {
            _originalScales.Add(element.transform.localScale);
            _originalPositions.Add(element.transform.position);
            element.transform.SetParent(_scalingChild);
        }
    }

    public void StopScaling()
    {
        _isScaling = false;

        _newScales.Clear();
        _newPositions.Clear();

        // Unparent all elements from _scalingChild
        foreach (var element in _levelEditorManager._selectedElements)
        {
            _newScales.Add(element.transform.localScale);
            _newPositions.Add(element.transform.position);
            element.transform.SetParent(null);
        }

        _levelEditorManager.UndoRedoManager.RecordScaleAction(this,
            _levelEditorManager._selectedElements,
            _originalScales,
            _originalPositions,
            _newScales,
            _newPositions);

        // Reset _scalingChild scale
        _scalingChild.localScale = Vector3.one;
    }

    private void HandleScaling(Vector3 currentMousePosition)
    {
        if (_levelEditorManager._selectedElements.Count == 0) return;

        // Compute scaling factor based on mouse movement
        float scaleFactor = (currentMousePosition - _scalingChild.position).magnitude;
        scaleFactor = Mathf.Max(0.1f, scaleFactor); // Prevent negative scaling

        // Apply snapping
        if (_levelEditorManager._snapping)
        {
            scaleFactor = Mathf.Round(scaleFactor / _levelEditorManager._snapValue) * _levelEditorManager._snapValue;
        }

        Vector3 newScale = _initialScale;
        switch (_movementAxis)
        {
            case LevelEditorManager.LevelEditorAxis.X:
                newScale.x *= scaleFactor;
                break;
            case LevelEditorManager.LevelEditorAxis.Y:
                newScale.y *= scaleFactor;
                break;
            case LevelEditorManager.LevelEditorAxis.Universal:
                newScale *= scaleFactor;
                break;
        }

        if (Mathf.Approximately(newScale.x, 0))
            newScale.x = 0.1f;
        if (Mathf.Approximately(newScale.y, 0))
            newScale.y = 0.1f;
        if (Mathf.Approximately(newScale.z, 0))
            newScale.z = 0.1f;

        // Apply new scale
        _scalingChild.localScale = newScale;
    }

    public void PositionCenterAtSelectedElementsCenter(EditorElement editorElement = null)
    {
        if (_levelEditorManager._selectedElements.Count == 0) return;

        // Calculate the center of all selected elements
        Vector3 center = Vector3.zero;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            center += element.transform.position;
        }
        center /= _levelEditorManager._selectedElements.Count;

        center.z = -0.1f;

        // Position the arrows at the center
        transform.position = center;
    }
}
