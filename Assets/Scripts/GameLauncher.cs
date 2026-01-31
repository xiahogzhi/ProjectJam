using Azathrix.EzInput.Core;
using Azathrix.EzInput.Events;
using Azathrix.EzUI.Core;
using Azathrix.EzUI.Interfaces;
using Azathrix.Framework.Core;
using Azathrix.Framework.Core.Attributes;
using Azathrix.Framework.Interfaces;
using Azathrix.Framework.Interfaces.SystemEvents;
using Azathrix.GameKit.Runtime.Builder.PrefabBuilders;
using Azathrix.GameKit.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using ExcelDataReader.Log;
using Framework.Games;
using SoundSystems;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Log = Azathrix.Framework.Tools.Log;

[RequireSystem(typeof(UISystem))]
[RequireSystem(typeof(SoundSystem))]
[RequireSystem(typeof(EzInputSystem))]
public class GameLauncher : ISystem, ISystemInitialize, IUILoadingHandler
{
    [Inject] private UISystem _system;
    [Inject] private EzInputSystem _ezInputSystem;
    [Inject] private SoundSystem _soundSystem;

    private LoadingPanel _loadingPanel;

    public async UniTask<ILoadingController> ShowLoading(LoadingConfig config)
    {
        Log.Info("开始显示Loading");
        var t = await _loadingPanel.ShowAsync();
        Log.Info("结束显示Loading");
        return t;
    }

    public async UniTask HideLoading()
    {
        Log.Info("开始隐藏Loading");
        await _loadingPanel.HideAsync();
        Log.Info("结束隐藏Loading");
    }

    public UniTask OnInitializeAsync()
    {
        var root = GameObject.FindWithTag("Root");
        Object.DontDestroyOnLoad(root);
        _system.SetUIRoot(GameObject.FindWithTag("UIRoot").transform);
        _system.SetUICamera(GameObject.FindWithTag("UICamera").GetComponent<Camera>());
        _system.SetEventSystem(GameObject.FindWithTag("EventSystem").GetComponent<EventSystem>());

        WaitForStart().Forget();

        AzathrixFramework.Dispatcher.Subscribe((ref InputMapChangedEvent evt) =>
        {
            Log.Info("设置Map:" + evt.CurrentMap);
        });

        return UniTask.CompletedTask;
    }

    async UniTask WaitForStart()
    {
        await UniTask.WaitUntil(() => AzathrixFramework.IsStarted);
        // await UniTask.Yield();
        // _ezInputSystem.PlayerInput.enabled = false;
        // await UniTask.Yield();
        // _ezInputSystem.PlayerInput.enabled = true;
        // var loading = _system.LoadPersistenceUI<LoadingPanel>();
        _loadingPanel = PrefabBuilder.Get().SetPrefab("UI/LoadingPanel".LoadPrefab()).SetDefaultActive(false)
            .SetParent(SystemEnvironment.instance.systemConfig.uiRoot).Build().GetComponent<LoadingPanel>();
        // _system.SetLoadingHandler(this);
        LoadingPanel.Instance = _loadingPanel;
        _system.Show<StartPanel>();
        _soundSystem.PlayBackground("event:/背景音乐/Main_BGM");
    }
}