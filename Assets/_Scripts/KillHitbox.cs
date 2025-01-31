using UnityEngine;

public class KillHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("RedBubble") ||
            collision.CompareTag("BlueBubble"))
            collision.GetComponent<BubbleBehaviour>().PopBubble();

        if (collision.CompareTag("Player"))
            GameManager.Instance.KillPlayer();
    }
}
