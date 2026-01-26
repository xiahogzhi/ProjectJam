using Azathrix.EzUI.Core;
using Azathrix.Framework.Core.Attributes;
using Azathrix.Framework.Interfaces;
using Azathrix.Framework.Interfaces.SystemEvents;
using Cysharp.Threading.Tasks;
using Framework.Games;
using UnityEngine;

[RequireSystem(typeof(UISystem))]
public class GameLauncher : ISystem, ISystemInitialize
{
    [Inject] private UISystem _system;

    public UniTask OnInitializeAsync()
    {

        return UniTask.CompletedTask;
    }
}