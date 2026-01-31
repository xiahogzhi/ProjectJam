// using System;
// using System.Collections.Generic;
// using FMODUnity;
// using Framework.Misc.Interfaces;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using Utilities;
//
// namespace Framework.Games.Systems.SoundSystems
// {
//     [Name("音效材质")]
//     [CreateAssetMenu(menuName = "配置/音效材质")]
//     public class SoundMaterial : ScriptableObject,IAssetInfo
//     {
//         [LabelText("材质名")] [SerializeField] private string _materialName = "新材质";
//
//         [LabelText("主音效")] [SerializeField] private EventReference _mainSound;
//
//         [LabelText("匹配配置")] [SerializeField] private List<MatchConfig> _configs = new List<MatchConfig>();
//
//         [LabelText("优先级")] [SerializeField] private int _priority;
//
//         [LabelText("叠加当前音效")] [SerializeField] private bool _overlap;
//
//         public int priority => _priority;
//
//         public bool overlap => _overlap;
//
//         [Serializable]
//         public class MatchConfig
//         {
//             [LabelText("目标材质")] [SerializeField] private SoundMaterial _targetMaterial;
//
//             [LabelText("匹配音效")] [SerializeField] private EventReference _targetSound;
//
//             public SoundMaterial targetMaterial
//             {
//                 get => _targetMaterial;
//                 set => _targetMaterial = value;
//             }
//
//             public EventReference targetSound
//             {
//                 get => _targetSound;
//                 set => _targetSound = value;
//             }
//         }
//
//         // public string AssetDisplayName => _materialName;
//
//         public EventReference mainSound => _mainSound;
//
//         public EventReference MatchSound(SoundMaterial sm)
//         {
//             if (sm == null || sm.mainSound.IsNull)
//                 return _mainSound;
//
//
//             foreach (var variable in _configs)
//             {
//                 if (variable.targetMaterial == sm)
//                     return variable.targetSound;
//             }
//
//             if (sm._priority < _priority)
//                 return _mainSound;
//
//             return sm._mainSound;
//         }
//
//         public string assetName => _materialName;
//         public Sprite assetIcon { get; }
//     }
// }