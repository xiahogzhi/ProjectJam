using Azathrix.EzUI;
using Azathrix.EzUI.Core;
using Azathrix.EzUI.Interfaces;
using Azathrix.Framework.Core.Attributes;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StartPanel : FocusUI, IMainUI,IMainUILoadable
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

        public async UniTask OnLoading(ILoadingController controller)
        {
            await UniTask.WaitForSeconds(1, true);
            controller.SetProgress(1);
        }

        public LoadingConfig LoadingConfig { get; } = new LoadingConfig();
    }
}