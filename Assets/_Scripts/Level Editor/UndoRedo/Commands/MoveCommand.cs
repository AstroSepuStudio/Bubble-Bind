using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ICommand
{
    private MoveModeBehaviour _moveModeBehaviour;
    private List<EditorElement> _elements;
    private List<Vector3> _originalPositions;
    private List<Vector3> _newPositions;

    public MoveCommand(MoveModeBehaviour moveModeBehaviour, 
        List<EditorElement> elements,
        List<Vector3> originalPositions,
        List<Vector3> newPositions)
    {
        _moveModeBehaviour = moveModeBehaviour;
        _elements = new(elements);
        _originalPositions = new(originalPositions);
        _newPositions = new(newPositions);
    }

    public void Execute()
    {
        for (int i = 0; i < _elements.Count; i++)
        {
            _elements[i].transform.position = _newPositions[i];
        }

        _moveModeBehaviour.PositionArrowsAtSelectedElementsCenter();
    }

    public void Undo()
    {
        for (int i = 0; i < _elements.Count; i++)
        {
            _elements[i].transform.position = _originalPositions[i];
        }

        _moveModeBehaviour.PositionArrowsAtSelectedElementsCenter();
    }
}
