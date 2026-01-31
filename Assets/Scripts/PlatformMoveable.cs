using UnityEngine;

public class PlatformMoveable : MonoBehaviour
{
    [SerializeField] private Transform _a;
    [SerializeField] private Transform _b;
    [SerializeField] private float _speed = 5;

    private Vector2 _pointA;
    private Vector2 _pointB;
    private Vector2 _targetPoint;
    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _pointA = _a.position;
        _pointB = _b.position;
        _targetPoint = _pointB;
    }

    private void FixedUpdate()
    {
        Vector2 newPosition = Vector2.MoveTowards(transform.position, _targetPoint, _speed * Time.fixedDeltaTime);
        transform.position = newPosition;

        if (Vector2.Distance(transform.position, _targetPoint) < 0.01f)
        {
            _targetPoint = _targetPoint == _pointA ? _pointB : _pointA;
        }
    }
}