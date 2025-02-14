using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavedElement
{
    public string ElementName;
    public int ElementIndex; // Index of the instanced element
    public int DataIndex; // Index of the element's data
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public List<int> ElementIntegerValues;
    public List<float> ElementFloatValues;
    public List<Vector3> ElementVectors;
    public bool Physics;
}
