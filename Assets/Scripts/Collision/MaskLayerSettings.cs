using System.Collections.Generic;
using Azathrix.Framework.Settings;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// AB面全局设置
/// 统一配置材质和层级
/// </summary>
[ShowSetting]
public class MaskLayerSettings : SettingsBase<MaskLayerSettings>
{
    [Title("层级配置")]
    [LabelText("A 面层级")]
    [ValueDropdown("GetAllLayers")]
    [SerializeField]
    private int _layerA = 0;

    [LabelText("B 面层级")]
    [ValueDropdown("GetAllLayers")]
    [SerializeField]
    private int _layerB = 0;

    [Title("Sprite 材质配置")]
    [LabelText("A 面 Sprite 材质")]
    [SerializeField]
    private Material _spriteMaterialA;

    [LabelText("B 面 Sprite 材质")]
    [SerializeField]
    private Material _spriteMaterialB;

    // 公开属性
    public int LayerA => _layerA;
    public int LayerB => _layerB;
    public Material SpriteMaterialA => _spriteMaterialA;
    public Material SpriteMaterialB => _spriteMaterialB;

    /// <summary>
    /// 获取指定层面的层级
    /// </summary>
    public int GetLayer(bool isLayerA)
    {
        return isLayerA ? _layerA : _layerB;
    }

    /// <summary>
    /// 获取指定层面的 Sprite 材质
    /// </summary>
    public Material GetSpriteMaterial(bool isLayerA)
    {
        return isLayerA ? _spriteMaterialA : _spriteMaterialB;
    }

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
