// using System;
// using Azathrix.GameKit.Runtime.Behaviours;
// using UnityEngine;
//
// [RequireComponent(typeof(BoxCollider2D))]
// public class EndPointChecker : GameScript
// {
//     public event Action OnEnterEndPointEvent;
//     private int _count;
//
//     private BoxCollider2D _boxCollider2D;
//
//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("EndPoint") )
//         {
//             OnEnterEndPointEvent?.Invoke();
//         }
//     }
//
//     private void OnDrawGizmos()
//     {
//         if (_boxCollider2D==null)
//             _boxCollider2D =  GetComponent<BoxCollider2D>();
//         Gizmos.color = Color.chocolate;
//         Gizmos.DrawWireCube(_boxCollider2D.bounds.center, _boxCollider2D.bounds.size);
//         Gizmos.color = Color.white;
//     }
// }