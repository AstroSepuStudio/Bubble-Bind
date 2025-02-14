using System;
using UnityEngine;

[CreateAssetMenu]
public class EditorElementData : ScriptableObject
{
    public BubbleBindElement.Element ElementType;
    public string ElementName;
    public Sprite ElementIcon;
    public GameObject ElementPrefab;
    public float SpriteScale;

    public GameObject ElementEditionWindow;
}

[Serializable]
public class ElementWindowPair
{
    public EditorElementData ElementData;
    public GameObject InstancedEditionWindow;
    public ElementEditingUtils ElementEditingUtils_;
}