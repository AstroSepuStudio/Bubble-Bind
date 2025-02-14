using System.Collections;
using UnityEngine;

public class GoalBehaviour : MonoBehaviour
{
    [SerializeField] string _nextLevel;
    [SerializeField] Collider2D _collider;

    Transform _playerTransform;

    IEnumerator MakePlayerEnterGoal()
    {
        _playerTransform = GameManager.Instance.Player_Movement.transform;
        _collider.enabled = false;
        Vector3 originalPosition = _playerTransform.position;
        float rotationSpeed = GameManager.Instance.Player_Movement._bodyRotateSpeed;
        float timer = 0;
        Vector3 scale = Vector3.one;
        while (timer < 0.49f)
        {
            GameManager.Instance.Player_Movement._body.Rotate(rotationSpeed * -(1 + timer * 2) * Time.deltaTime * Vector3.forward);

            _playerTransform.position = Vector3.Lerp(originalPosition, transform.position, timer / 0.5f);
            scale.x -= Time.deltaTime;
            scale.y -= Time.deltaTime;
            scale.z -= Time.deltaTime;
            _playerTransform.localScale = scale;

            if (timer < 0.166666f)
                timer += Time.deltaTime * 2;
            if (timer < 0.333333f)
                timer += Time.deltaTime;
            else
                timer += Time.deltaTime / 2;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.DisablePlayer();
            StartCoroutine(MakePlayerEnterGoal());
            GameManager.Instance.LoadScene(_nextLevel);
        }
    }
}
