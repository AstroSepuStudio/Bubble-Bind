using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System;

public class ElementEditingUtils : MonoBehaviour
{
    LevelEditorManager _levelEditorManager;
    [SerializeField] TMP_InputField _buttonInputField;
    [SerializeField] TMP_InputField _cameraInputField;
    [SerializeField] TMP_InputField _cannonShootDelayIF;
    [SerializeField] TMP_InputField _cannonShootDirectionIF;
    [SerializeField] Toggle _cannonToggle;
    [SerializeField] Toggle _physicsToggle;

    [SerializeField] GameObject _vector2IF;
    [SerializeField] Transform _dynamicPlatPosContent;
    [SerializeField] Button _dynamicPlatPosAdd;
    [SerializeField] Button _dynamicPlatPosRemove;
    [SerializeField] Toggle _dynamicPlatLoopToggle;
    [SerializeField] Toggle _dynamicPlatAutoToggle;
    [SerializeField] TMP_InputField _dynamicPlatDelayIF;
    [SerializeField] TMP_InputField _dynamicPlatSpeedIF;
    List<Vector2InputField> _vectors2IF;

    public void Initialize(LevelEditorManager levelEditorManager)
    {
        _levelEditorManager = levelEditorManager;

        if (_buttonInputField != null)
            _buttonInputField.onEndEdit.AddListener((value) => SubscribeElementToButton());

        if (_cameraInputField != null)
        {
            _cameraInputField.onEndEdit.AddListener((value) => ChangeCameraSize());
            _cameraInputField.text = _levelEditorManager._elementCamera.orthographicSize.ToString();
        }

        if (_cannonToggle != null)
        {
            _cannonShootDelayIF.onEndEdit.AddListener((value) => SetShootingDelay());
            _cannonShootDirectionIF.onEndEdit.AddListener((value) => SetCannonShootRotation());
            _cannonToggle.onValueChanged.AddListener((value) => SetCannonMode());
        }

        if (_physicsToggle != null)
        {
            _physicsToggle.onValueChanged.AddListener((value) => SetPhysics());
        }

        if (_dynamicPlatLoopToggle != null)
        {
            _vectors2IF = new();
            for (int i = 0; i < 2; i++)
            {
                GameObject tmp = Instantiate(_vector2IF, _dynamicPlatPosContent);
                Vector2InputField tmp2 = tmp.GetComponent<Vector2InputField>();

                tmp2.X_InputField.onEndEdit.AddListener((value) => 
                    SetPosition(_vectors2IF.Count, LevelEditorManager.LevelEditorAxis.X));

                tmp2.Y_InputField.onEndEdit.AddListener((value) => 
                    SetPosition(_vectors2IF.Count, LevelEditorManager.LevelEditorAxis.Y));

                _vectors2IF.Add(tmp2);
            }

            _dynamicPlatLoopToggle.onValueChanged.AddListener((value) => SetLoop());
            _dynamicPlatAutoToggle.onValueChanged.AddListener((value) => SetAuto());
            _dynamicPlatPosAdd.onClick.AddListener(AddPosition);
            _dynamicPlatPosRemove.onClick.AddListener(RemovePosition);
            _dynamicPlatDelayIF.onEndEdit.AddListener((value) => SetDelay());
            _dynamicPlatSpeedIF.onEndEdit.AddListener((value) => SetSpeed());
        }
    }

    public void UpdateWindow(EditorElementData elementData)
    {
        // Perform update based on the _data type
        switch (elementData.ElementType)
        {
            case BubbleBindElement.Element.Button:
                UpdateButtonWindow();
                break;
            case BubbleBindElement.Element.Cannon:
                UpdateCannonWindow();
                break;
            case BubbleBindElement.Element.Ground:
                UpdatePhysicsWindow();
                break;
            case BubbleBindElement.Element.Metal:
                UpdatePhysicsWindow();
                break;
            case BubbleBindElement.Element.SawBlade:
                UpdatePhysicsWindow();
                break;
            case BubbleBindElement.Element.MovingPlatform:
                UpdateDynamicPlatformWindow();
                break;
            default:
                break;
        }
    }

    #region Bubble
    public void LinkSelectedBubbles()
    {
        foreach (var bubble in _levelEditorManager._selectedElements)
        {
            foreach (var bubble2 in _levelEditorManager._selectedElements)
            {
                if (bubble.ElementIntegerValues.Contains(bubble2.ElementIndex))
                    continue;

                bubble.ElementIntegerValues.Add(bubble2.ElementIndex);
            }
        }
    }

