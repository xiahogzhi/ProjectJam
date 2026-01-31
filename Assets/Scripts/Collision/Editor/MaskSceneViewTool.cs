#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Scene 视图中的 Mask 效果控制 Overlay
/// </summary>
[Overlay(typeof(SceneView), "Mask Layer Control", true)]
public class MaskLayerOverlay : Overlay, ICreateToolbar
{
    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement();
        root.style.flexDirection = FlexDirection.Column;
        root.style.alignItems = Align.FlexStart;
        root.style.paddingLeft = 5;
        root.style.paddingRight = 5;
        root.style.paddingTop = 3;
        root.style.paddingBottom = 3;

        var label = new Label("Mask 层级");
        label.style.marginBottom = 3;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        root.Add(label);

        // 创建 Toggle 组
        var toggleGroup = new RadioButtonGroup();
        toggleGroup.choices = new[] { "A层", "B层", "Both" };

        // 根据当前设置选择初始值
        if (MaskSceneViewSettings.ShowLayerA && MaskSceneViewSettings.ShowLayerB)
            toggleGroup.value = 2; // Both
        else if (MaskSceneViewSettings.ShowLayerA)
            toggleGroup.value = 0; // A
        else if (MaskSceneViewSettings.ShowLayerB)
            toggleGroup.value = 1; // B
        else
            toggleGroup.value = 2; // Both

        toggleGroup.RegisterValueChangedCallback(evt =>
        {
            switch (evt.newValue)
            {
                case 0: // A层
                    MaskSceneViewSettings.ShowLayerA = true;
                    MaskSceneViewSettings.ShowLayerB = false;
                    break;
                case 1: // B层
                    MaskSceneViewSettings.ShowLayerA = false;
                    MaskSceneViewSettings.ShowLayerB = true;
                    break;
                case 2: // Both
                    MaskSceneViewSettings.ShowLayerA = true;
                    MaskSceneViewSettings.ShowLayerB = true;
                    break;
            }
            MaskSceneViewSettings.UpdateShader();
        });

        root.Add(toggleGroup);

        return root;
    }

    public IEnumerable<string> toolbarElements
    {
        get
        {
            yield return MaskLayerAToggle.Id;
            yield return MaskLayerBToggle.Id;
        }
    }
}

[EditorToolbarElement(Id, typeof(SceneView))]
public class MaskLayerAToggle : EditorToolbarToggle
{
    public const string Id = "MaskLayerControl/LayerA";

    public MaskLayerAToggle()
    {
        text = "A层";
        tooltip = "显示/隐藏 A 层";
        value = MaskSceneViewSettings.ShowLayerA;

        this.RegisterValueChangedCallback(evt =>
        {
            MaskSceneViewSettings.ShowLayerA = evt.newValue;
            MaskSceneViewSettings.UpdateShader();
        });
    }
}

[EditorToolbarElement(Id, typeof(SceneView))]
public class MaskLayerBToggle : EditorToolbarToggle
{
    public const string Id = "MaskLayerControl/LayerB";

    public MaskLayerBToggle()
    {
        text = "B层";
        tooltip = "显示/隐藏 B 层";
        value = MaskSceneViewSettings.ShowLayerB;

        this.RegisterValueChangedCallback(evt =>
        {
            MaskSceneViewSettings.ShowLayerB = evt.newValue;
            MaskSceneViewSettings.UpdateShader();
        });
    }
}

/// <summary>
/// Mask 场景视图设置（静态类管理状态）
/// </summary>
[InitializeOnLoad]
public static class MaskSceneViewSettings
{
    private static readonly int GlobalMaskEnabled = Shader.PropertyToID("_GlobalMaskEnabled");
    private static readonly int GlobalMaskShowLayerA = Shader.PropertyToID("_GlobalMaskShowLayerA");
    private static readonly int GlobalMaskShowLayerB = Shader.PropertyToID("_GlobalMaskShowLayerB");

    public static bool ShowLayerA
    {
        get => EditorPrefs.GetBool("MaskTool_ShowLayerA", true);
        set => EditorPrefs.SetBool("MaskTool_ShowLayerA", value);
    }

    public static bool ShowLayerB
    {
        get => EditorPrefs.GetBool("MaskTool_ShowLayerB", true);
        set => EditorPrefs.SetBool("MaskTool_ShowLayerB", value);
    }

    static MaskSceneViewSettings()
    {
        // 编辑器启动时初始化
        EditorApplication.delayCall += () =>
        {
            if (!Application.isPlaying)
            {
                UpdateShader();
            }
        };

        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // 进入播放模式，启用 mask 效果
            Shader.SetGlobalFloat(GlobalMaskEnabled, 1f);
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // 退出播放模式，恢复编辑器设置
            UpdateShader();
        }
    }

    public static void UpdateShader()
    {
        // 编辑器模式下，禁用 mask 效果但通过 ShowLayerA/B 控制显隐
        Shader.SetGlobalFloat(GlobalMaskEnabled, 0f);
        Shader.SetGlobalFloat(GlobalMaskShowLayerA, ShowLayerA ? 1f : 0f);
        Shader.SetGlobalFloat(GlobalMaskShowLayerB, ShowLayerB ? 1f : 0f);

        SceneView.RepaintAll();
    }
}
#endif
