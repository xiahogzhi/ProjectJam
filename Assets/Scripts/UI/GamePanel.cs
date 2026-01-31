using Azathrix.EzUI;
using Azathrix.EzUI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GamePanel : FocusUI, IMainUI
    {
        [SerializeField] private Button _exitButton;

        protected override void OnScriptInitialize()
        {
            base.OnScriptInitialize();
            _exitButton?.onClick.AddListener(() =>
                {
                    UISystem.Show<StartPanel>();
                }
            );
        }
    }
}