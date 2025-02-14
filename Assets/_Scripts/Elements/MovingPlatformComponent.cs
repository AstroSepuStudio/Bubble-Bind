using UnityEngine;

public class MovingPlatformComponent : MonoBehaviour
{
    [SerializeField] DynamicPlatform _dynamicPlatform;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_dynamicPlatform != null)
                _dynamicPlatform.AddVelocityToPlayer();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_dynamicPlatform != null)
                _dynamicPlatform.RemoveVelocityFromPlayer();
        }
    }
}