    public void UnlinkSelectedBubbles()
    {
        foreach (var item in _levelEditorManager._selectedElements)
        {
            item.ElementIntegerValues.Clear();
        }
    }
    #endregion

    #region Button
    void SubscribeElementToButton()
    {
        foreach (var button in _levelEditorManager._selectedElements)
        {
            button.ElementIntegerValues.Clear(); // Clear the saved indexes

            // Parse the input field for indexes
            string[] indexStrings = _buttonInputField.text.Split(',');
            foreach (var indexString in indexStrings)
            {
                if (int.TryParse(indexString.Trim(), out int index))
                {
                    var element = _levelEditorManager.GetElementByIndex(index);
                    if (element != null)
                    {
                        // Check if the element implements IListenToButton
                        var listener = element._child.GetComponent<IListenToButton>();
                        if (listener != null)
                        {
                            // Save the index of the subscribed element
                            button.ElementIntegerValues.Add(index);
                        }
                        else
                        {
                            Debug.LogWarning($"Element with index {index} does not implement IListenToButton.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No element found with index {index}.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid index format: {indexString}");
                }
            }
        }        
    }
    
    void UpdateButtonWindow()
    {
        // Get the ElementIntegerValues of the first selected element
        var firstElementValues = _levelEditorManager._selectedElements[0].ElementIntegerValues;

        // Check if all selected elements have the same ElementIntegerValues
        bool allValuesMatch = true;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (!element.ElementIntegerValues.SequenceEqual(firstElementValues))
            {
                allValuesMatch = false;
                break;
            }
        }

        // Update the input field text
        if (allValuesMatch && firstElementValues.Count > 0)
        {
            // Convert the indexes to a comma-separated string
            _buttonInputField.text = string.Join(",", firstElementValues);
        }
        else if (firstElementValues.Count > 0)
        {
            // If values don't match or there are no values, show "-"
            _buttonInputField.text = "-";
        }
    }
    #endregion

    #region Camera
    void ChangeCameraSize()
    {
        _levelEditorManager.ChangeElementCameraSize(float.Parse(_cameraInputField.text));
    }
    #endregion

    #region Cannon
    void SetShootingDelay()
    {
        foreach (var element in _levelEditorManager._selectedElements)
        {
            element.ElementFloatValues[0] = float.Parse(_cannonShootDelayIF.text);
        }
    }

    void SetCannonMode()
    {
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (_cannonToggle.isOn)
                element.ElementIntegerValues[0] = 1;
            else
                element.ElementIntegerValues[0] = 0;
        }
    }

    void SetCannonShootRotation()
    {
        foreach (var element in _levelEditorManager._selectedElements)
        {
            float rotation = float.Parse(_cannonShootDirectionIF.text);
            element._pivot.rotation = Quaternion.Euler(0, 0, (-rotation) + 90);

            element.ElementFloatValues[1] = rotation;
        }
    }

    void UpdateCannonWindow()
    {
        // Get the ElementIntegerValues of the first selected element
        var firstElementIntValues = _levelEditorManager._selectedElements[0].ElementIntegerValues;

        // Check if all selected elements have the same ElementIntegerValues
        bool allValuesMatch = true;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (!element.ElementIntegerValues.SequenceEqual(firstElementIntValues))
            {
                allValuesMatch = false;
                break;
            }
        }

        if (allValuesMatch)
        {
            if (firstElementIntValues[0] == 0)
                _cannonToggle.isOn = false;
            else
                _cannonToggle.isOn = true;
        }
        else
            _cannonToggle.isOn = false;

        // Get the ElementFloatValues of the first selected element
        var firstElementFloatValues = _levelEditorManager._selectedElements[0].ElementFloatValues;

