using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 层级映射配置
/// </summary>
[Serializable]
public class LayerMapping
{
    [LabelText("源层级")]
    [ValueDropdown("GetAllLayers")]
    public int sourceLayer;

    [LabelText("切割后层级")]
    [ValueDropdown("GetAllLayers")]
    public int targetLayer;

#if UNITY_EDITOR
    private static IEnumerable<ValueDropdownItem<int>> GetAllLayers()
    {
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                yield return new ValueDropdownItem<int>(layerName, i);
            }
        }
    }
#endif
}

/// <summary>
/// 碰撞切割Mask控制器
/// 通过API控制激活状态和位置，支持多层级映射
/// </summary>
public class CollisionMask : MonoBehaviour
{
    [Title("层级配置")]
    [LabelText("层级映射")]
    [Tooltip("配置每个层级被切割后应该变成什么层级")]
    [SerializeField] private List<LayerMapping> layerMappings = new();

    [LabelText("目标碰撞层")]
    [Tooltip("需要检测的所有层级（会自动根据layerMappings生成）")]
    [SerializeField, ReadOnly] private LayerMask targetLayer;

    [Title("调试")]
    [LabelText("当前激活状态")]
    [SerializeField, ReadOnly] private bool _isActive = false;

    private BoxCollider2D _maskCollider;
    private Dictionary<int, int> _layerMappingDict = new();

    // 追踪所有被切割的碰撞体，用于在mask移动或取消时恢复
    private HashSet<SliceableCollider> _currentlyAffected = new();
    private HashSet<SliceableCollider> _previouslyAffected = new();

    // 标记是否已经恢复过，避免重复调用
    private bool _hasRestored = false;

    // 预分配列表，避免GC
    private List<Collider2D> _colliderBuffer = new();

    public bool IsActive => _isActive;
    public Bounds MaskBounds => _maskCollider.bounds;
    public BoxCollider2D MaskCollider => _maskCollider;

    void Awake()
    {
        _maskCollider = GetComponent<BoxCollider2D>();
        if (_maskCollider == null)
        {
            _maskCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        BuildLayerMappingDict();
    }

    void OnValidate()
    {
        BuildLayerMappingDict();
        UpdateTargetLayerMask();
    }

    void BuildLayerMappingDict()
    {
        _layerMappingDict.Clear();
        foreach (var mapping in layerMappings)
        {
            _layerMappingDict[mapping.sourceLayer] = mapping.targetLayer;
        }
    }

    void UpdateTargetLayerMask()
    {
        int mask = 0;
        foreach (var mapping in layerMappings)
        {
            mask |= (1 << mapping.sourceLayer);
        }
        targetLayer = mask;
    }

    /// <summary>
    /// 获取指定层级切割后的目标层级
    /// </summary>
    public int GetTargetLayer(int sourceLayer)
    {
        if (_layerMappingDict.TryGetValue(sourceLayer, out int target))
        {
            return target;
        }
        return sourceLayer; // 没有配置则保持原层级
    }

    /// <summary>
    /// 运行时添加层级映射
    /// </summary>
    public void AddLayerMapping(int sourceLayer, int targetLayer)
    {
        _layerMappingDict[sourceLayer] = targetLayer;

        // 更新targetLayer mask
        this.targetLayer |= (1 << sourceLayer);
    }

    /// <summary>
    /// 运行时移除层级映射
    /// </summary>
    public void RemoveLayerMapping(int sourceLayer)
    {
        _layerMappingDict.Remove(sourceLayer);

        // 更新targetLayer mask
        int mask = 0;
        foreach (var key in _layerMappingDict.Keys)
        {
            mask |= (1 << key);
        }
        targetLayer = mask;
    }

    void FixedUpdate()
    {
        if (!_isActive)
        {
            // 未激活时，恢复所有被切割的碰撞体（只执行一次）
            if (!_hasRestored)
            {
                RestoreAllColliders();
                _hasRestored = true;
            }
            return;
        }

        _hasRestored = false;

        Bounds maskBounds = _maskCollider.bounds;

        // 先处理已经被切割的碰撞体（直接检查bounds重叠，不依赖OverlapBox）
        foreach (var sliceable in _previouslyAffected)
        {
            if (sliceable == null) continue;

            // 使用缓存的bounds检查（即使collider被禁用也能正确获取）
            Bounds originalBounds = sliceable.OriginalBounds;
            if (maskBounds.Intersects(originalBounds))
            {
                // 仍然重叠，继续切割
                int targetLayerForSlice = GetTargetLayer(sliceable.OriginalLayer);
                sliceable.ApplySlice(maskBounds, targetLayerForSlice);
                _currentlyAffected.Add(sliceable);
            }
            else
            {
                // 不再重叠，恢复
                sliceable.RestoreCollider();
            }
        }

        // 检测新进入mask区域的碰撞体
        _colliderBuffer.Clear();
        Physics2D.OverlapBox(
            maskBounds.center,
            maskBounds.size,
            0f,
            new ContactFilter2D { layerMask = targetLayer, useLayerMask = true, useTriggers = true },
            _colliderBuffer
        );

        foreach (var col in _colliderBuffer)
        {
            if (col == _maskCollider) continue;

            // 查找SliceableCollider组件
            var sliceable = col.GetComponentInParent<SliceableCollider>();
            if (sliceable == null) continue;

            // 跳过已经处理过的
            if (_currentlyAffected.Contains(sliceable)) continue;

            // 获取目标层级
            int targetLayerForSlice = GetTargetLayer(sliceable.OriginalLayer);

            // 应用切割，传入目标层级
            sliceable.ApplySlice(maskBounds, targetLayerForSlice);
            _currentlyAffected.Add(sliceable);
        }

        // 交换集合引用，为下一帧准备
        (_currentlyAffected, _previouslyAffected) = (_previouslyAffected, _currentlyAffected);
        _currentlyAffected.Clear();
    }

    void RestoreAllColliders()
    {
        foreach (var sliceable in _currentlyAffected)
        {
            sliceable.RestoreCollider();
        }
        _currentlyAffected.Clear();

        foreach (var sliceable in _previouslyAffected)
        {
            sliceable.RestoreCollider();
        }
        _previouslyAffected.Clear();
    }

    /// <summary>
    /// 手动设置激活状态
    /// </summary>
    public void SetActive(bool active)
    {
        _isActive = active;
    }

    /// <summary>
    /// 手动设置位置
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_maskCollider == null)
            _maskCollider = GetComponent<BoxCollider2D>();

        if (_maskCollider == null) return;

        Gizmos.color = _isActive ? new Color(1f, 0f, 0f, 0.5f) : new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(_maskCollider.bounds.center, _maskCollider.bounds.size);

        Gizmos.color = _isActive ? Color.red : Color.yellow;
        Gizmos.DrawWireCube(_maskCollider.bounds.center, _maskCollider.bounds.size);
    }
#endif
}
