using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Commands.Core
{
    /// <summary>
    /// 命令编辑器
    /// </summary>
    public class CommandTextEditor
    {
        public CommandTextEditor(string content)
        {
            _content = content;
            Update();
        }

        public CommandTextEditor()
        {
        }

        /// <summary>
        /// 命令数据
        /// </summary>
        public List<string> data { set; get; } = new List<string>();

        /// <summary>
        /// 当前编辑索引
        /// </summary>
        public int editingIndex { set; get; } = -1;

        private static StringBuilder temp { get; } = new StringBuilder();

        private int _position = -1;

        private string _content;

        /// <summary>
        /// 内容字符索引
        /// </summary>
        public int contentCharIndex
        {
            set
            {
                if (_position != value)
                {
                    _position = value;
                    Update();
                }
            }
            get => _position;
        }

        public string content
        {
            set
            {
                if (_content != value)
                {
                    _content = value;
                    Update();
                }
            }
            get => _content;
        }

        public EditTypeEnum editType
        {
            get
            {
                if (editingIndex < 0)
                    return EditTypeEnum.None;

                if (editingIndex == 0)
                    return EditTypeEnum.Command;

                return EditTypeEnum.Param;
            }
        }

        public enum EditTypeEnum
        {
            None,
            Command,
            Param
        }

        public string commandName
        {
            get
            {
                if (data.Count > 0)
                    return data[0];
                return null;
            }
        }

        public string[] @params
        {
            get
            {
                if (data.Count > 1)
                {
                    string[] result = new string[data.Count - 1];
                    for (int i = 1; i < data.Count; i++)
                    {
                        result[i - 1] = data[i];
                    }

                    return result;
                }

                return Array.Empty<string>();
            }
        }

        public void Reset()
        {
            data.Clear();
            _content = null;
            _position = -1;
            editingIndex = -1;
        }

        public void Update()
        {
            data.Clear();
            temp.Clear();
            editingIndex = -1;
            if (string.IsNullOrEmpty(content))
                return;
            bool ignoreSpace = false;

            bool endFlag = content[^1] == ' ';
            for (int i = 0; i < content.Length; i++)
            {
                char ch = content[i];
                if (contentCharIndex == i)
                    editingIndex = data.Count;

                if (ch == '"')
                {
                    ignoreSpace = !ignoreSpace;
                    continue;
                }

                if (ch == ' ')
                {
                    if (ignoreSpace)
                    {
                        temp.Append(ch);
                        continue;
                    }

                    if (temp.Length > 0)
                    {
                        data.Add(temp.ToString());
                        temp.Clear();
                    }
                }
                else
                {
                    temp.Append(ch);
                }
            }

            if (temp.Length > 0)
            {
                data.Add(temp.ToString());
            }

            if (editingIndex == -1)
            {
                if (endFlag)
                    data.Add("");

                editingIndex = data.Count - 1;
            }
        }

        public override string ToString()
        {
            return _content;
        }

        public string TryGetCurrentData()
        {
            return TryGetData(editingIndex);
        }

        public string TryGetData(int i)
        {
            if (i < 0 || i >= data.Count)
                return null;
            return data[i];
        }

        public void Update(string inputText, int inputStringPosition)
        {
            _content = inputText;
            _position = inputStringPosition;
            Update();
        }

        public void SetData(int index, string d)
        {
            if (index < 0 || index >= data.Count)
                return;

            data[index] = d;
        }

        public string RebuildString()
        {
            string result = "";
            for (int i = 0; i < data.Count; i++)
            {
                result += data[i];

                if (i < data.Count - 1)
                    result += " ";
            }

            return result;
        }

        public void Backspace()
        {
            if (data.Count <= 0)
                return;

            data.RemoveAt(data.Count - 1);
        }
    }
}