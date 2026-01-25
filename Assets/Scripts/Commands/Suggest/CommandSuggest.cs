using System.Collections;
using System.Collections.Generic;
using Framework.Commands.Core;

namespace Framework.Commands.Suggest
{
    /// <summary>
    /// 命令建议
    /// </summary>
    public class CommandSuggest
    {
        public CommandSuggest(CommandManager commandManager)
        {
            this.commandManager = commandManager;
            commandManager.OnScanCommandEvt += InitializeCommandSuggest;
            commandManager.macro.OnMacroChangedEvt += InitializeCommandSuggest;
        }

        private CommandManager commandManager { get; }

        //命令建议
        public SuggestBuilder<string> commandSuggest { get; } = new SuggestBuilder<string>();

        private Dictionary<CommandParam, SuggestBuilder<ISuggestItem>> paramSuggestDic { get; } = new();

        public SuggestBuilder<ISuggestItem> GetParamSuggests(CommandParam cp)
        {
            if (cp == null)
                return null;

            if (cp.suggestAction == null)
                return null;

            if (paramSuggestDic.TryGetValue(cp, out var r))
                return r;


            if (cp.suggestAction.Invoke(null, null) is IList list)
            {
                SuggestBuilder<ISuggestItem> builder = new SuggestBuilder<ISuggestItem>();
                foreach (ISuggestItem item in list)
                {
                    builder.AddSuggest(item);
                }

                if (cp.paramAttribute.cacheSuggest)
                    paramSuggestDic.Add(cp, builder);

                return builder;
            }

            return null;
        }

        void InitializeCommandSuggest()
        {
            // _commandSuggest.OnSelectEvt += (s, u) =>
            // {
            //     if (u > 0)
            //         _commandLine.SetCommand(u);
            // };

            commandSuggest.Clear();
            foreach (var v in commandManager.commands)
            {
                // string paramText = cur.GetCommandParamText2();
                // string comment = string.Empty;
                // if (!string.IsNullOrEmpty(cur.CommandAttr.Comment))
                //     comment = $"  <color=#7EFF6C>#{cur.CommandAttr.Comment}</color>";
                var cur = v.Value;
                string select = $"<b><color=#FFF46C> > {cur.fullName}</color></b>";
                string normal = $"{cur.fullName}";

                commandSuggest.AddSuggest(normal, select, cur.fullName, cur.fullName);
            }

            List<(string, string)> m = new List<(string, string)>();
            foreach (var variable in commandManager.macro.macros)
                m.Add((variable.name, null));

            foreach (var variable in commandManager.macro.dynamicMacros)
                m.Add((variable.name, variable.comment));

            foreach (var variable in m)
            {
                var comment = variable.Item2;
                var cur = variable.Item1;
                if (!string.IsNullOrEmpty(comment))
                    comment = $"  <color=#8A8A8A><i>{comment}</i></color>";

                string select = $"<b><color=#FFF46C> > {cur}{comment}</color></b>";
                string normal = $"{cur}{comment}";
                commandSuggest.AddSuggest(normal, select, variable.Item1, variable.Item1);
            }
        }
    }
}