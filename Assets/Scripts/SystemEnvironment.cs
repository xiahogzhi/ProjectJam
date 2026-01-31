using System;
using Framework.Misc.Interfaces;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Games
{
    /// <summary>
    /// 启动程序环境
    /// </summary>
    public class SystemEnvironment : MonoBehaviour
    {
        public const SystemLanguage DefaultLanguage = SystemLanguage.English;

        public const SystemLanguage EditorLanguage = SystemLanguage.ChineseSimplified;


        private static SystemEnvironment _instance;

        public static SystemEnvironment instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<SystemEnvironment>();

                return _instance;
            }
        }


        [Serializable]
        public class SystemConfig : IBoxInlineGUI
        {
            [LabelText("UI摄像机")] [SerializeField] private Camera _uiCamera;
            [LabelText("游戏摄像机")] [SerializeField] private Camera _gameCamera;
            [LabelText("UIRoot")] [SerializeField] private Transform _uiRoot;

            [SerializeField] private Canvas _canvas;
            [LabelText("主摄像机")] [SerializeField] private CinemachineCamera _mainCamera;

            #region 属性

            public Camera uiCamera => _uiCamera;

            public Camera gameCamera => _gameCamera;

            public Transform uiRoot => _uiRoot;

            public Canvas canvas => _canvas;

            public CinemachineCamera mainCamera => _mainCamera;


            #endregion
        }

        [LabelText("系统配置")] [SerializeField] private SystemConfig _systemConfig;

        public SystemConfig systemConfig => _systemConfig;
    }
}