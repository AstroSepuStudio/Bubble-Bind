using System.Collections.Generic;

public class SelectCommand : ICommand
{
    private LevelEditorManager _levelEditorManager;
    private EditorElement _selectedElement;

    public SelectCommand(LevelEditorManager levelEditorManager, EditorElement editorElement)
    {
        _levelEditorManager = levelEditorManager;
        _selectedElement = editorElement;
    }

    public void Execute()
    {
        _levelEditorManager.SelectElement(_selectedElement);
    }

    public void Undo()
    {
        _levelEditorManager.DeselectElement(_selectedElement);
    }
}

public class ExclusiveSelectCommand : ICommand
{
    private LevelEditorManager _levelEditorManager;
    private EditorElement _selectedElement;
    private List<EditorElement> _deselectedElements;

    public ExclusiveSelectCommand(LevelEditorManager levelEditorManager, 
        EditorElement editorElement, 
        List<EditorElement> deselectedElements)
    {
        _levelEditorManager = levelEditorManager;
        _selectedElement = editorElement;
        _deselectedElements = new(deselectedElements);
    }

    public void Execute()
    {
        _levelEditorManager.SelectElement(_selectedElement);

        foreach (var item in _deselectedElements)
        {
            _levelEditorManager.DeselectElement(item);
        }
    }

    public void Undo()
    {
        _levelEditorManager.DeselectElement(_selectedElement);

        foreach (var item in _deselectedElements)
        {
            _levelEditorManager.SelectElement(item);
        }
    }
}

public class BulkSelectCommand : ICommand
{
    private LevelEditorManager _levelEditorManager;
    private List<EditorElement> _selectedElements;

    public BulkSelectCommand(LevelEditorManager levelEditorManager, List<EditorElement> elements)
    {
        _levelEditorManager = levelEditorManager;
        _selectedElements = new(elements);
    }

    public void Execute()
    {
        foreach (var item in _selectedElements)
        {
            _levelEditorManager.SelectElement(item);
        }
    }

    public void Undo()
    {
        foreach (var item in _selectedElements)
        {
            _levelEditorManager.DeselectElement(item);
        }
    }
}
