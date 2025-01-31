using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonBehaviour : MonoBehaviour
{
    public UnityEvent OnActivation;
    public UnityEvent OnDeactivation;

    [SerializeField] SpriteRenderer _buttonStateSprite;
    [SerializeField] Sprite _activatedSprite;
    [SerializeField] Sprite _deactivatedSprite;

    [SerializeField] Animator _animator;

    List<GameObject> _gameobjectsOnContact = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_gameobjectsOnContact.Count == 0)
        {
            _buttonStateSprite.sprite = _activatedSprite;
            _animator.SetTrigger("Activate");
            OnActivation?.Invoke();
        }

        _gameobjectsOnContact.Add(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _gameobjectsOnContact.Remove(collision.gameObject);

        if (_gameobjectsOnContact.Count == 0)
        {
            _buttonStateSprite.sprite = _deactivatedSprite;
            _animator.SetTrigger("Deactivate");
            OnDeactivation?.Invoke();
        }
    }
}