        // Check if all selected elements have the same ElementFloatValues
        allValuesMatch = true;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (!element.ElementFloatValues.SequenceEqual(firstElementFloatValues))
            {
                allValuesMatch = false;
                break;
            }
        }

        if (allValuesMatch)
        {
            _cannonShootDelayIF.text = firstElementFloatValues[0].ToString();
            _cannonShootDirectionIF.text = firstElementFloatValues[1].ToString();
        }
        else
        {
            _cannonShootDelayIF.text = "-";
            _cannonShootDirectionIF.text = "-";
        }
    }
    #endregion

    #region Physics Element
    void SetPhysics()
    {
        foreach (var element in _levelEditorManager._selectedElements)
        {
            element.Physics = _physicsToggle.isOn;
        }
    }

    void UpdatePhysicsWindow()
    {
        bool allActivePhysics = true;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (!element.Physics)
            {
                allActivePhysics = false;
                break;
            }
        }

        _physicsToggle.isOn = allActivePhysics;
    }
    #endregion

    #region Dynamic Platform
    void UpdateDynamicPlatformWindow()
    {
        _dynamicPlatPosAdd.gameObject.SetActive(true);
        _dynamicPlatPosRemove.gameObject.SetActive(true);

        // Get the first selected element as a reference
        var firstElement = _levelEditorManager._selectedElements[0];

        // Compare ElementVectors
        bool match = true;
        for (int i = 1; i < _levelEditorManager._selectedElements.Count; i++)
        {
            var currentElement = _levelEditorManager._selectedElements[i];
            if (!VectorsMatch(firstElement.ElementVectors, currentElement.ElementVectors))
            {
                match = false;
                break;
            }
        }

        // Update Vector2 input fields
        if (match && firstElement.ElementVectors.Count > 0)
        {
            // Clear existing Vector2 input fields
            foreach (var vector2IF in _vectors2IF)
            {
                Destroy(vector2IF.gameObject);
            }
            _vectors2IF.Clear();

            // Create new Vector2 input fields based on the first element's ElementVectors
            for (int i = 0; i < firstElement.ElementVectors.Count; i++)
            {
                GameObject tmp = Instantiate(_vector2IF, _dynamicPlatPosContent);
                Vector2InputField tmp2 = tmp.GetComponent<Vector2InputField>();

                tmp2.X_InputField.text = firstElement.ElementVectors[i].x.ToString();
                tmp2.Y_InputField.text = firstElement.ElementVectors[i].y.ToString();

                int index = i; // Capture the index for the lambda
                tmp2.X_InputField.onEndEdit.AddListener((value) =>
                    SetPosition(index, LevelEditorManager.LevelEditorAxis.X));

                tmp2.Y_InputField.onEndEdit.AddListener((value) =>
                    SetPosition(index, LevelEditorManager.LevelEditorAxis.Y));

                _vectors2IF.Add(tmp2);
            }
        }
        else
        {
            // If vectors don't match, clear the Vector2 input fields
            foreach (var vector2IF in _vectors2IF)
            {
                Destroy(vector2IF.gameObject);
            }
            _vectors2IF.Clear();

            _dynamicPlatPosAdd.gameObject.SetActive(false);
            _dynamicPlatPosRemove.gameObject.SetActive(false);
        }

        // Compare LoopToggle
        match = true;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (element.ElementIntegerValues[0] != firstElement.ElementIntegerValues[0])
            {
                match = false;
                break;
            }
        }

        // Update LoopToggle
        if (match)
            _dynamicPlatLoopToggle.isOn = firstElement.ElementIntegerValues[0] == 1;
        else
            _dynamicPlatLoopToggle.isOn = false;

        // Compare AutoToggle
        match = true;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (element.ElementIntegerValues[1] != firstElement.ElementIntegerValues[1])
            {
                match = false;
                break;
            }
        }

        // Update AutoToggle
        if (match)
            _dynamicPlatAutoToggle.isOn = firstElement.ElementIntegerValues[1] == 1;
        else
            _dynamicPlatAutoToggle.isOn = false;

        // Compare Delay
        match = true;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (!Mathf.Approximately(element.ElementFloatValues[1], firstElement.ElementFloatValues[1]))
            {
                match = false;
                break;
            }
        }

        // Update Delay input field
        if (match)
            _dynamicPlatDelayIF.text = firstElement.ElementFloatValues[1].ToString("F2");
        else
            _dynamicPlatDelayIF.text = "-";

        // Compare Speed
        match = true;
        foreach (var element in _levelEditorManager._selectedElements)
        {
            if (!Mathf.Approximately(element.ElementFloatValues[0], firstElement.ElementFloatValues[0]))
            {
                match = false;
                break;
            }
        }

        // Update Speed input field
        if (match)
            _dynamicPlatSpeedIF.text = firstElement.ElementFloatValues[0].ToString("F2");
        else
            _dynamicPlatSpeedIF.text = "-";
    }

    void SetPosition(int index, LevelEditorManager.LevelEditorAxis axis)
    {
        foreach (var item in _levelEditorManager._selectedElements)
        {
            if (item.ElementVectors.Count <= index)
                continue;

            if (index == 0)
            {
                _vectors2IF[index].X_InputField.text = "0";
                _vectors2IF[index].Y_InputField.text = "0";
                break;
            }

            if (axis == LevelEditorManager.LevelEditorAxis.X)
                item.ElementVectors[index] = 
                    new Vector3(float.Parse(_vectors2IF[index].X_InputField.text), item.ElementVectors[index].y, 0);
            
            if (axis == LevelEditorManager.LevelEditorAxis.Y)
                item.ElementVectors[index] = 
                    new Vector3(item.ElementVectors[index].x, float.Parse(_vectors2IF[index].Y_InputField.text), 0);
        }

        UpdateDynamicPlatformLineRenderer();
    }

    void AddPosition()
    {
        GameObject tmp = Instantiate(_vector2IF, _dynamicPlatPosContent);
        Vector2InputField tmp2 = tmp.GetComponent<Vector2InputField>();
        Vector3 newPos = new
            (float.Parse(_vectors2IF[^1].X_InputField.text) + 1,
            float.Parse(_vectors2IF[^1].Y_InputField.text), 0);

        tmp2.X_InputField.text = newPos.x.ToString();
        tmp2.X_InputField.onEndEdit.AddListener((value) =>
            SetPosition(_vectors2IF.Count, LevelEditorManager.LevelEditorAxis.X));

        tmp2.Y_InputField.text = newPos.y.ToString();
        tmp2.Y_InputField.onEndEdit.AddListener((value) =>
            SetPosition(_vectors2IF.Count, LevelEditorManager.LevelEditorAxis.Y));

        _vectors2IF.Add(tmp2);

        foreach (var item in _levelEditorManager._selectedElements)
        {
            item.ElementVectors.Add(newPos);
        }

        UpdateDynamicPlatformLineRenderer();
    }

    void RemovePosition()
    {
        if (_vectors2IF.Count <= 2)
            return;

        Vector2InputField tmp = _vectors2IF[^1];
        _vectors2IF.Remove(tmp);
        Destroy(tmp.gameObject);

        foreach (var item in _levelEditorManager._selectedElements)
        {
            item.ElementVectors.RemoveAt(item.ElementVectors.Count - 1);
        }

        UpdateDynamicPlatformLineRenderer();
    }

    void UpdateDynamicPlatformLineRenderer()
    {
        foreach (var item in _levelEditorManager._selectedElements)
        {
            item._lineRenderer.positionCount = item.ElementVectors.Count;
            item._lineRenderer.SetPositions(item.ElementVectors.ToArray());
        }
    }

    void SetLoop()
    {
        foreach (var item in _levelEditorManager._selectedElements)
        {
            if (_dynamicPlatLoopToggle.isOn)
                item.ElementIntegerValues[0] = 1;
            else
                item.ElementIntegerValues[0] = 0;
        }
    }

    void SetAuto()
    {
        foreach (var item in _levelEditorManager._selectedElements)
        {
            if (_dynamicPlatAutoToggle.isOn)
                item.ElementIntegerValues[1] = 1;
            else
                item.ElementIntegerValues[1] = 0;
        }
    }

    void SetSpeed()
    {
        foreach (var item in _levelEditorManager._selectedElements)
        {
            item.ElementFloatValues[0] = float.Parse(_dynamicPlatSpeedIF.text);
        }
    }

    void SetDelay()
    {
        foreach (var item in _levelEditorManager._selectedElements)
        {
            item.ElementFloatValues[1] = float.Parse(_dynamicPlatDelayIF.text);
        }
    }

    bool VectorsMatch(List<Vector3> vectors1, List<Vector3> vectors2)
    {
        if (vectors1.Count != vectors2.Count)
            return false;

        for (int i = 0; i < vectors1.Count; i++)
        {
            if (!Mathf.Approximately(vectors1[i].x, vectors2[i].x) ||
                !Mathf.Approximately(vectors1[i].y, vectors2[i].y))
            {
                return false;
            }
        }

        return true;
    }

    private float ParseInputFieldValue(string value)
    {
        // If the value is "-", treat it as 0
        if (value == "-")
        {
            return 0f;
        }

        // Try to parse the value as a float
        if (float.TryParse(value, out float result))
        {
            return result;
        }

        // If parsing fails, return 0
        return 0f;
    }
    #endregion
}
