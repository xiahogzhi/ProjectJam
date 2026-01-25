using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework.Commands.Attributes;
using Framework.Commands.Suggest;
using Sirenix.Serialization;
using UnityEngine;

namespace Framework.Commands.Core
{
    public class CommandMacro
    {
        [Command("macro.define", "创建宏,需要以命令#end结尾")]
        public static void Macro(string name)
        {
            CommandManager.instance.macro.Define(name);
        }

        [Command("macro.end", "结束宏编辑")]
        public static void EndMacro()
        {
            CommandManager.instance.macro.End();
        }

        [Command("macro.show", "显示所有宏")]
        public static void ShowMacro()
        {
            CommandManager.instance.macro.Show();
        }

        [Command("macro.del", "删除宏")]
        public static void DelMacro([CommandParam("宏命令", suggestAction = "GetMacroSuggest")] string macro)
        {
            CommandManager.instance.macro.Delete(macro);
        }

        public bool isEditing => editingMacro != null;

        public void ProcessMacro(CommandTextEditor temp)
        {
            if (temp.content == "#end")
            {
                End();
                return;
            }

            if (isEditing)
            {
                AddCommandToEditMacro(temp.content);
                return;
            }

            ExecuteMacro(temp.commandName, temp.@params.Cast<object>().ToArray());
        }

        void ExecuteMacro(string macro, object[] @params)
        {
            var f = FindMacro(macro);
            if (f != null)
            {
                try
                {
                    foreach (var variable in f.commands)
                        commandManager.Execute(variable, true);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                return;
            }

            var d = FindDynamicMacro(macro);
            if (d != null)
            {
                try
                {
                    d.command.DynamicInvoke(@params);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                return;
            }

            Debug.LogError($"未知宏命令:{macro}");
        }

        public void Delete(string macro)
        {
            if (!macro.StartsWith("#"))
                macro = "#" + macro;

            for (int i = 0; i < macros.Count; i++)
            {
                if (macros[i].name == macro)
                {
                    macros.RemoveAt(i);
                    OnMacroChangedEvt?.Invoke();
                    Debug.Log("删除成功: " + macro);
                    return;
                }
            }

            Debug.Log("删除失败:" + macro + " 不存在");
        }

        static CommandSuggestList<string> GetMacroSuggest()
        {
            CommandSuggestList<string> result = new CommandSuggestList<string>();
            foreach (var variable in CommandManager.instance.macro.macros)
            {
                result.Add(variable.name);
            }

            return result;
        }

        public void Help(string macro)
        {
            var m = FindMacro(macro);
            if (m != null)
            {
                Debug.Log($"宏命令:{m.name}");
                foreach (var variable in m.commands)
                    Debug.Log(" > " + variable);
            }
            else
            {
                Debug.Log($"未知宏命令:{macro}");
            }
        }

        public void Show()
        {
            foreach (var variable in macros)
            {
                Debug.Log($"{variable.name}");
            }
        }

        public void AddDynamicMacros(string name, string comment, Delegate deg)
        {
            var d = FindDynamicMacro(name);
            if (d != null)
                return;
            dynamicMacros.Add(
                new DynamicMacro() {name = name, comment = comment, command = deg});
        }

        string filePath => Path.Combine(Application.persistentDataPath, "command_macro.dat");

        public CommandMacro(CommandManager commandManager)
        {
            this.commandManager = commandManager;
            commandManager.OnLoadEvt += () =>
            {
                if (File.Exists(filePath))
                {
                    var data = File.ReadAllBytes(filePath);
                    var m = SerializationUtility.DeserializeValue<List<Macro>>(data, DataFormat.Binary);
                    if (m != null)
                        macros = m;
                }
            };
            commandManager.OnSaveEvt += () =>
            {
                var data = SerializationUtility.SerializeValue(macros, DataFormat.Binary);
                File.WriteAllBytes(filePath, data);
            };

            AddDynamicMacros("#show", "显示所有宏命令", new Action(Show));
            AddDynamicMacros("#del", "删除宏命令", new Action<string>(Delete));
            AddDynamicMacros("#define", "定义宏命令", new Action<string>(Define));
        }

        private CommandManager commandManager { get; }

        /// <summary>
        /// 宏命令
        /// </summary>
        public List<Macro> macros { get; set; } = new List<Macro>();

        /// <summary>
        /// 动态宏命令
        /// </summary>
        public List<DynamicMacro> dynamicMacros { get; } = new List<DynamicMacro>();


        public event Action OnMacroChangedEvt;

        private Macro editingMacro { get; set; }

        public void Define(string name)
        {
            if (editingMacro != null)
            {
                Debug.Log("请先结束当前宏编辑");
                return;
            }

            var macro = FindMacro("#" + name);
            if (macro != null)
            {
                Debug.Log("宏已存在,请先删除");
                return;
            }

            if (name.Contains("#"))
            {
                Debug.Log("名字不能包含#");
                return;
            }

            editingMacro = new Macro {name = "#" + name};
            Debug.Log($"开始记录宏命令,结束需以#end");
        }

        /// <summary>
        /// 查找宏
        /// </summary>
        /// <param name="macro"></param>
        /// <returns></returns>
        public Macro FindMacro(string macro)
        {
            foreach (var variable in macros)
            {
                if (variable.name == macro)
                    return variable;
            }

            return null;
        }

        public DynamicMacro FindDynamicMacro(string macro)
        {
            foreach (var variable in dynamicMacros)
            {
                if (variable.name == macro)
                    return variable;
            }

            return null;
        }

        public void AddCommandToEditMacro(string command)
        {
            if (editingMacro == null)
            {
                Debug.Log("请先创建宏");
                return;
            }

            Debug.Log($"记录命令:{editingMacro.name} => {command}");

            editingMacro.commands.Add(command);
        }


        public void End()
        {
            if (editingMacro == null)
            {
                Debug.Log("请先创建宏");
                return;
            }

            Debug.Log($"创建宏成功:{editingMacro.name}");
            macros.Add(editingMacro);
            editingMacro = null;
            OnMacroChangedEvt?.Invoke();
        }
    }
}