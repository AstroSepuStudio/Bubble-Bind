using UnityEngine;

public class ManualMovingPlatformBehaviour : MonoBehaviour
{
    [SerializeField] Rigidbody2D _platform;
    [SerializeField] Transform[] _positions;
    [SerializeField] float _speed;

    float _distanceLeft;
    bool _playerOnContact = false;

    private int _currentIndex = 0;
    private int _direction = -1; // 1 for forward, -1 for backward

    private void Start()
    {
        _platform.gravityScale = 0;
        _platform.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (_playerOnContact)
        {
            GameManager.Instance.Player_Movement._movingPlatformVelocity = _platform.linearVelocity;
        }

        if (_positions.Length == 0 || _distanceLeft <= 0.1f) 
        { 
            _platform.linearVelocity = Vector2.zero;
            _platform.bodyType = RigidbodyType2D.Kinematic;
            GameManager.Instance.Player_Movement._movingPlatformVelocity = Vector2.zero;
            return;
        }
        if (_platform.bodyType == RigidbodyType2D.Kinematic)
            _platform.bodyType = RigidbodyType2D.Dynamic;

        // Move the platform towards the target position
        _platform.linearVelocity = (_positions[_currentIndex].position - _platform.transform.position).normalized * _speed;

        // Check if the platform is close enough to the target
        _distanceLeft = Vector2.Distance(_platform.transform.position, _positions[_currentIndex].position);
        if (_distanceLeft <= 0.1f)
        {
            MoveToNextPosition();
        }
    }

    public void MoveToNextPosition()
    {
        if (_positions.Length == 0) return;

        // Update index
        _currentIndex += _direction;

        if (_currentIndex == _positions.Length)
        {
            _currentIndex = _positions.Length - 1;
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

    public void AddVelocityToPlayer()
    {
        _playerOnContact = true;
    }

    public void RemoveVelocityFromPlayer()
    {
        _playerOnContact = false;
        GameManager.Instance.Player_Movement._movingPlatformVelocity = Vector3.zero;
    }

    public void SetMove(bool move)
    {
        if (move)
            _direction = 1;
        else
            _direction = -1;

        MoveToNextPosition();
        // Check if the platform is close enough to the target
        _distanceLeft = Vector2.Distance(_platform.transform.position, _positions[_currentIndex].position);
    }
}
