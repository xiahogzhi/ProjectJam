using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// AB面设置组件
/// 挂在对象上，用于快速切换 A/B 面的材质、层级和粒子参数
/// </summary>
public class MaskLayerSetter : MonoBehaviour
{
    public enum MaskLayerType
    {
        [LabelText("A 面")] LayerA,
        [LabelText("B 面")] LayerB
    }

    [LabelText("当前层面")] [OnValueChanged("OnLayerTypeChanged")] [SerializeField]
    private MaskLayerType _currentLayer = MaskLayerType.LayerA;

    [LabelText("设置粒子 Keyword")] [SerializeField]
    private bool _setParticleKeyword = true;

    [LabelText("包含子对象")] [SerializeField] private bool _includeChildren = true;

    // 缓存的组件引用
    private SpriteRenderer[] _spriteRenderers;
    private ParticleSystemRenderer[] _particleRenderers;

    public MaskLayerType CurrentLayer => _currentLayer;

    [OnInspectorInit]
    void OnInspectorInitialize()
    {
        OnLayerTypeChanged();
    }

    void Awake()
    {
        CacheComponents();
    }

    void CacheComponents()
    {
        if (_includeChildren)
        {
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            _particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>(true);
        }
        else
        {
            var sr = GetComponent<SpriteRenderer>();
            _spriteRenderers = sr != null ? new[] {sr} : Array.Empty<SpriteRenderer>();

            var pr = GetComponent<ParticleSystemRenderer>();
            _particleRenderers = pr != null ? new[] {pr} : Array.Empty<ParticleSystemRenderer>();
        }
    }

    public void SetToLayerA()
    {
        _currentLayer = MaskLayerType.LayerA;
        ApplyCurrentSettings();
    }

    public void SetToLayerB()
    {
        _currentLayer = MaskLayerType.LayerB;
        ApplyCurrentSettings();
    }

    public void ApplyCurrentSettings()
    {
        var settings = MaskLayerSettings.Instance;
        if (settings == null)
        {
            Debug.LogError("MaskLayerSettings not found!");
            return;
        }

        bool isLayerA = _currentLayer == MaskLayerType.LayerA;

        // 从全局设置获取层级和材质
        int targetLayer = settings.GetLayer(isLayerA);
        Material targetMaterial = settings.GetSpriteMaterial(isLayerA);

        // 设置层级
        if (_includeChildren)
        {
            SetLayerRecursively(gameObject, targetLayer);
        }
        else
        {
            gameObject.layer = targetLayer;
        }

        // 确保组件已缓存
        if (_spriteRenderers == null || _particleRenderers == null)
        {
            CacheComponents();
        }

        // 设置 SpriteRenderer 材质
        if (targetMaterial != null)
        {
            foreach (var sr in _spriteRenderers)
            {
                if (sr != null)
                {
                    sr.sharedMaterial = targetMaterial;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(sr);
#endif
                }
            }
        }

        // 设置粒子材质的 keyword
        if (_setParticleKeyword)
        {
            foreach (var pr in _particleRenderers)
            {
                if (pr != null && pr.sharedMaterial != null)
                {
                    Material mat = pr.sharedMaterial;

                    // 启用 Custom Mask Layer
                    mat.EnableKeyword("CUSTOMMASKLAYER_ON");

                    if (isLayerA)
                    {
                        mat.EnableKeyword("CUSTOMMASKLAYER_A");
                        mat.DisableKeyword("CUSTOMMASKLAYER_B");
                    }
                    else
                    {
                        mat.EnableKeyword("CUSTOMMASKLAYER_B");
                        mat.DisableKeyword("CUSTOMMASKLAYER_A");
                    }

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(mat);
                    UnityEditor.EditorUtility.SetDirty(pr);
#endif
                }
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void OnLayerTypeChanged()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            CacheComponents();
            ApplyCurrentSettings();
        }
#endif
    }
}