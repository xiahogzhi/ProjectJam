using UnityEngine;

/// <summary>
/// 碰撞事件转发器
/// 将子碰撞体的事件转发到父级SliceableCollider
/// </summary>
public class ColliderEventForwarder : MonoBehaviour
{
    private SliceableCollider _parent;

    public void Initialize(SliceableCollider parent)
    {
        _parent = parent;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        _parent?.ForwardTriggerEnter(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        _parent?.ForwardTriggerExit(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        _parent?.ForwardTriggerStay(other);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        _parent?.ForwardCollisionEnter(other);
    }

    void OnCollisionExit2D(Collision2D other)
    {
        _parent?.ForwardCollisionExit(other);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        _parent?.ForwardCollisionStay(other);
    }
}
