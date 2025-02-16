using System.Collections.Generic;
using UnityEngine;

public class InstantiateCommand : ICommand
{
    LevelEditorManager _levelEditorManager;

    EditorElement _instantiatedElement;
    EditorElementData _instantiatedElementData; 
    SavedElement _savedElement;

    public InstantiateCommand(LevelEditorManager levelEditorManager, EditorElement element)
    {
        _levelEditorManager = levelEditorManager;
        _instantiatedElement = element;
        _instantiatedElementData = element._data;
    }

    public void Execute()
    {
        _instantiatedElement = _levelEditorManager.InstantiateElement(_instantiatedElementData, true, Vector3.zero);
        _instantiatedElement.ElementIndex = _savedElement.ElementIndex;
        _instantiatedElement.transform.SetPositionAndRotation(_savedElement.position, _savedElement.rotation);
        _instantiatedElement.transform.localScale = _savedElement.scale;
        _instantiatedElement.ElementFloatValues = _savedElement.ElementFloatValues;
        _instantiatedElement.ElementIntegerValues = _savedElement.ElementIntegerValues;
        _instantiatedElement.ElementVectors = _savedElement.ElementVectors;
        _instantiatedElement.Physics = _savedElement.Physics;
    }

    public void Undo()
    {
        SavedElement savedElement = new()
        {
            ElementIndex = _instantiatedElement.ElementIndex,
            DataIndex = _levelEditorManager.GetDataIndexFromEED(_instantiatedElement._data),
            position = _instantiatedElement.transform.position,
            rotation = _instantiatedElement.transform.rotation,
            scale = _instantiatedElement.transform.localScale,
            ElementIntegerValues = new List<int>(_instantiatedElement.ElementIntegerValues),
            ElementFloatValues = new List<float>(_instantiatedElement.ElementFloatValues),
            ElementVectors = new List<Vector3>(_instantiatedElement.ElementVectors),
            Physics = _instantiatedElement.Physics
        };

        _savedElement = savedElement;
        _levelEditorManager.DestroyElement(_instantiatedElement);
    }
}

public class DuplicateCommand : ICommand
{
    LevelEditorManager _levelEditorManager;

    List<EditorElement> _instantiatedElements;
    List<EditorElementData> _instantiatedElementsData = new();
    List<SavedElement> _savedElements = new();

    public DuplicateCommand(LevelEditorManager levelEditorManager, List<EditorElement> elements)
    {
        _levelEditorManager = levelEditorManager;
        _instantiatedElements = new List<EditorElement>(elements);

        foreach (var element in _instantiatedElements)
        {
            _instantiatedElementsData.Add(element._data);
        }
    }

    public void Execute()
    {
        _instantiatedElements.Clear();
        for (int i = 0; i < _instantiatedElementsData.Count; i++)
        {
            EditorElement element = _levelEditorManager.InstantiateElement(_instantiatedElementsData[i], true, Vector3.zero);
            element.ElementIndex = _savedElements[i].ElementIndex;
            element.transform.SetPositionAndRotation(_savedElements[i].position, _savedElements[i].rotation);
            element.transform.localScale = _savedElements[i].scale;
            element.ElementFloatValues = _savedElements[i].ElementFloatValues;
            element.ElementIntegerValues = _savedElements[i].ElementIntegerValues;
            element.ElementVectors = _savedElements[i].ElementVectors;
            element.Physics = _savedElements[i].Physics;
            _instantiatedElements.Add(element);
        }

        foreach (var item in _instantiatedElements)
        {
            item.SetUpElement(_levelEditorManager);
        }
    }

    public void Undo()
    {
        _savedElements.Clear();
        for (int i = 0; i < _instantiatedElements.Count; i++)
        {
            SavedElement savedElement = new()
            {
                ElementIndex = _instantiatedElements[i].ElementIndex,
                DataIndex = _levelEditorManager.GetDataIndexFromEED(_instantiatedElements[i]._data),
                position = _instantiatedElements[i].transform.position,
                rotation = _instantiatedElements[i].transform.rotation,
                scale = _instantiatedElements[i].transform.localScale,
                ElementIntegerValues = new List<int>(_instantiatedElements[i].ElementIntegerValues),
                ElementFloatValues = new List<float>(_instantiatedElements[i].ElementFloatValues),
                ElementVectors = new List<Vector3>(_instantiatedElements[i].ElementVectors),
                Physics = _instantiatedElements[i].Physics
            };

            _savedElements.Add(savedElement);
            _levelEditorManager.DestroyElement(_instantiatedElements[i]);
        }
    }
}
