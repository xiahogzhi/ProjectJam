using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Mask 全局管理器
/// 将 mask 参数设置到 Shader 全局变量，所有使用 MaskLayerSprite Shader 的材质都会受影响
/// 直接挂在 CollisionMask 同一物体上即可
/// </summary>
[RequireComponent(typeof(CollisionMask))]
public class MaskGlobalManager : MonoBehaviour
{
    [Title("效果参数")]
    [LabelText("启用Mask效果")]
    [SerializeField] private bool _enableMaskEffect = true;

    [LabelText("预览时B层透明度")]
    [Range(0f, 1f)]
    [SerializeField] private float _previewAlpha = 0.5f;

    [LabelText("激活时B层透明度")]
    [Range(0f, 1f)]
    [SerializeField] private float _activeAlpha = 1f;

    private CollisionMask _collisionMask;

    // 公开属性供 Editor 访问
    public bool EnableMaskEffect
    {
        get => _enableMaskEffect;
        set => _enableMaskEffect = value;
    }

    public bool IsActive => _collisionMask != null && _collisionMask.IsActive;

    // Shader 全局属性名
    private static readonly int GlobalMaskEnabled = Shader.PropertyToID("_GlobalMaskEnabled");
    private static readonly int GlobalMaskCenter = Shader.PropertyToID("_GlobalMaskCenter");
    private static readonly int GlobalMaskSize = Shader.PropertyToID("_GlobalMaskSize");
    private static readonly int GlobalMaskActive = Shader.PropertyToID("_GlobalMaskActive");
    private static readonly int GlobalMaskPreviewAlpha = Shader.PropertyToID("_GlobalMaskPreviewAlpha");
    private static readonly int GlobalMaskActiveAlpha = Shader.PropertyToID("_GlobalMaskActiveAlpha");

    void Awake()
    {
        _collisionMask = GetComponent<CollisionMask>();

        // 运行时自动启用 mask 效果
        _enableMaskEffect = true;
    }

    void Start()
    {
        // 初始化全局参数
        UpdateGlobalShaderParams();
    }

    void Update()
    {
        UpdateGlobalShaderParams();
    }

    void UpdateGlobalShaderParams()
    {
        // 全局开关
        Shader.SetGlobalFloat(GlobalMaskEnabled, _enableMaskEffect ? 1f : 0f);

        if (_collisionMask == null || !_enableMaskEffect)
        {
            // 没有 mask 或禁用效果
            Shader.SetGlobalVector(GlobalMaskCenter, Vector2.zero);
            Shader.SetGlobalVector(GlobalMaskSize, Vector2.zero);
            Shader.SetGlobalFloat(GlobalMaskActive, 0f);
        }
        else
        {
            Bounds maskBounds = _collisionMask.MaskBounds;
            Shader.SetGlobalVector(GlobalMaskCenter, (Vector2)maskBounds.center);
            Shader.SetGlobalVector(GlobalMaskSize, (Vector2)maskBounds.size);
            Shader.SetGlobalFloat(GlobalMaskActive, _collisionMask.IsActive ? 1f : 0f);
        }

        Shader.SetGlobalFloat(GlobalMaskPreviewAlpha, _previewAlpha);
        Shader.SetGlobalFloat(GlobalMaskActiveAlpha, _activeAlpha);
    }

    /// <summary>
    /// 启用/禁用 Mask 效果
    /// </summary>
    public void SetMaskEffectEnabled(bool enabled)
    {
        _enableMaskEffect = enabled;
    }

    /// <summary>
    /// 设置预览透明度
    /// </summary>
    public void SetPreviewAlpha(float alpha)
    {
        _previewAlpha = Mathf.Clamp01(alpha);
    }

    /// <summary>
    /// 设置激活透明度
    /// </summary>
    public void SetActiveAlpha(float alpha)
    {
        _activeAlpha = Mathf.Clamp01(alpha);
    }
}
