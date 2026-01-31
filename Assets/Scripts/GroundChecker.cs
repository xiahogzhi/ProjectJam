using System;
using Azathrix.GameKit.Runtime.Behaviours;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GroundChecker : GameScript
{
    public event Action<bool> OnGroundStateChangedEvent;
    private int _count;

    private BoxCollider2D _boxCollider2D;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground") ||  other.CompareTag("Wall"))
        {
            if (_count == 0)
                OnGroundStateChangedEvent?.Invoke(true);
            _count++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground")||  other.CompareTag("Wall"))
        {
            _count--;
            if (_count == 0)
                OnGroundStateChangedEvent?.Invoke(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (_boxCollider2D==null)
            _boxCollider2D =  GetComponent<BoxCollider2D>();
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawWireCube(_boxCollider2D.bounds.center, _boxCollider2D.bounds.size);
        Gizmos.color = Color.white;
    }
}