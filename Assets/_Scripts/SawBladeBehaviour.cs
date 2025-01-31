using UnityEngine;

public class SawBladeBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] Transform _spriteTransform;
    [SerializeField] float _rotateSpeed;
    Vector3 _movement;

    [Header("Audio")]
    [SerializeField] AudioData _sawbladeSpawnSFX;

    private void Start()
    {
        AudioManager.Instance.PlayClip(_sawbladeSpawnSFX);
        _rb.AddForce(_movement, ForceMode2D.Impulse);
    }

    public void Move(Vector3 velocity)
    {
        _movement = velocity;
    }

    private void Update()
    {
        _spriteTransform.Rotate(_rotateSpeed * -_rb.linearVelocityX * Time.deltaTime * Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("BlueBubble") ||
            collision.gameObject.CompareTag("RedBubble"))
            collision.gameObject.GetComponent<BubbleBehaviour>().PopBubble();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.KillPlayer();
        }
    }
}
