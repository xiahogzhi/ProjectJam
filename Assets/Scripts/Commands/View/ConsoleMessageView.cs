using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace Framework.Commands.View
{
    public class ConsoleMessageView : EnhancedScrollerCellView
    {
        [SerializeField] private TextMeshProUGUI _text;

        public TextMeshProUGUI text
        {
            get => _text;
            set => _text = value;
        }
    }
}