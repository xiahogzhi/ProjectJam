using Azathrix.EzUI;
using Azathrix.EzUI.Core;
using Azathrix.EzUI.Interfaces;
using Azathrix.Framework.Core;
using Cysharp.Threading.Tasks;

namespace UI
{
    public class EndPanel : Panel, IMainUI, IMainUILoadable
    {
        public override bool useMask { get; } = false;

        protected override void OnScriptInitialize()
        {
            base.OnScriptInitialize();
            ToStart();
        }

        async void ToStart()
        {
            await UniTask.WaitForSeconds(3,true);
            AzathrixFramework.GetSystem<GamePlaySystem>().ExitGame();
        }

        public async UniTask OnLoading(ILoadingController controller)
        {
            await UniTask.WaitForSeconds(0.5f);
        }

        public LoadingConfig LoadingConfig { get; }
    }
}