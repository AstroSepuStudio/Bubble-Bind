using System.Collections.Generic;
using UnityEngine;

public class RotateCommand : ICommand
{
    private RotateModeBehaviour _rotateModeBehaviour;
    private List<EditorElement> _elements;
    private List<Quaternion> _elementsOriginalRotation;
    private List<Vector3> _elementsOriginalPosition;
    private List<Quaternion> _elementsNewRotation;
    private List<Vector3> _elementsNewPosition;

    public RotateCommand(RotateModeBehaviour rotateModeBehaviour, 
        List<EditorElement> elements, 
        List<Quaternion> originalRotations,
        List<Vector3> originalPosition,
        List<Quaternion> newRotations,
        List<Vector3> newPosition)
    {
        _rotateModeBehaviour = rotateModeBehaviour;
        _elements = new(elements);
        _elementsOriginalRotation = new(originalRotations);
        _elementsOriginalPosition = new(originalPosition);
        _elementsNewRotation = new(newRotations);
        _elementsNewPosition = new(newPosition);
    }

    public void Execute()
    {
        for (int i = 0; i < _elements.Count; i++)
        {
            _elements[i].transform.rotation = _elementsNewRotation[i];
            _elements[i].transform.position = _elementsNewPosition[i];
        }

        _rotateModeBehaviour.PositionCenterAtSelectedElementsCenter();
    }

    public void Undo()
    {
        for (int i = 0; i < _elements.Count; i++)
        {
            _elements[i].transform.rotation = _elementsOriginalRotation[i];
            _elements[i].transform.position = _elementsOriginalPosition[i];
        }

        _rotateModeBehaviour.PositionCenterAtSelectedElementsCenter();
    }
}
