using Azathrix.Framework.Core.Launcher;
using Azathrix.Framework.Core.Pipeline;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

[Register]
[PhaseId("ChangeSceneHook")]
public class ChangeSceneHook : ILauncherPhase
{
    public async UniTask ExecuteAsync(LauncherContext context)
    {
#if UNITY_EDITOR
        SceneManager.LoadScene("Launcher");

        await UniTask.WaitForSeconds(0.1f);
#endif
    }

    public int Order { get; } = -100;
}