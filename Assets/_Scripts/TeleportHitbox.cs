using UnityEngine;

public class TeleportHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            GameManager.Instance.TeleportPlayerToSpawn();
    }
}
