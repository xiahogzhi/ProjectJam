using Azathrix.Framework.Core;
using Azathrix.Framework.Tools;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    /// <summary>当前关卡的场景名称（来自 LevelConfig，每次关卡加载时自动更新）</summary>
    public string LevelSceneName { get; private set; }

    [SerializeField] private TextMeshProUGUI dynamicTitle;

    private void Awake()
    {
        // 确保在 Awake 中订阅事件，这样即使对象在场景切换时保持启用，也能收到事件
        // 订阅关卡加载完成事件，当关卡更新时自动更新场景名称
        AzathrixFramework.Dispatcher.Subscribe((ref GamePlaySystem.OnLevelLoaded evt) =>
        {
            if (evt.levelConfig != null)
            {
                LevelSceneName = evt.levelConfig.SceneName;
                Log.Info($"[Tile] 收到关卡加载事件，场景名: {LevelSceneName}");
                UpdateTitleText();
            }
        }).AddTo(this);
    }

    private void Start()
    {
        // 初始化时也读取一次（如果已经有关卡加载）
        ReadLevelSceneName();
    }

    /// <summary>读取当前关卡的场景名称，可在关卡加载后任意时刻调用</summary>
    public void ReadLevelSceneName()
    {
        var playSystem = AzathrixFramework.GetSystem<GamePlaySystem>();
        if (playSystem?.currentLevel != null)
        {
            LevelSceneName = playSystem.currentLevel.SceneName;
            Log.Info($"[Tile] 初始化读取场景名: {LevelSceneName}");
            UpdateTitleText();
        }
        else
        {
            LevelSceneName = null;
            UpdateTitleText();
        }
    }

    /// <summary>更新标题文本</summary>
    private void UpdateTitleText()
    {
        if (dynamicTitle != null)
        {
            if (LevelSceneName=="Level9")
            {
                dynamicTitle.text = "Exit";
                return;
            }
            dynamicTitle.text = LevelSceneName+"/8" ?? string.Empty;
            Log.Info($"[Tile] 标题已更新");
        }
        else
        {
            Log.Warning("[Tile] dynamicTitle 未赋值，无法更新标题");
        }
    }
}
