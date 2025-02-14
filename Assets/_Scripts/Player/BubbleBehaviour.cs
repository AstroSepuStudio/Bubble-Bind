using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle;
using UnityEngine;

public class BubbleBehaviour : MonoBehaviour
{
    [Header("Physics Related")]
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] FixedJoint2D _joint;
    [SerializeField] float _playerImpulseForce;

    bool _rbLinked;

    [Header("Bubble Specifics")]
    [SerializeField] bool _startStatic;
    [SerializeField] List<BubbleBehaviour> _linkedBubbles;
    [SerializeField] GameObject _popParticles;

    [SerializeField] Transform _spriteTransform;
    [SerializeField] float _inflationTime;
    [SerializeField] float _initialScale;
    [SerializeField] float _targetScale;
    [SerializeField] BubbleBindElement.Element _bubbleColor;
    [SerializeField] BubbleBindElement.Element _bubblyBox;

    BubbleBindElement.Element _oppositeBubble;
    BubbleBindElement.Element _bubbleCatcher;

    Vector3 _movement;

    [Header("Audio")]
    [SerializeField] AudioData _popSFX;
    [SerializeField] AudioData _inflateSFX;

    private void Start()
    {
        if (_startStatic)
            StopBubble();
        else
        {
            StartCoroutine(InflateBubble());
            _rb.AddForce(_movement, ForceMode2D.Impulse);
            AudioManager.Instance.PlayClip(_inflateSFX);
        }

        if (_bubbleColor == BubbleBindElement.Element.RedBubble)
        {
            _bubbleCatcher = BubbleBindElement.Element.RedCatcher;
            _oppositeBubble = BubbleBindElement.Element.BlueBubble;
        }
        else if (_bubbleColor == BubbleBindElement.Element.BlueBubble)
        {
            _bubbleCatcher = BubbleBindElement.Element.BlueCatcher;
            _oppositeBubble = BubbleBindElement.Element.RedBubble;
        }
    }

    public void LinkBubbles(List<EditorElement> bubbles)
    {
        _linkedBubbles.Clear();
        foreach (var bubble in bubbles)
        {
            _linkedBubbles.Add(bubble._child.GetComponent<BubbleBehaviour>());
        }
    }

    public void LinkBubbles(EditorElement bubble)
    {
        _linkedBubbles.Add(bubble._child.GetComponent<BubbleBehaviour>());
    }

    public void ClearLinks()
    {
        _linkedBubbles.Clear();
    }

    IEnumerator InflateBubble()
    {
        float timer = 0f;
        float lerp;
        Vector3 currentScale = Vector3.zero;

        while (timer < 1)
        {
            lerp = Mathf.Lerp(_initialScale, _targetScale, timer);

            if (timer < 0.333333f)
                timer += 1 / _inflationTime * Time.deltaTime * 3;
            if (timer < 0.666666f)
                timer += 1 / _inflationTime * Time.deltaTime;
            else
                timer += 1 / _inflationTime * Time.deltaTime / 3;

            currentScale.x = lerp;
            currentScale.y = lerp;
            currentScale.z = lerp;
            _spriteTransform.localScale = currentScale;
            yield return null;
        }
    }

    private void Update()
    {
        if (TransitionManager._gamePaused) return;

        if (_rbLinked)
        {
            if (_joint.connectedBody == null)
            {
                _rb.gravityScale = 1;
            }
        }
    }

    public void PopBubble()
    {
        for (int i = 0; i < _linkedBubbles.Count; i++)
        {
            if (_linkedBubbles[i] == this) continue;

            _linkedBubbles[i].PopBubbleRaw();
        }

        PopBubbleRaw();
        AudioManager.Instance.PlayClip(_popSFX);
    }

    void PopBubbleRaw()
    {
        _popParticles.transform.parent = null;
        _popParticles.SetActive(true);
        Destroy(_popParticles.gameObject, 3f);
        Destroy(gameObject);
    }

    void StopBubble()
    {
        _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
    }

    public void Move(Vector3 velocity)
    {
        _movement = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(_bubblyBox.ToString()) ||
            collision.gameObject.CompareTag(_oppositeBubble.ToString()))
        {
            if (!_rbLinked)
            {
                _rb.linearVelocity = Vector2.zero;
                _joint.connectedBody = collision.rigidbody;
                _joint.enabled = true;
                _rbLinked = true;
                _rb.gravityScale = 1;

                if (_startStatic)
                    _rb.freezeRotation = true;
            }
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            PopBubble();
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (PlayerMovement.Instance.transform.position.y > transform.position.y)
                PlayerMovement.Instance.AddImpulse(Vector2.up, _playerImpulseForce);
            else
                PlayerMovement.Instance.AddImpulse(Vector2.down, _playerImpulseForce);

            PopBubble();
        }

        if (collision.gameObject.CompareTag(_bubbleColor.ToString()))
        {
            for (int i = 0; i < _linkedBubbles.Count; i++)
            {
                if (collision.gameObject == _linkedBubbles[i].gameObject)
                    return;
            }

            collision.gameObject.GetComponent<BubbleBehaviour>().PopBubble();
            PopBubble();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(_bubbleCatcher.ToString()))
        {
            PopBubble();
        }
    }
}
