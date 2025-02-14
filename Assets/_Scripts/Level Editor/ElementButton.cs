using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementButton : MonoBehaviour
{
    [SerializeField] EditorElementData _elementData;
    [SerializeField] Image _elementIcon;
    [SerializeField] TextMeshProUGUI _elementName;
    [SerializeField] Button _button;
    Color _originalColor;

    LevelEditorManager _manager;

    public void SetUp(LevelEditorManager manager, EditorElementData elementData)
    {
        _manager = manager;
        _elementData = elementData;
        _elementIcon.sprite = _elementData.ElementIcon;
        _elementName.SetText(_elementData.ElementName);
        _originalColor = _button.image.color;
        _button.onClick.AddListener(SelectElement);
    }

    public EditorElementData GetEditorElementData()
    {
        return _elementData;
    }

    public void Deselect()
    {
        _button.image.color = _originalColor;
    }

    void SelectElement()
    {
        _button.image.color = Color.red;
        _manager.SelectElementButton(_elementData);
    }
}
