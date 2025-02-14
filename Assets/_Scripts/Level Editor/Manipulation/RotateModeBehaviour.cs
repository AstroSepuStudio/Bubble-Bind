using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateModeBehaviour : MonoBehaviour
{
    [SerializeField] private LevelEditorManager _levelEditorManager;
    [SerializeField] private Transform _rotatingCenter;

    private Camera _mainCamera;
    public bool _isRotating;
    private float _initialAngle;

    Vector3 _mousePosition;
    Vector3 _initialRotation;

    List<Quaternion> _originalRotations = new();
    List<Vector3> _originalPositions = new();
    List<Quaternion> _newRotations = new();
    List<Vector3> _newPositions = new();

    private void Update()
    {
        if (_isRotating)
        {
            _mousePosition = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _mousePosition.z = 0;

            HandleRotation();
        }
    }

    private void OnEnable()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        PositionCenterAtSelectedElementsCenter();

        _levelEditorManager.OnElementSelected += PositionCenterAtSelectedElementsCenter;
        _levelEditorManager.OnElementDeselected += PositionCenterAtSelectedElementsCenter;
        _levelEditorManager.OnElementDestroyed += PositionCenterAtSelectedElementsCenter;
    }

    public void StartRotating(Vector3 mousePosition)
    {
        if (_rotatingCenter == null || _levelEditorManager._selectedElements.Count == 0) return;

        _isRotating = true;
        _initialAngle = GetAngleFromMouse(mousePosition);
        _initialRotation = _rotatingCenter.rotation.eulerAngles;

        _originalPositions.Clear();
        _originalRotations.Clear();

        // Parent all selected elements to _rotatingCenter
        foreach (var element in _levelEditorManager._selectedElements)
        {
            _originalPositions.Add(element.transform.position);
            _originalRotations.Add(element.transform.rotation);

            element.transform.SetParent(_rotatingCenter);
        }
    }

    private float GetAngleFromMouse(Vector3 mousePosition)
    {
        Vector3 direction = mousePosition - _rotatingCenter.position;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    public void StopRotating()
    {
        _newRotations.Clear();
        _newPositions.Clear();

        _isRotating = false;

        // Unparent all elements from _rotatingCenter
        foreach (var element in _levelEditorManager._selectedElements)
        {
            _newRotations.Add(element.transform.rotation);
            _newPositions.Add(element.transform.position);

            element.transform.SetParent(null);
        }

        _levelEditorManager.UndoRedoManager.RecordRotateAction(this,
            _levelEditorManager._selectedElements,
            _originalRotations,
            _originalPositions,
            _newRotations,
            _newPositions);
    }

    private void HandleRotation()
    {
        if (_levelEditorManager._selectedElements.Count == 0) return;

        float angleDelta = GetAngleFromMouse(_mousePosition) - _initialAngle;

        // Apply snapping
        if (_levelEditorManager._snapping)
            angleDelta = Mathf.Round(angleDelta / _levelEditorManager._snappingAngle) * _levelEditorManager._snappingAngle;

        // Rotate the entire _rotatingCenter (which rotates all children)
        _rotatingCenter.rotation = Quaternion.Euler(0, 0, _initialRotation.z + angleDelta);
    }

    public void PositionCenterAtSelectedElementsCenter(EditorElement editorElement = null)
    {
        if (_levelEditorManager == null || _rotatingCenter == null || _levelEditorManager._selectedElements.Count == 0)
        {
            _rotatingCenter.gameObject.SetActive(false);
            return;
        }
        _rotatingCenter.gameObject.SetActive(true);

        // Calculate the center of all selected elements
        Vector3 center = Vector3.zero;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            center += element.transform.position;
        }
        center /= _levelEditorManager._selectedElements.Count;

        center.z = -0.1f;

        // Position the arrows at the center
        _rotatingCenter.position = center;
    }
}
