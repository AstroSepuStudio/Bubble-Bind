using UnityEngine;

public class MovingPlatformComponent : MonoBehaviour
{
    [SerializeField] AutomaticMovingPlatformBehaviour _autoPlatformBehaviour;
    [SerializeField] ManualMovingPlatformBehaviour _manualPlatformBehaviour;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_autoPlatformBehaviour != null)
                _autoPlatformBehaviour.AddVelocityToPlayer();
            else if (_manualPlatformBehaviour != null)
                _manualPlatformBehaviour.AddVelocityToPlayer();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_autoPlatformBehaviour != null)
                _autoPlatformBehaviour.RemoveVelocityFromPlayer();
            else if (_manualPlatformBehaviour != null)
                _manualPlatformBehaviour.RemoveVelocityFromPlayer();
        }
    }
}
