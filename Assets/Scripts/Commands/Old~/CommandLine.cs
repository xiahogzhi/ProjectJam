// using System.Collections.Generic;
// using System.Text;
//
// namespace CommandModule
// {
//     public class CommandLine
//     {
//         public Command command { set; get; }
//
//         public string commandName
//         {
//             get
//             {
//                 if (command != null)
//                     return command.fullName;
//                 return null;
//             }
//         }
//
//         public List<ParamSetter> paramSetters { get; } = new();
//
//         public bool hasCommand
//         {
//             get { return command != null; }
//         }
//
//         public string BuildInputText()
//         {
//             var param = string.Empty;
//             if (command.@params != null)
//             {
//                 for (int i = 0; i < command.@params.Length; i++)
//                 {
//                     var p = command.@params[i];
//                     var ps = GetParam(i);
//                     if (ps == null)
//                     {
//                         param += $" #";
//                     }
//                     else
//                     {
//                         param += " " + ps.Value.key;
//                     }
//                 }
//             }
//
//             return $"{command.fullName}{param}";
//         }
//
//         public override string ToString()
//         {
//             if (command == null)
//             {
//                 return "Empty Command";
//             }
//
//             StringBuilder sb = new StringBuilder();
//             var first = true;
//             foreach (var variable in paramSetters)
//             {
//                 if (first)
//                     first = false;
//                 else
//                     sb.Append(",");
//                 sb.Append($"{variable.key}");
//                 if (variable.value != null)
//                 {
//                     sb.Append($":{variable.value}");
//                 }
//             }
//
//             return $"[{command?.id}] {command?.fullName}({sb})";
//         }
//
//         public void SetCommand(string command)
//         {
//             SetCommand(CommandConsole.instance.GetCommand(command));
//         }
//
//         public void SetCommand(uint command)
//         {
//             SetCommand(CommandConsole.instance.GetCommand(command));
//         }
//
//         public void SetCommand(Command command)
//         {
//             if (this.command != command)
//             {
//                 this.command = command;
//                 paramSetters.Clear();
//
//                 // Debug.Log("选择命令:" + command);
//             }
//         }
//
//         public ParamSetter? GetParam(int index)
//         {
//             foreach (var variable in paramSetters)
//             {
//                 if (variable.index == index)
//                 {
//                     return variable;
//                 }
//             }
//
//             return null;
//         }
//
//         public void SetParam(ParamSetter paramSetter)
//         {
//             for (int i = 0; i < paramSetters.Count; i++)
//             {
//                 if (paramSetters[i].index == paramSetter.index)
//                 {
//                     paramSetters[i] = paramSetter;
//                     return;
//                 }
//             }
//         }
//
//         public void SetParamObjectValue(int index, string key, object value)
//         {
//             var v = GetParam(index);
//             if (v == null)
//             {
//                 ParamSetter ps = new ParamSetter();
//                 ps.index = index;
//                 ps.key = key;
//                 ps.value = value;
//                 paramSetters.Add(ps);
//                 // Debug.Log($"设置参数:{key} => {value}");
//             }
//             else
//             {
//                 var ps = v.Value;
//                 if (ps.key != key)
//                 {
//                     ps.key = key;
//                     ps.value = value;
//                     SetParam(ps);
//                     // Debug.Log($"设置参数:{key} => {value}");
//                 }
//             }
//         }
//
//         public void DeleteParam(int index)
//         {
//             for (int i = 0; i < paramSetters.Count; i++)
//             {
//                 if (paramSetters[i].index == index)
//                 {
//                     paramSetters.RemoveAt(i);
//                     return;
//                 }
//             }
//         }
//
//         public void Reset()
//         {
//             command = null;
//             paramSetters.Clear();
//         }
//
//         public void RemoveParam(int index)
//         {
//             for (int i = 0; i < paramSetters.Count; i++)
//             {
//                 if (paramSetters[i].index == index)
//                 {
//                     paramSetters.RemoveAt(i);
//                     return;
//                 }
//             }
//         }
//     }
// }