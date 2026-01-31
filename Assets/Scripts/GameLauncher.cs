using Azathrix.EzInput.Core;
using Azathrix.EzInput.Events;
using Azathrix.EzUI.Core;
using Azathrix.Framework.Core;
using Azathrix.Framework.Core.Attributes;
using Azathrix.Framework.Interfaces;
using Azathrix.Framework.Interfaces.SystemEvents;
using Cysharp.Threading.Tasks;
using ExcelDataReader.Log;
using SoundSystems;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Log = Azathrix.Framework.Tools.Log;

[RequireSystem(typeof(UISystem))]
[RequireSystem(typeof(SoundSystem))]
[RequireSystem(typeof(EzInputSystem))]
public class GameLauncher : ISystem, ISystemInitialize
{
    [Inject] private UISystem _system;
    [Inject] private EzInputSystem _ezInputSystem;
    [Inject] private SoundSystem _soundSystem;

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
        _system.Show<StartPanel>();
        _soundSystem.PlayBackground("event:/背景音乐/Main_BGM");
    }
}