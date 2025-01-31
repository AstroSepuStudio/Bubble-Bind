using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BubbleShooting : MonoBehaviour
{
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] GameObject _redBubblePrefab;
    [SerializeField] GameObject _blueBubblePrefab;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] LayerMask _ignorePlayer;
    [SerializeField] float _shootCooldown;
    float _shootCDtimer;

    [SerializeField] Transform _shootDirection;
    [SerializeField] Transform _shootPoint;
    [SerializeField] float _shootForce;
    Vector3 _mouseWorldPos;

    private void Start()
    {
        _playerInput.actions["ShootRed"].started += ShootRedBubble;
        _playerInput.actions["ShootBlue"].started += ShootBlueBubble;
    }

    private void Update()
    {
        if (TransitionManager._gamePaused) return;

        _mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _mouseWorldPos.z = 0f; // zero z

        _shootDirection.rotation = Quaternion.LookRotation(Vector3.forward, (_mouseWorldPos - transform.position).normalized);

        if (_shootCDtimer > 0)
            _shootCDtimer -= Time.deltaTime;
    }

    void ShootRedBubble(InputAction.CallbackContext context)
    {
        ShootBubble(_redBubblePrefab);
    }

    void ShootBlueBubble(InputAction.CallbackContext context)
    {
        ShootBubble(_blueBubblePrefab);
    }

    void ShootBubble(GameObject prefab)
    {
        if (TransitionManager._gamePaused) return;

        if (_shootCDtimer > 0) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, (_shootPoint.position - transform.position).normalized,
            Vector2.Distance(_shootPoint.position, transform.position), _ignorePlayer);

        if (hit)
        {
            if (prefab == _redBubblePrefab)
            {
                if (!hit.transform.gameObject.CompareTag("BlueCatcher"))
                    return;
            }

            if (prefab == _blueBubblePrefab)
            {
                if (!hit.transform.gameObject.CompareTag("RedCatcher"))
                    return;
            }
        }

        GameObject temp = Instantiate(prefab, _shootPoint.position, Quaternion.identity);
        temp.GetComponent<BubbleBehaviour>().Move((_shootPoint.position - transform.position).normalized * _shootForce);

        _shootCDtimer = _shootCooldown;
    }

    private void OnDestroy()
    {
        _playerInput.actions["ShootRed"].started -= ShootRedBubble;
        _playerInput.actions["ShootBlue"].started -= ShootBlueBubble;
    }
}
