using System.Collections;
using UnityEngine;

public class AutomaticMovingPlatformBehaviour : MonoBehaviour
{
    [SerializeField] Rigidbody2D _platform;
    [SerializeField] Transform[] _positions;
    [SerializeField] bool _loop;
    [SerializeField] float _speed;
    [SerializeField] float _delayBetweenPoints;
    WaitForSeconds _waitingTime;
    bool _isWaiting;

    float _distanceLeft;
    bool _playerOnContact = false;

    private int _currentIndex = 0;
    private int _direction = 1; // 1 for forward, -1 for backward

    private void Start()
    {
        _platform.gravityScale = 0;
        _platform.freezeRotation = true;
        _waitingTime = new WaitForSeconds(_delayBetweenPoints);
    }

    private void FixedUpdate()
    {
        if (_playerOnContact)
        {
            GameManager.Instance.Player_Movement._movingPlatformVelocity = _platform.linearVelocity;
        }

        if (_positions.Length == 0 || _isWaiting)
        {
            _platform.linearVelocity = Vector2.zero;
            _platform.bodyType = RigidbodyType2D.Kinematic;
            return;
        }
        if (_platform.bodyType == RigidbodyType2D.Kinematic)
            _platform.bodyType = RigidbodyType2D.Dynamic;

        // Move platform towards target position
        _platform.linearVelocity = (_positions[_currentIndex].position - _platform.transform.position).normalized * _speed;

        _distanceLeft = Vector2.Distance(_platform.transform.position, _positions[_currentIndex].position);
        if (_distanceLeft <= 0.1f) // Adjust threshold if needed
        {
            StartCoroutine(WaitBeforeNextMove());
        }
    }

    private IEnumerator WaitBeforeNextMove()
    {
        _isWaiting = true; // Stop movement during delay
        _platform.linearVelocity = Vector2.zero; // Ensure platform stops completely

        yield return _waitingTime; // Wait for set time

        MoveToNextPosition();
        _isWaiting = false; // Resume movement
    }

    public void MoveToNextPosition()
    {
        if (_positions.Length == 0) return;

        // Update index
        _currentIndex += _direction;

        if (_loop)
        {
            if (_currentIndex >= _positions.Length)
            {
                _currentIndex = 0; // Loop back to the first position
            }
            else if (_currentIndex < 0)
            {
                _currentIndex = _positions.Length - 1; // Loop to the last position
            }
        }
        else
        {
            // Check bounds and reverse direction if needed
            if (_currentIndex >= _positions.Length)
            {
                _direction = -1;
                _currentIndex = _positions.Length - 2; // Move to the previous valid index
            }
            else if (_currentIndex < 0)
            {
                _direction = 1;
                _currentIndex = 1; // Move to the next valid index
            }
        }
    }

    public void AddVelocityToPlayer()
    {
        _playerOnContact = true;
    }

    public void RemoveVelocityFromPlayer()
    {
        _playerOnContact = false;
        GameManager.Instance.Player_Movement._movingPlatformVelocity = Vector3.zero;
    }
}
