using System;
using UnityEngine;

/// <summary>
/// 可被切割的碰撞体组件
/// 挂在需要被mask切割的碰撞体上
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class SliceableCollider : MonoBehaviour
{
     private BoxCollider2D _originalCollider;

    // 预创建的5个子碰撞体
    private BoxCollider2D _topCollider;
    private BoxCollider2D _bottomCollider;
    private BoxCollider2D _leftCollider;
    private BoxCollider2D _rightCollider;
    private BoxCollider2D _centerCollider; // mask范围内的部分

    private bool _isSliced = false;
    private int _originalLayer;
    private int _currentTargetLayer;
    private Bounds _cachedOriginalBounds; // 缓存原始bounds

    // 事件转发 - 保留原始碰撞体的事件功能
    public event Action<Collider2D> OnSlicedTriggerEnter;
    public event Action<Collider2D> OnSlicedTriggerExit;
    public event Action<Collider2D> OnSlicedTriggerStay;
    public event Action<Collision2D> OnSlicedCollisionEnter;
    public event Action<Collision2D> OnSlicedCollisionExit;
    public event Action<Collision2D> OnSlicedCollisionStay;

    public bool IsSliced => _isSliced;
    public BoxCollider2D OriginalCollider => _originalCollider;
    public int OriginalLayer => _originalLayer;

    /// <summary>
    /// 获取原始碰撞体的bounds（即使被禁用也返回正确值）
    /// </summary>
    public Bounds OriginalBounds
    {
        get
        {
            if (_isSliced)
                return _cachedOriginalBounds;
            return _originalCollider.bounds;
        }
    }

    void Awake()
    {
        if (_originalCollider == null)
            _originalCollider = GetComponent<BoxCollider2D>();

        _originalLayer = gameObject.layer;

        // 预创建5个子碰撞体
        _topCollider = CreateSubCollider("Top");
        _bottomCollider = CreateSubCollider("Bottom");
        _leftCollider = CreateSubCollider("Left");
        _rightCollider = CreateSubCollider("Right");
        _centerCollider = CreateSubCollider("Center");

        // 初始状态：子碰撞体禁用
        SetSubCollidersActive(false);
    }

    BoxCollider2D CreateSubCollider(string partName)
    {
        var go = new GameObject($"{gameObject.name}_Sub_{partName}");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.layer = gameObject.layer;
        go.tag = gameObject.tag;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = _originalCollider.isTrigger;
        col.sharedMaterial = _originalCollider.sharedMaterial;
        col.enabled = false;

        // 添加事件转发器
        var forwarder = go.AddComponent<ColliderEventForwarder>();
        forwarder.Initialize(this);

        return col;
    }

    void SetSubCollidersActive(bool active)
    {
        _topCollider.enabled = active;
        _bottomCollider.enabled = active;
        _leftCollider.enabled = active;
        _rightCollider.enabled = active;
        _centerCollider.enabled = active;
    }

    void SetOuterCollidersLayer(int layer)
    {
        // 外围4个子碰撞体保持原始层级
        _topCollider.gameObject.layer = layer;
        _bottomCollider.gameObject.layer = layer;
        _leftCollider.gameObject.layer = layer;
        _rightCollider.gameObject.layer = layer;
    }

    void SetCenterColliderLayer(int layer)
    {
        // 中心碰撞体设置为目标层级
        _centerCollider.gameObject.layer = layer;
    }

    /// <summary>
    /// 应用切割
    /// </summary>
    /// <param name="maskBounds">mask的世界空间bounds</param>
    /// <param name="targetLayer">切割后子碰撞体的目标层级，-1表示保持原层级</param>
    public void ApplySlice(Bounds maskBounds, int targetLayer = -1)
    {
        // 使用缓存的bounds或实时获取
        Bounds originalBounds;
        if (_isSliced)
        {
            originalBounds = _cachedOriginalBounds;
        }
        else
        {
            originalBounds = _originalCollider.bounds;
            _cachedOriginalBounds = originalBounds; // 缓存
        }

        // 检查是否有重叠
        if (!maskBounds.Intersects(originalBounds))
        {
            if (_isSliced) RestoreCollider();
            return;
        }

        _isSliced = true;
        _originalCollider.enabled = false;

        // 设置层级：外围保持原始层级，中心设置为目标层级
        _currentTargetLayer = targetLayer >= 0 ? targetLayer : _originalLayer;
        SetOuterCollidersLayer(_originalLayer);
        SetCenterColliderLayer(_currentTargetLayer);

        // 计算重叠区域（世界坐标）
        float overlapMinX = Mathf.Max(originalBounds.min.x, maskBounds.min.x);
        float overlapMaxX = Mathf.Min(originalBounds.max.x, maskBounds.max.x);
        float overlapMinY = Mathf.Max(originalBounds.min.y, maskBounds.min.y);
        float overlapMaxY = Mathf.Min(originalBounds.max.y, maskBounds.max.y);

        // Debug.Log($"[SliceableCollider] Original: {originalBounds}, Mask: {maskBounds}");
        // Debug.Log($"[SliceableCollider] Overlap: minX={overlapMinX}, maxX={overlapMaxX}, minY={overlapMinY}, maxY={overlapMaxY}");

        // 更新4个子碰撞体
        // 上部分：原始顶部到重叠区域顶部
        UpdateSubCollider(_topCollider, "Top",
            originalBounds.min.x, overlapMaxY,
            originalBounds.max.x, originalBounds.max.y);

        // 下部分：原始底部到重叠区域底部
        UpdateSubCollider(_bottomCollider, "Bottom",
            originalBounds.min.x, originalBounds.min.y,
            originalBounds.max.x, overlapMinY);

        // 左部分：原始左边到重叠区域左边（只在重叠高度范围内）
        UpdateSubCollider(_leftCollider, "Left",
            originalBounds.min.x, overlapMinY,
            overlapMinX, overlapMaxY);

        // 右部分：重叠区域右边到原始右边（只在重叠高度范围内）
        UpdateSubCollider(_rightCollider, "Right",
            overlapMaxX, overlapMinY,
            originalBounds.max.x, overlapMaxY);

        // 中心部分：mask范围内的区域（设置为目标层级）
        UpdateSubCollider(_centerCollider, "Center",
            overlapMinX, overlapMinY,
            overlapMaxX, overlapMaxY);
    }

    void UpdateSubCollider(BoxCollider2D col, string name, float minX, float minY, float maxX, float maxY)
    {
        float width = maxX - minX;
        float height = maxY - minY;

        // Debug.Log($"[SliceableCollider] {name}: width={width}, height={height}");

        // 太小就禁用
        if (width < 0.001f || height < 0.001f)
        {
            col.enabled = false;
            return;
        }

        col.enabled = true;

        // 计算世界空间中心点
        Vector2 worldCenter = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);

        // 转换为相对于父物体的本地坐标
        Vector2 localCenter = transform.InverseTransformPoint(worldCenter);

        col.offset = localCenter;
        col.size = new Vector2(width / transform.lossyScale.x, height / transform.lossyScale.y);
    }

    /// <summary>
    /// 恢复原始碰撞体
    /// </summary>
    public void RestoreCollider()
    {
        if (!_isSliced) return;

        _isSliced = false;
        _originalCollider.enabled = true;
        SetSubCollidersActive(false);

        // 恢复子碰撞体的原始层级（为下次切割准备）
        SetOuterCollidersLayer(_originalLayer);
        SetCenterColliderLayer(_originalLayer);
    }

    // 供事件转发器调用
    internal void ForwardTriggerEnter(Collider2D other) => OnSlicedTriggerEnter?.Invoke(other);
    internal void ForwardTriggerExit(Collider2D other) => OnSlicedTriggerExit?.Invoke(other);
    internal void ForwardTriggerStay(Collider2D other) => OnSlicedTriggerStay?.Invoke(other);
    internal void ForwardCollisionEnter(Collision2D other) => OnSlicedCollisionEnter?.Invoke(other);
    internal void ForwardCollisionExit(Collision2D other) => OnSlicedCollisionExit?.Invoke(other);
    internal void ForwardCollisionStay(Collision2D other) => OnSlicedCollisionStay?.Invoke(other);

    void OnDestroy()
    {
        // 清理子物体
        if (_topCollider != null) Destroy(_topCollider.gameObject);
        if (_bottomCollider != null) Destroy(_bottomCollider.gameObject);
        if (_leftCollider != null) Destroy(_leftCollider.gameObject);
        if (_rightCollider != null) Destroy(_rightCollider.gameObject);
        if (_centerCollider != null) Destroy(_centerCollider.gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_originalCollider == null) return;

        // 绘制原始碰撞体范围
        Gizmos.color = _isSliced ? Color.red : Color.green;
        Gizmos.DrawWireCube(_originalCollider.bounds.center, _originalCollider.bounds.size);
    }
#endif
}
