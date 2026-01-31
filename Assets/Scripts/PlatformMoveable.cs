using UnityEngine;

public class PlatformMoveable : MonoBehaviour
{
    [SerializeField] private Transform _a;
    [SerializeField] private Transform _b;
    [SerializeField] private float _speed = 5;

    private Vector3 _pointA;
    private Vector3 _pointB;
    private Vector3 _targetPoint;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _pointA = _a.position;
        _pointB = _b.position;
        _targetPoint = _pointB;
    }

    private void FixedUpdate()
    {
        Vector3 newPosition = Vector3.MoveTowards(_rb.position, _targetPoint, _speed * Time.fixedDeltaTime);
        _rb.MovePosition(newPosition);

        if (Vector3.Distance(_rb.position, _targetPoint) < 0.01f)
        {
            _targetPoint = _targetPoint == _pointA ? _pointB : _pointA;
        }
    }
}