using Azathrix.EzUI;
using Azathrix.EzUI.Core;
using Azathrix.Framework.Core.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StartPanel : FocusUI, IMainUI
    {
        [SerializeField] private Button _startButton;

        [Inject] private GamePlaySystem _playSystem;

        protected override void OnScriptInitialize()
        {
            base.OnScriptInitialize();
            _startButton?.onClick.AddListener(() =>
                {
                    _playSystem.StartGame();
                }
            );
        }
    }
}