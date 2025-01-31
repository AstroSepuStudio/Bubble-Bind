using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    [SerializeField] Rigidbody2D _rb;
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] float _speed;
    [SerializeField] float _dropSpeed;
    [SerializeField] float _jumpForce;
    [SerializeField] float _coyoteJumpTime;

    public Transform _body;
    public float _bodyRotateSpeed;
    public Vector2 _movingPlatformVelocity = Vector3.zero;

    public Transform _faceTransform;
    [SerializeField] GameObject _normalExpression;
    [SerializeField] GameObject _jumpingExpression;
    [SerializeField] float _moveFaceSpeed;
    float _maxFaceDistance = 0.15f;

    [Header("Ground Detection")]
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] Transform _footTransform;
    [SerializeField] float _radiusGroundDetection;

    [SerializeField] float _isGroundedTimer;
    Vector2 _inputDirection;
    float _impulseCDtimer;
    Vector2 _momentum = Vector2.zero;
    Vector3 _originalScale;

    private void Start()
    {
        Instance = this;
        _playerInput.actions["Jump"].started += Jump;
        _originalScale = _body.localScale;
    }

    void Update()
    {
        if (TransitionManager._gamePaused) return;

        if (_isGroundedTimer > 0)
            _isGroundedTimer -= Time.deltaTime;

        if (_impulseCDtimer > 0)
            _impulseCDtimer -= Time.deltaTime;

        if (Physics2D.OverlapCircle(_footTransform.position, _radiusGroundDetection, _groundLayer))
        {
            _isGroundedTimer = _coyoteJumpTime;
            _momentum = Vector2.zero;
        }

        _inputDirection = _playerInput.actions["Move"].ReadValue<Vector2>();

        _body.Rotate(_bodyRotateSpeed * -_inputDirection.x * Time.deltaTime * Vector3.forward);

        _faceTransform.localPosition = Vector3.Slerp(_faceTransform.localPosition, _inputDirection.normalized * _maxFaceDistance, Time.deltaTime * _moveFaceSpeed);
    }

    private void FixedUpdate()
    {
        if (TransitionManager._gamePaused) return;

        // Apply platform velocity while standing on it
        if (_movingPlatformVelocity != Vector2.zero)
        {
            _momentum = _movingPlatformVelocity;
            _rb.AddForceY(4);
        }

        // Keep momentum when airborne
        if (_isGroundedTimer <= 0)
        {
            _momentum.y = Mathf.MoveTowards(_momentum.y, 0, Time.fixedDeltaTime);

            // Prevent abrupt stopping when moving against momentum
            if (_momentum.x != 0)
            {
                if ((_momentum.x > 0 && _inputDirection.x < 0) ||
                    (_momentum.x < 0 && _inputDirection.x > 0))
                {
                    _momentum.x += _inputDirection.x * _speed;
                }

                _momentum.x = Mathf.MoveTowards(_momentum.x, 0, Time.fixedDeltaTime);
            }
        }

        _rb.linearVelocityX = _inputDirection.x * _speed + _momentum.x;
    }

    void Jump(InputAction.CallbackContext context)
    {
        if (TransitionManager._gamePaused) return;

        if (_isGroundedTimer > 0)
        {
            _rb.linearVelocityY = _movingPlatformVelocity.y; // Inherit vertical movement from platform
            // Preserve platform velocity when jumping
            _momentum = _movingPlatformVelocity;

            _isGroundedTimer = 0;

            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

            StartCoroutine(JumpAnimation());
        }
    }

    IEnumerator JumpAnimation()
    {
        float timer = 0;
        Vector3 targetScale = new(0.3f, 0.3f, 0.3f);

        _normalExpression.SetActive(false);
        _jumpingExpression.SetActive(true);

        while (timer < 0.075f)
        {
            _body.localScale = Vector3.Lerp(_originalScale, targetScale, timer / 0.075f);
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0;
        while (timer < 0.25f)
        {
            _body.localScale = Vector3.Lerp(targetScale, _originalScale, timer / 0.25f);
            timer += Time.deltaTime;
            yield return null;
        }

        _normalExpression.SetActive(true);
        _jumpingExpression.SetActive(false);

        _body.localScale = _originalScale;
    }

    public void AddImpulse(Vector3 direction, float force)
    {
        if (_impulseCDtimer > 0)
            return;

        _rb.linearVelocityY = 0;
        _rb.AddForce(direction * force, ForceMode2D.Impulse);

        _impulseCDtimer = 0.2f;
    }

    private void OnDestroy()
    {
        _playerInput.actions["Jump"].started -= Jump;
        Instance = null;
    }
}
