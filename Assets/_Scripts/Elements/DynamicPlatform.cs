using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicPlatform : MonoBehaviour, IListenToButton
{
    public Rigidbody2D _platform;
    [SerializeField] List<Transform> _platformCorners;
    [SerializeField] LayerMask _groundLayer;
    public List<Vector3> _targetPositions;

    public bool _auto;
    public bool _loop;
    public float _speed;
    public float _delayBetweenPoints;
    bool _manualMove;

    WaitForSeconds _waitingTime;
    bool _isWaiting;

    float _distanceLeft;
    bool _playerOnContact = false;

    int _currentIndex = 0;
    int _lastIndex = 0;
    int _direction = 1; // 1 for forward, -1 for backward

    public void SetUp(List<Vector3> positions, float speed, float delay, bool auto, bool loop)
    {
        _targetPositions = positions;
        _speed = speed;
        _delayBetweenPoints = delay;
        _auto = auto;
        _loop = loop;
    }

    private void Start()
    {
        _platform.gravityScale = 0;
        _platform.freezeRotation = true;
        _waitingTime = new WaitForSeconds(_delayBetweenPoints);

        for (int i = 0; i < _targetPositions.Count; i++)
        {
            _targetPositions[i] += _platform.transform.position;
        }
    }

    private void FixedUpdate()
    {
        if (_targetPositions.Count == 0) return;

        if (_playerOnContact)
            GameManager.Instance.Player_Movement._movingPlatformVelocity = _platform.linearVelocity;

        if (CheckStopCondition())
        {
            _platform.linearVelocity = Vector2.zero;
            _platform.bodyType = RigidbodyType2D.Kinematic;
            return;
        }
        if (_platform.bodyType == RigidbodyType2D.Kinematic)
            _platform.bodyType = RigidbodyType2D.Dynamic;

        _platform.linearVelocity =
            (_targetPositions[_currentIndex] -
            _platform.transform.position).normalized * _speed;

        _distanceLeft = Vector2.Distance(_platform.transform.position,
            _targetPositions[_currentIndex]);

        if (_distanceLeft <= 0.1f)
        {
            DoWaitBeforeNextMove();
        }
    }

    bool CheckStopCondition()
    {
        if (_auto)
            return _isWaiting;
        else
        {
            if (_isWaiting || (_loop && !_manualMove) || 
                (!_manualMove && _direction > 0) || (_lastIndex == _currentIndex))
                return true;
            else
                return false;
        }
    }

    public void DoWaitBeforeNextMove()
    {
        if (_targetPositions.Count == 0) return;

        StartCoroutine(WaitBeforeNextMove());
    }

    private IEnumerator WaitBeforeNextMove()
    {
        _isWaiting = true; // Stop movement during delay
        _platform.linearVelocity = Vector2.zero; // Ensure platform stops completely

        yield return _waitingTime; // Wait for set time

        MoveToNextPosition();

        _isWaiting = false; // Resume movement
    }

    void MoveToNextPosition()
    {
        _lastIndex = _currentIndex;
        _currentIndex += _direction;

        if (_loop)
        {
            if (_currentIndex >= _targetPositions.Count)
            {
                _currentIndex = 0; // Loop back to the first position
            }
            else if (_currentIndex < 0)
            {
                _currentIndex = _targetPositions.Count - 1; // Loop to the last position
            }
        }
        else if (_auto)
        {
            // Check bounds and reverse direction if needed
            if (_currentIndex >= _targetPositions.Count)
            {
                _direction = -1;
                _currentIndex = _targetPositions.Count - 2; // Move to the previous valid index
            }
            else if (_currentIndex < 0)
            {
                _direction = 1;
                _currentIndex = 1; // Move to the next valid index
            }
        }
        else
        {
            if (_currentIndex == _targetPositions.Count)
            {
                _currentIndex = _targetPositions.Count - 1;
                _distanceLeft = 0;
                return;
            }
            else if (_currentIndex < 0)
            {
                _currentIndex = 0;
                _distanceLeft = 0;
                return;
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

    public void OnButtonActivated()
    {
        if (_auto) return;

        _direction = 1;
        _manualMove = true;

        if (_isWaiting)
            return;

        MoveToNextPosition();
        _distanceLeft = Vector2.Distance(_platform.transform.position, _targetPositions[_currentIndex]);
    }

    public void OnButtonDeactivated()
    {
        if (_auto) return;

        _direction = -1;
        _manualMove = false;

        if (_isWaiting)
            return;

        MoveToNextPosition();
        _distanceLeft = Vector2.Distance(_platform.transform.position, _targetPositions[_currentIndex]);
    }

#if UNITY_EDITOR_64
    private void OnDrawGizmos()
    {
        if (_targetPositions.Count == 0) return;

        Gizmos.color = Color.yellow;

        List<Vector3> tmp = new(_targetPositions);

        for (int i = 0; i < tmp.Count; i++)
        {
            tmp[i] += _platform.transform.position;
        }

        for (int i = 0; i < tmp.Count; i++)
        {
            if (i + 1 == tmp.Count)
            {
                if (_loop)
                    Gizmos.DrawLine(tmp[i], tmp[0]);
                break;
            }

            Gizmos.DrawLine(tmp[i], tmp[i + 1]);
        }

        foreach (var item in _platformCorners)
        {
            Vector3 dir = _platform.linearVelocity.normalized * 0.2f;
            Gizmos.DrawLine(item.position, item.position + dir);
        }
    }
#endif
}
