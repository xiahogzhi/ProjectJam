// using System.Collections.Generic;
// using GameModule;
// using GameModule.Contexts;
// using GameModule.Interfaces;
//
// namespace CommandModule
// {
//     public class CommandSystem : GameSystem<CommandSystem>, ISystemInitialize
//     {
//         private static List<Command> _commands = new List<Command>();
//         public  CommandHistory commandHistory { get; } = new();
//         
//         public override string name => $"命令系统({nameof(CommandSystem)})";
//
//
//         public void ExecuteCommand(CommandLine commandLine)
//         {
//             commandHistory.AddHistory(commandLine);
//
//             //查找命令
//             if (commandLine.hasCommand)
//             {
//                 var command = commandLine.command;
//                 var act = command.commandAction;
//                 List<object> @params = new List<object>();
//                 for (int i = 0; i < command.@params.Length; i++)
//                 {
//                     var param = command.@params[i];
//                     var paramSetter = commandLine.GetParam(i);
//                     var value = paramSetter?.value;
//
//                     //查找建议
//                     if (value == null)
//                     {
//                         var suggest = param.FindParamSuggest(paramSetter?.key);
//                         if (suggest != null)
//                             value = suggest.GetValue();
//                     }
//
//                     //强制转换
//                     if (value == null)
//                     {
//                         value = CommandUtils.Cast(paramSetter?.key, param.paramInfo);
//                     }
//
//                     @params.Add(value);
//                 }
//
//                 act.Invoke(null, @params.ToArray());
//             }
//         }
//
//
//         public  IEnumerable<Command> GetCommands()
//         {
//             foreach (var c in _commands)
//             {
//                 yield return c;
//             }
//         }
//
//         public  IEnumerable<Command> SearchCommand(string command)
//         {
//             foreach (var c in _commands)
//             {
//                 var fullName = c.fullName;
//                 if (fullName.Contains(command))
//                     yield return c;
//             }
//         }
//
//         public  Command GetCommand(uint id)
//         {
//             if (id == 0)
//                 return null;
//
//             foreach (var c in _commands)
//             {
//                 if (c.id == id)
//                     return c;
//             }
//
//             return null;
//         }
//
//         public  Command GetCommand(string command)
//         {
//             foreach (var c in _commands)
//             {
//                 var fullName = c.fullName;
//                 if (fullName == command)
//                     return c;
//             }
//
//             return null;
//         }
//
//         public  void InvokeCommand(Command command, params object[] @params)
//         {
//             command.Invoke(@params);
//         }
//
//         void ISystemInitialize.Initialize(Launcher launcher)
//         {
//         
//         }
//     }
// }