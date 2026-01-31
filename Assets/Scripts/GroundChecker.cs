using System;
using Azathrix.GameKit.Runtime.Behaviours;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GroundChecker : GameScript
{
    public event Action<bool> OnGroundStateChangedEvent;
    public event Action<Transform> OnPlatformEnterEvent;
    private int _count;

    private BoxCollider2D _boxCollider2D;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // if (other.CompareTag("Platform"))
        // {
        //     var p = other.GetComponentInParent<PlatformMoveable>();
        //     OnPlatformEnterEvent?.Invoke(p.transform);
        // }
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Platform"))
        {
            if (_count == 0)
            {
                OnGroundStateChangedEvent?.Invoke(true);
            }

            _count++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // if (other.CompareTag("Platform"))
        //     OnPlatformEnterEvent?.Invoke(null);
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Platform"))
        {
            _count--;
            if (_count == 0)
            {
                OnGroundStateChangedEvent?.Invoke(false);
               
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_boxCollider2D == null)
            _boxCollider2D = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawWireCube(_boxCollider2D.bounds.center, _boxCollider2D.bounds.size);
        Gizmos.color = Color.white;
    }
}