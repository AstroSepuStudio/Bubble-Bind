using System.Collections.Generic;

public class DeselectCommand : ICommand
{
    private LevelEditorManager _levelEditorManager;
    private EditorElement _deselectedElement;

    public DeselectCommand(LevelEditorManager levelEditorManager, EditorElement editorElement)
    {
        _levelEditorManager = levelEditorManager;
        _deselectedElement = editorElement;
    }

    public void Execute()
    {
        _levelEditorManager.DeselectElement(_deselectedElement);
    }

    public void Undo()
    {
        _levelEditorManager.SelectElement(_deselectedElement);
    }
}

public class BulkDeselectCommand : ICommand
{
    private LevelEditorManager _levelEditorManager;
    private List<EditorElement> _deselectedElements;

    public BulkDeselectCommand(LevelEditorManager levelEditorManager, List<EditorElement> elements)
    {
        _levelEditorManager = levelEditorManager;
        _deselectedElements = new(elements);
    }

    public void Execute()
    {
        foreach (var item in _deselectedElements)
        {
            _levelEditorManager.DeselectElement(item);
        }
    }

    public void Undo()
    {
        foreach (var item in _deselectedElements)
        {
            _levelEditorManager.SelectElement(item);
        }
    }
}
