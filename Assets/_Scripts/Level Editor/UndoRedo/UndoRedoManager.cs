using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager
{
    Stack<ICommand> _undoStack = new();
    Stack<ICommand> _redoStack = new();

    #region Logic
    // Records a new action and adds it to the undo stack.
    // Clears the redo stack if a new action is recorded after an undo.
    void RecordAction(ICommand command)
    {
        _undoStack.Push(command); // Add to undo stack
        _redoStack.Clear(); // Clear redo stack when a new action is recorded
    }

    // Undoes the last action.
    public void Undo()
    {
        if (_undoStack.Count > 0)
        {
            ICommand command = _undoStack.Pop(); // Get the last action
            command.Undo(); // Undo the action
            _redoStack.Push(command); // Add to redo stack
        }
    }

    // Redoes the last undone action.
    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
            ICommand command = _redoStack.Pop(); // Get the last undone action
            command.Execute(); // Redo the action
            _undoStack.Push(command); // Add back to undo stack
        }
    }

    // Clears both undo and redo stacks.
    public void ClearHistory()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
    #endregion

    #region Recording Commands
    public void RecordMoveAction(MoveModeBehaviour moveModeBehaviour, 
        List<EditorElement> elements,
        List<Vector3> originalPositions,
        List<Vector3> newPositions)
    {
        ICommand moveCommand = new MoveCommand(moveModeBehaviour, elements, originalPositions, newPositions);
        RecordAction(moveCommand);
    }

    public void RecordRotateAction(RotateModeBehaviour rotateModeBehaviour, 
        List<EditorElement> elements,
        List<Quaternion> originalRotations,
        List<Vector3> originalPosition,
        List<Quaternion> newRotations,
        List<Vector3> newPosition)
    {
        ICommand rotateCommand = new RotateCommand(rotateModeBehaviour, 
            elements, 
            originalRotations, originalPosition, 
            newRotations, newPosition);

        RecordAction(rotateCommand);
    }

    public void RecordScaleAction(ScaleModeBehaviour scaleModeBehaviour, 
        List<EditorElement> elements,
        List<Vector3> originalScales,
        List<Vector3> originalPosition,
        List<Vector3> newScales,
        List<Vector3> newPosition)
    {
        ICommand scaleCommand = new ScaleCommand(scaleModeBehaviour, elements, 
            originalScales, originalPosition, 
            newScales, newPosition);

        RecordAction(scaleCommand);
    }

    public void RecordSelectAction(LevelEditorManager levelEditorManager, EditorElement element)
    {
        ICommand selectCommand = new SelectCommand(levelEditorManager, element);
        RecordAction(selectCommand);
    }

    public void RecordExclusiveSelectAction(LevelEditorManager levelEditorManager, 
        EditorElement element, List<EditorElement> deselectedElements)
    {
        ICommand selectCommand = new ExclusiveSelectCommand(levelEditorManager, element, deselectedElements);
        RecordAction(selectCommand);
    }

    public void RecordBulkSelectAction(LevelEditorManager levelEditorManager, List<EditorElement> elements)
    {
        ICommand selectCommand = new BulkSelectCommand(levelEditorManager, elements);
        RecordAction(selectCommand);
    }

    public void RecordDeselectAction(LevelEditorManager levelEditorManager, EditorElement element)
    {
        ICommand selectCommand = new DeselectCommand(levelEditorManager, element);
        RecordAction(selectCommand);
    }

    public void RecordBulkDeselectAction(LevelEditorManager levelEditorManager, List<EditorElement> elements)
    {
        ICommand selectCommand = new BulkDeselectCommand(levelEditorManager, elements);
        RecordAction(selectCommand);
    }
    #endregion
}
