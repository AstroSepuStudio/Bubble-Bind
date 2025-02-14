using UnityEngine;
using UnityEngine.Events;

public class GameTickManager : MonoBehaviour
{
    [SerializeField] int _ticksPerSecond;
    public static UnityEvent OnTick = new();

    float _timer;

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            OnTick?.Invoke();
            _timer = 1 / _ticksPerSecond;
        }
    }
}
