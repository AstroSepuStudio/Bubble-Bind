using System.Collections.Generic;
using UnityEngine;

public class ScaleCommand : ICommand
{
    private ScaleModeBehaviour _scaleModeBehaviour;
    private List<EditorElement> _elements;
    private List<Vector3> _originalScales, _originalPosition, _newScales, _newPosition;

    public ScaleCommand(ScaleModeBehaviour scaleModeBehaviour, 
        List<EditorElement> elements,
        List<Vector3> originalScales,
        List<Vector3> originalPosition,
        List<Vector3> newScales,
        List<Vector3> newPosition)
    {
        _scaleModeBehaviour = scaleModeBehaviour;
        _elements = new(elements);
        _originalScales = new(originalScales);
        _originalPosition = new(originalPosition);
        _newScales = new(newScales);
        _newPosition = new(newPosition);
    }

    public void Execute()
    {
        for (int i = 0; i < _elements.Count; i++)
        {
            _elements[i].transform.localScale = _newScales[i];
            _elements[i].transform.position = _newPosition[i];
        }

        _scaleModeBehaviour.PositionCenterAtSelectedElementsCenter();
    }

    public void Undo()
    {
        for (int i = 0; i < _elements.Count; i++)
        {
            _elements[i].transform.localScale = _originalScales[i];
            _elements[i].transform.position = _originalPosition[i];
        }

        _scaleModeBehaviour.PositionCenterAtSelectedElementsCenter();
    }
}
