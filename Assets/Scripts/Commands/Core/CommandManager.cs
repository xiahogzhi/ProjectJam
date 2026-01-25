using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Commands.Attributes;
using Framework.Commands.Suggest;
using UnityEngine;

namespace Framework.Commands.Core
{
    public class CommandManager
    {
        /// <summary>
        /// 命令建议
        /// </summary>
        public CommandSuggest suggest { get; }


        /// <summary>
        /// 宏命令
        /// </summary>
        public CommandMacro macro { get; }

        /// <summary>
        /// 命令历史
        /// </summary>
        public CommandHistory commandHistory { get; }

        // public const string CommandSaveFileName = "command.dat";

        /// <summary>
        /// 命令集合
        /// </summary>
        public Dictionary<string, Command> commands { get; } = new();

        public event Action OnSaveEvt;
        public event Action OnScanCommandEvt;
        public event Action OnLoadEvt;

        private static CommandManager _instance;

        public static CommandManager instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CommandManager();
                return _instance;
            }
        }

        public CommandManager()
        {
            macro = new CommandMacro(this);
            suggest = new CommandSuggest(this);
            commandHistory = new CommandHistory(this);
        }

        public void ScanCommand()
        {

            commands.Clear();
            var r =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                from method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                where method.IsDefined(typeof(CommandAttribute))
                select new {type, method};

            foreach (var variable in r)
            {
                Command c = new Command(variable.type, variable.method);
                if (c.fullName.Contains("#"))
                {
                    Debug.LogError("无效命令,命令名中不能包含#: " + c.fullName);
                    continue;
                }

                if (commands.ContainsKey(c.fullName))
                {
                    Debug.LogError("重复命令: " + c.fullName);
                    continue;
                }

                commands.Add(c.fullName, c);
                Debug.Log("加载命令: " + c);
            }

            OnScanCommandEvt?.Invoke();
        }

        public Command FindCommand(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                return null;

            return commands.GetValueOrDefault(commandName);
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public void Load()
        {
            OnLoadEvt?.Invoke();
        }

        /// <summary>
        /// 存储数据
        /// </summary>
        public void Save()
        {
            OnSaveEvt?.Invoke();
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ignoreHistory"></param>
        public void Execute(string str, bool ignoreHistory = false)
        {
            if (string.IsNullOrEmpty(str))
                return;

            if (!ignoreHistory)
                commandHistory.AddHistory(str);

            var temp = new CommandTextEditor();

            temp.Reset();
            temp.content = str;

            if (str.StartsWith("#") || macro.isEditing)
            {
                macro.ProcessMacro(temp);
                return;
            }


            if (string.IsNullOrEmpty(temp.commandName))
                return;

            var command = FindCommand(temp.commandName);
            if (command == null)
            {
                Debug.LogError($"未知命令:{temp.commandName}");
                return;
            }

            var paramObjects = CommandUtils.GetParamObjects(command, temp.@params);

            try
            {
                Debug.Log($"执行命令:{str}");
                command.method.Invoke(null, paramObjects);
            }
            catch (Exception e)
            {
                Debug.LogError($"错误命令:{str}");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 获取建议
        /// </summary>
        public SuggestBuilder GetSuggest(CommandTextEditor commandTextEditor)
        {
            var command = FindCommand(commandTextEditor.commandName);

            if (commandTextEditor.editingIndex == 0)
                return suggest.commandSuggest;

            var p = command.TryGetParam(commandTextEditor.editingIndex - 1);
            if (p != null)
            {
                var sg = suggest.GetParamSuggests(p);
                if (sg != null)
                {
                    var d = commandTextEditor.TryGetData(commandTextEditor.editingIndex);
                    if (string.IsNullOrEmpty(d))
                        sg.ClearSearch();
                    else
                        sg.Search(d);
                }
            }

            return null;
        }

        public ValueTuple<Command, SuggestBuilder> GetCommandWithSuggest(CommandTextEditor commandTextEditor)
        {
            if (string.IsNullOrEmpty(commandTextEditor.content))
                return new ValueTuple<Command, SuggestBuilder>(null, null);

            var command = FindCommand(commandTextEditor.commandName);
            SuggestBuilder sb = null;

            if (commandTextEditor.editingIndex == 0 || command == null)
            {
                sb = suggest.commandSuggest;
                if (sb != null)
                {
                    var d = commandTextEditor.TryGetData(0);
                    if (string.IsNullOrEmpty(d))
                        sb.ClearSearch();
                    else
                        sb.Search(d);
                }
            }

            if (commandTextEditor.editingIndex > 0 && command == null)
            {
                return new ValueTuple<Command, SuggestBuilder>();
            }

            if (sb == null && command != null)
            {
                var p = command.TryGetParam(commandTextEditor.editingIndex - 1);
                if (p != null)
                {
                    sb = suggest.GetParamSuggests(p);
                }

                if (sb != null)
                {
                    var d = commandTextEditor.TryGetData(commandTextEditor.editingIndex);
                    if (string.IsNullOrEmpty(d))
                        sb.ClearSearch();
                    else
                        sb.Search(d);
                }
            }


            return new ValueTuple<Command, SuggestBuilder>(command, sb);
        }
    }
}