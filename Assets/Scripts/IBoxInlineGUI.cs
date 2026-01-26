#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif
using UnityEngine;

namespace Framework.Misc.Interfaces
{
#if UNITY_EDITOR
    public class IBoxInlineGUIDrawer : OdinValueDrawer<IBoxInlineGUI>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // SirenixEditorGUI.BeginHorizontalPropertyLayout(label);
            SirenixEditorGUI.BeginInlineBox();
            CallNextDrawer(label);
            SirenixEditorGUI.EndInlineBox();
            // SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }

#endif

    public interface IBoxInlineGUI
    {
    }
}