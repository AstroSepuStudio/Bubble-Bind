using UnityEngine;

public class CannonBehaviour : MonoBehaviour
{
    [SerializeField] Transform _pivotTransform;
    [SerializeField] Transform _shootPoint;

    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] float _shootForce;
    [SerializeField] float _cooldown = 1;
    [SerializeField] bool _auto;

    float _timer;

    private void Update()
    {
        if (_timer > 0) 
            _timer -= Time.deltaTime;
        else
        {
            if (_auto)
                ShootProjectile();
        }
    }

    public void ShootProjectile()
    {
        if (_timer > 0) return;

        GameObject temp = Instantiate(_projectilePrefab, _shootPoint.position, Quaternion.identity);

        if (_projectilePrefab.CompareTag("SawBlade"))
            temp.GetComponent<SawBladeBehaviour>().Move((_shootPoint.position - _pivotTransform.position).normalized * _shootForce);
        else
            temp.GetComponent<BubbleBehaviour>().Move((_shootPoint.position - _pivotTransform.position).normalized * _shootForce);

        _timer = _cooldown;
    }
}
