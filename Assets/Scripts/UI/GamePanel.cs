using Azathrix.EzUI;
using Azathrix.EzUI.Core;
using Azathrix.EzUI.Interfaces;
using Azathrix.Framework.Core;
using Azathrix.Framework.Core.Attributes;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class GamePanel : Panel, IMainUI, IMainUILoadable
    {
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _replayButton;
        [Inject] private GamePlaySystem _playSystem;

        public override bool useMask { get; } = false;

        protected override void OnScriptInitialize()
        {
            base.OnScriptInitialize();
            _exitButton?.onClick.AddListener(() => { _playSystem.ExitGame(); });
            _replayButton?.onClick.AddListener(() => { _playSystem.Replay(); });

            AzathrixFramework.Dispatcher.Subscribe((ref PlayerController.OnPlayerDead playerDead) =>
            {
                _replayButton.enabled = false;
                _exitButton.enabled = false;
            }).AddTo(this);
        }

        public async UniTask OnLoading(ILoadingController controller)
        {
            await UniTask.WaitForSeconds(1, true);
        }

        public LoadingConfig LoadingConfig { get; }
    }
}