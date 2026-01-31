using Azathrix.EzUI.Core;
using Azathrix.Framework.Core.Attributes;
using Azathrix.Framework.Interfaces;
using UI;
using UnityEngine.SceneManagement;

[RequireSystem(typeof(UISystem))]
public class GamePlaySystem : ISystem
{
    [Inject] private UISystem _uiSystem;

    public void StartGame()
    {
        var id = ES3.Load<int>(SaveKeys.LevelId, 1);
        _uiSystem.Show<GamePanel>();
    }
}