#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AB面设置工具
/// 用于快速设置对象的材质、层级和粒子参数
/// </summary>
public class MaskLayerSetupTool : OdinEditorWindow
{
    [MenuItem("Tools/Mask Layer Setup Tool")]
    static void OpenWindow()
    {
        GetWindow<MaskLayerSetupTool>("AB面设置工具").Show();
    }

    public enum MaskLayerType
    {
        [LabelText("A 面")]
        LayerA,
        [LabelText("B 面")]
        LayerB
    }

    [Title("目标设置")]
    [LabelText("目标对象")]
    [Tooltip("要设置的对象，可以多选")]
    [SerializeField]
    private List<GameObject> targetObjects = new();

    [Button("从选中对象获取", ButtonSizes.Medium)]
    void GetFromSelection()
    {
        targetObjects.Clear();
        targetObjects.AddRange(Selection.gameObjects);
    }

    [Title("层级设置")]
    [LabelText("设置为")]
    [SerializeField]
    private MaskLayerType layerType = MaskLayerType.LayerA;

    [Title("粒子设置")]
    [LabelText("设置粒子材质 Keyword")]
    [Tooltip("启用后会设置粒子材质的 CUSTOMMASKLAYER_A/B keyword")]
    [SerializeField]
    private bool setParticleKeyword = true;

    [Title("操作")]
    [Button("应用设置", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
    void ApplySettings()
    {
        if (targetObjects == null || targetObjects.Count == 0)
        {
            EditorUtility.DisplayDialog("错误", "请先选择目标对象", "确定");
            return;
        }

        int processedCount = 0;
        foreach (var obj in targetObjects)
        {
            if (obj == null) continue;
            ProcessGameObject(obj);
            processedCount++;
        }

        EditorUtility.DisplayDialog("完成", $"已处理 {processedCount} 个对象", "确定");
    }

    [Button("应用到选中对象", ButtonSizes.Large), GUIColor(0.4f, 0.6f, 0.8f)]
    void ApplyToSelection()
    {
        var selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "请先在场景中选择对象", "确定");
            return;
        }

        int processedCount = 0;
        foreach (var obj in selectedObjects)
        {
            if (obj == null) continue;
            ProcessGameObject(obj);
            processedCount++;
        }

        EditorUtility.DisplayDialog("完成", $"已处理 {processedCount} 个对象", "确定");
    }

    [Button("打开全局设置", ButtonSizes.Medium)]
    void OpenSettings()
    {
        var settings = MaskLayerSettings.Instance;
        if (settings != null)
        {
            Selection.activeObject = settings;
        }
    }

    void ProcessGameObject(GameObject obj)
    {
        var settings = MaskLayerSettings.Instance;
        if (settings == null)
        {
            Debug.LogError("MaskLayerSettings not found!");
            return;
        }

        Undo.RecordObject(obj, "Mask Layer Setup");

        bool isLayerA = layerType == MaskLayerType.LayerA;
        int targetLayer = settings.GetLayer(isLayerA);
        Material targetMaterial = settings.GetSpriteMaterial(isLayerA);

        // 设置层级
        SetLayerRecursively(obj, targetLayer);

        // 设置 SpriteRenderer 材质
        var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in spriteRenderers)
        {
            if (targetMaterial != null)
            {
                Undo.RecordObject(sr, "Set Material");
                sr.sharedMaterial = targetMaterial;
                EditorUtility.SetDirty(sr);
            }
        }

        // 设置粒子系统的材质 keyword
        if (setParticleKeyword)
        {
            var particleSystems = obj.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particleSystems)
            {
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    Material mat = renderer.sharedMaterial;
                    Undo.RecordObject(mat, "Set Particle Keyword");

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

                    EditorUtility.SetDirty(mat);
                    EditorUtility.SetDirty(renderer);
                }
            }
        }

        // 设置 SliceableCollider（如果有的话）
        var sliceableColliders = obj.GetComponentsInChildren<SliceableCollider>(true);
        foreach (var sc in sliceableColliders)
        {
            EditorUtility.SetDirty(sc);
        }

        EditorUtility.SetDirty(obj);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
#endif
