using Azathrix.EzUI.Core;
using Azathrix.Framework.Core.Attributes;
using Azathrix.Framework.Interfaces;
using Azathrix.Framework.Tools;
using Azcel;
using Game.Tables;
using UI;
using UnityEngine.SceneManagement;

[RequireSystem(typeof(UISystem))]
[RequireSystem(typeof(AzcelSystem))]
public class GamePlaySystem : ISystem
{
    [Inject] private UISystem _uiSystem;
    [Inject] private AzcelSystem _azcelSystem;

    public void StartGame()
    {
        var id = ES3.Load<int>(SaveKeys.LevelId, 1);
        var config = _azcelSystem.GetConfig<LevelConfig>(id);
        if (config == null)
        {
            Log.Error("不存在关卡:" + id);
            return;
        }

        SceneManager.LoadScene(config.SceneName);
        _uiSystem.Show<GamePanel>();
    }
}