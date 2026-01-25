using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Framework.Commands.Core;
using TMPro;
using UnityEngine;

namespace Framework.Commands.View
{
    public class ConsoleContent : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private ConsoleMessageView _consoleMessageViewPrefab;
        [SerializeField] private TextMeshProUGUI _calcText;

        private List<ConsoleMessage> messages { get; } = new List<ConsoleMessage>();

        private List<string> waitAdMessages { get; } = new List<string>();
        private Queue<Action> actionQueue { get; } = new Queue<Action>();

        private bool isActive { set; get; }

        private bool isInit { set; get; }


        private void OnEnable()
        {
            if (!isInit)
            {
                isInit = true;
                _scroller.Delegate = this;
                // _scroller.ReloadData();
            }
        }

        public void AddMessage(string[] text)
        {
        }

        private void Update()
        {
            while (actionQueue.Count > 0)
            {
                var de = actionQueue.Dequeue();
                de();
            }
        }


        void _AddMessage(string text, bool ignore = false)
        {
            var canJumpEnd = messages.Count > 0 && _scroller.EndDataIndex >= messages.Count - 3;


            _calcText.text = text;
            messages.Add(new ConsoleMessage() {height = _calcText.preferredHeight, text = text});

            if (!ignore)
            {
                var oldPosition = _scroller.ScrollPosition;
                _scroller.ReloadData();

                if (canJumpEnd)
                {
                    _scroller.JumpToDataIndex(messages.Count);
                }
                else
                {
                    _scroller.ScrollPosition = oldPosition;
                }
            }
        }

        public void AddMessage(string text, bool ignore = false, bool isDirect = false)
        {
            if (!isActive)
            {
                waitAdMessages.Add(text);
                return;
            }

            if (isDirect)
            {
                _AddMessage(text, ignore);
            }
            else
            {
                actionQueue.Enqueue(() => { _AddMessage(text, ignore); });
            }
        }


        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return messages.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return messages[dataIndex].height;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int
            dataIndex, int cellIndex)
        {
            ConsoleMessageView cellView = scroller.GetCellView(_consoleMessageViewPrefab) as
                ConsoleMessageView;
            cellView.text.text = messages[dataIndex].text;
            cellView.name = "Message : " + dataIndex;
            return cellView;
        }

        public void SetActive(bool value)
        {
            isActive = value;
            gameObject.SetActive(value);

            if (value)
            {
                if (waitAdMessages.Count > 0)
                {
                    foreach (var variable in waitAdMessages)
                        AddMessage(variable, true, true);

                    waitAdMessages.Clear();
                    _scroller.ReloadData();
                    _scroller.JumpToDataIndex(messages.Count);
                }
            }
        }
    }
}