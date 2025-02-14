using UnityEngine;

public class CannonBehaviour : MonoBehaviour, IListenToButton
{
    [SerializeField] Transform _pivotTransform;
    [SerializeField] Transform _shootPoint;

    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] float _shootForce;
    public float _cooldown = 1;
    public bool _auto;

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

    public void OnButtonActivated()
    {
        ShootProjectile();
    }

    public void OnButtonDeactivated()
    {

    }

    public void SetShootDirection(float angle)
    {
        _pivotTransform.rotation = Quaternion.Euler(0, 0, (-angle) + 90);
    }
}
