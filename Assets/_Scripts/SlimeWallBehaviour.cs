using UnityEngine;

public class SlimeWallBehaviour : MonoBehaviour
{
    [SerializeField] Transform _forcedBouncePosition;
    [SerializeField] float _bounceForce = 2.5f;
    Vector2 _targetPos;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("BlueBubble") ||
            collision.gameObject.CompareTag("RedBubble"))
        {
            Rigidbody2D rb = collision.rigidbody;

            if (_forcedBouncePosition != null)
            {
                _targetPos.x = _forcedBouncePosition.position.x;
                _targetPos.y = _forcedBouncePosition.position.y;
                rb.linearVelocity = Vector2.zero;
                rb.AddForce((_targetPos - collision.GetContact(0).point).normalized * _bounceForce, ForceMode2D.Impulse);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(Vector2.Reflect(collision.relativeVelocity, collision.GetContact(0).normal).normalized * _bounceForce, ForceMode2D.Impulse);
            }
        }
    }
}
