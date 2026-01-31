using System;
using Azathrix.GameKit.Runtime.Behaviours;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HurtChecker : GameScript
{
    public event Action OnHurtEvent;

    private BoxCollider2D _boxCollider2D;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("HurtBox"))
        {
            OnHurtEvent?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        if (_boxCollider2D == null)
            _boxCollider2D = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.chocolate;
        Gizmos.DrawWireCube(_boxCollider2D.bounds.center, _boxCollider2D.bounds.size);
        Gizmos.color = Color.white;
    }
}