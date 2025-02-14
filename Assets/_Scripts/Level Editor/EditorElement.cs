using System.Collections.Generic;
using UnityEngine;

public class EditorElement : MonoBehaviour
{
    public GameObject _child;
    public Transform _pivot;
    [SerializeField] GameObject[] _sprites;
    [SerializeField] GameObject[] _outlines;
    [SerializeField] Collider2D _collider;
    public LineRenderer _lineRenderer;

    public int ElementIndex;
    public EditorElementData _data;
    public List<int> ElementIntegerValues = new();
    public List<float> ElementFloatValues = new();
    public List<Vector3> ElementVectors = new();
    public bool Physics;
    LevelEditorManager _levelEditorManager;

    public void SetUpElement(LevelEditorManager levelEditorManager)
    {
        _levelEditorManager = levelEditorManager;

        // Perform setup based on the _data type
        switch (_data.ElementType)
        {
            case BubbleBindElement.Element.RedBubble:
                SetUpBubble();
                break;
            case BubbleBindElement.Element.BlueBubble:
                SetUpBubble();
                break;
            case BubbleBindElement.Element.Button:
                SetUpButton();
                break;
            case BubbleBindElement.Element.Cannon:
                SetUpCannon();
                break;
            case BubbleBindElement.Element.MovingPlatform:
                SetUpDynamicPlatform();
                break;
            default:
                break;
        }

        SetUpPhysics();
    }

    public void InitializeElement()
    {
        _child.SetActive(true);

        _collider.enabled = false;
        foreach (var sprite in _sprites)
        {
            sprite.SetActive(false);
        }
    }

    public void SelectElement()
    {
        foreach (var outline in _outlines)
        {
            outline.SetActive(true);
        }
    }

    public void DeselectElement()
    {
        foreach (var outline in _outlines)
        {
            outline.SetActive(false);
        }
    }

    #region Set Up Element Section

    void SetUpBubble()
    {
        if (_child.TryGetComponent<BubbleBehaviour>(out var bubbleBehaviour))
        {
            foreach (var index in ElementIntegerValues)
            {
                EditorElement linkedBubble = _levelEditorManager.GetElementByIndex(index);
                bubbleBehaviour.LinkBubbles(linkedBubble);
            }
        }
    }

    void SetUpButton()
    {
        if (_child.TryGetComponent<ButtonBehaviour>(out var buttonBehaviour))
        {
            foreach (var index in ElementIntegerValues)
            {
                var element = _levelEditorManager.GetElementByIndex(index);
                var listener = element._child.GetComponent<IListenToButton>();

                // Subscribe the element to the button events
                buttonBehaviour.OnActivation.AddListener(listener.OnButtonActivated);
                buttonBehaviour.OnDeactivation.AddListener(listener.OnButtonDeactivated);
            }
        }
    }

    void SetUpCannon()
    {
        if (_child.TryGetComponent<CannonBehaviour>(out var cannonBehaviour))
        {
            // If there is no list, create new ones with default values
            if (ElementIntegerValues.Count == 0)
            {
                ElementIntegerValues = new List<int> { 0 };

                ElementFloatValues = new List<float> { 1, 90 };
            }

            cannonBehaviour._auto = ElementIntegerValues[0] == 1;
            cannonBehaviour._cooldown = ElementFloatValues[0];
            cannonBehaviour.SetShootDirection(ElementFloatValues[1]);
            _pivot.rotation = Quaternion.Euler(0, 0, (-ElementFloatValues[1]) + 90);

            if (cannonBehaviour._cooldown < 0.1f)
                cannonBehaviour._cooldown = 0.1f;
        }
    }
    
    public void SetUpPhysics()
    {
        Rigidbody2D rb = _child.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        if (Physics)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.None;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePosition;
        }
    }

    public void SetUpDynamicPlatform()
    {
        _lineRenderer.positionCount = ElementVectors.Count;
        _lineRenderer.SetPositions(ElementVectors.ToArray());

        if (_child.TryGetComponent<DynamicPlatform>(out var dp))
        {
            dp.SetUp(ElementVectors, 
                ElementFloatValues[0], 
                ElementFloatValues[1], 
                ElementIntegerValues[1] == 1, 
                ElementIntegerValues[0] == 1);
            return;
        }
    }
    #endregion
}
