using Azathrix.EzUI.Core;
using Azathrix.Framework.Core.Attributes;
using Azathrix.Framework.Interfaces;
using Azathrix.Framework.Tools;
using Azcel;
using Cysharp.Threading.Tasks;
using Framework.Games;
using Game.Tables;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireSystem(typeof(UISystem))]
[RequireSystem(typeof(AzcelSystem))]
public class GamePlaySystem : ISystem
{
    [Inject] private UISystem _uiSystem;
    [Inject] private AzcelSystem _azcelSystem;

    private LevelConfig _currentLevel;

    public LevelConfig currentLevel => _currentLevel;

    public async void StartGame()
    {
        if (_loading)
            return;
        Time.timeScale = 0;
        _loading = true;
        var id = ES3.Load<int>(SaveKeys.LevelId, 1);
        var config = _azcelSystem.GetConfig<LevelConfig>(id);
        if (config == null)
        {
            Log.Error("不存在关卡:" + id);
            Time.timeScale = 1;
            _loading = true;
            return;
        }

        Log.Info(LoadingPanel.Instance);
        _currentLevel = config;
        Log.Info(_currentLevel);
        await LoadingPanel.Instance.ShowAsync();
        await _uiSystem.ShowAsync<GamePanel>();
        await SceneManager.LoadSceneAsync(_currentLevel.SceneName).ToUniTask();
        await UniTask.WaitForSeconds(0.2f, true);
        await LoadingPanel.Instance.HideAsync();
        Time.timeScale = 1;
        _loading = false;
    }

    public async void EndGame()
    {
        if (_loading)
            return;
        Time.timeScale = 0;
        _loading = true;
        Log.Info(LoadingPanel.Instance);
        Log.Info(_currentLevel.SceneName);
        await LoadingPanel.Instance.ShowAsync();
        await _uiSystem.ShowAsync<EndPanel>();
        await SceneManager.LoadSceneAsync("Empty").ToUniTask();
        await UniTask.WaitForSeconds(0.2f, true);
        await LoadingPanel.Instance.HideAsync();

        Time.timeScale = 1;
        _loading = false;
    }

    public async void ExitGame()
    {
        if (_loading)
            return;
        Time.timeScale = 0;
        _loading = true;
        Log.Info(LoadingPanel.Instance);
        Log.Info(_currentLevel.SceneName);
        await LoadingPanel.Instance.ShowAsync();
        await _uiSystem.ShowAsync<StartPanel>();
        await SceneManager.LoadSceneAsync("Empty").ToUniTask();
        await UniTask.WaitForSeconds(0.2f, true);
        await LoadingPanel.Instance.HideAsync();

        Time.timeScale = 1;
        _loading = false;
    }

    public void FocusCamera(Transform target)
    {
        SystemEnvironment.instance.systemConfig.mainCamera.Follow = target;
    }

    private bool _loading;

    public async void Replay()
    {
        if (_loading)
            return;
        Time.timeScale = 0;
        _loading = true;
        var nextId = _currentLevel.Id;
        var config = _azcelSystem.GetConfig<LevelConfig>(nextId);
        if (config == null)
        {
            Log.Error("不存在关卡:" + nextId);
            Time.timeScale = 1;
            _loading = false;
            return;
        }

        _currentLevel = config;
        ES3.Save(SaveKeys.LevelId, nextId);


        Log.Info(LoadingPanel.Instance);
        Log.Info(_currentLevel);
        await LoadingPanel.Instance.ShowAsync();
        // await _uiSystem.ShowAsync<GamePanel>();
        await SceneManager.LoadSceneAsync("Empty").ToUniTask();
        await SceneManager.LoadSceneAsync(_currentLevel.SceneName).ToUniTask();
        await UniTask.WaitForSeconds(0.2f, true);
        await LoadingPanel.Instance.HideAsync();

        Time.timeScale = 1;
        _loading = false;
    }

    public async void GotoLevel(int nextLevel)
    {
        if (_loading)
            return;
        Time.timeScale = 0;
        _loading = true;
        var nextId = nextLevel;
        var config = _azcelSystem.GetConfig<LevelConfig>(nextId);
        if (config == null)
        {
            Log.Error("不存在关卡:" + nextId);
            Time.timeScale = 1;
            _loading = false;
            return;
        }

        _currentLevel = config;
        ES3.Save(SaveKeys.LevelId, nextId);


        Log.Info(LoadingPanel.Instance);
        Log.Info(_currentLevel);
        await LoadingPanel.Instance.ShowAsync();
        await _uiSystem.ShowAsync<GamePanel>();
        await SceneManager.LoadSceneAsync("Empty").ToUniTask();
        await SceneManager.LoadSceneAsync(_currentLevel.SceneName).ToUniTask();
        await UniTask.WaitForSeconds(0.2f, true);
        await LoadingPanel.Instance.HideAsync();

        Time.timeScale = 1;
        _loading = false;
    }

    public async void NextLevel()
    {
        if (_loading)
            return;
        Time.timeScale = 0;
        _loading = true;
        var nextId = _currentLevel.NextLevel;
        var config = _azcelSystem.GetConfig<LevelConfig>(nextId);
        if (config == null)
        {
            Log.Error("结束游玩:" + nextId);
            Time.timeScale = 1;
            _loading = false;

            EndGame();
            return;
        }

        _currentLevel = config;
        ES3.Save(SaveKeys.LevelId, nextId);


        Log.Info(LoadingPanel.Instance);
        Log.Info(_currentLevel);
        await LoadingPanel.Instance.ShowAsync();
        // await _uiSystem.ShowAsync<GamePanel>();
        await SceneManager.LoadSceneAsync(_currentLevel.SceneName).ToUniTask();
        await UniTask.WaitForSeconds(0.2f, true);
        await LoadingPanel.Instance.HideAsync();

        Time.timeScale = 1;
        _loading = false;
    }
}