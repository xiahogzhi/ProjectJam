using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Framework.Commands.Core
{
    public static class CommandUtils
    {
        public static string GetCommandParamText(this Command command)
        {
            if (command.@params.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.Append("  <color=#FFF46C>");
            sb.Append('[');
            bool isFirst = true;
            foreach (var variable in command.@params)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(',');
                sb.Append(variable.name);
            }

            sb.Append(']');
            sb.Append("</color>");

            return sb.ToString();
        }

        public static string GetCommandParamText2(this Command command)
        {
            if (command.@params.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.Append("  <color=#858585>");
            // sb.Append('[');
            bool isFirst = true;
            foreach (var variable in command.@params)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(' ');
                sb.Append(variable.paramInfo.ParameterType.Name);
            }

            // sb.Append(']');
            sb.Append("</color>");

            return sb.ToString();
        }

        public static object[] GetParamObjects(Command command, string[] @params)
        {
            List<object> result = new List<object>();
            for (int i = 0; i < command.@params.Length; i++)
            {
                var commandParam = command.@params[i];
                if (!(i < @params.Length))
                {
                    if (commandParam.paramInfo.HasDefaultValue)
                    {
                        result.Add(commandParam.paramInfo.DefaultValue);
                        continue;
                    }

                    result.Add(null);
                    continue;
                }

                var textParam = @params[i];
                object value = null;
                //查找建议
                if (!string.IsNullOrEmpty(textParam))
                {
                    var suggest = commandParam.FindParamSuggest(textParam);
                    if (suggest != null)
                        value = suggest.GetValue();

                    //强制转换
                    if (value == null)
                        value = Cast(textParam, commandParam.paramInfo);
                }


                result.Add(value);
            }

            return result.ToArray();
        }

        public static object Cast(string str, ParameterInfo parameterInfo, bool ignoreArray = false)
        {
            if (str == "#" || string.IsNullOrEmpty(str))
            {
                if (parameterInfo.HasDefaultValue)
                    return parameterInfo.DefaultValue;
                return null;
            }

            var type = parameterInfo.ParameterType;
            if (parameterInfo.ParameterType.IsArray)
            {
                type = parameterInfo.ParameterType.GetElementType();
                if (!ignoreArray)
                {
                    var sp = str.Split(',');
                    var array = Array.CreateInstance(type, sp.Length);
                    for (int i = 0; i < sp.Length; i++)
                    {
                        array.SetValue(Cast(sp[i], parameterInfo, true), i);
                    }

                    return array;
                }

            }


            if (type == typeof(int))
            {
                int.TryParse(str, out var p);
                return p;
            }

            if (type == typeof(double))
            {
                double.TryParse(str, out var p);
                return p;
            }

            if (type == typeof(float))
            {
                float.TryParse(str, out var p);
                return p;
            }

            if (type == typeof(string))
            {
                return str;
            }

            if (type == typeof(short))
            {
                short.TryParse(str, out var p);
                return p;
            }

            if (type == typeof(long))
            {
                long.TryParse(str, out var p);
                return p;
            }

            if (type == typeof(bool))
            {
                bool.TryParse(str, out var p);
                return p;
            }

            if (type == typeof(decimal))
            {
                decimal.TryParse(str, out var p);
                return p;
            }

            if (type == typeof(uint))
            {
                uint.TryParse(str, out var p);
                return p;
            }

            if (type == typeof(ushort))
            {
                ushort.TryParse(str, out var p);
                return p;
            }

            if (type == typeof(ulong))
            {
                ulong.TryParse(str, out var p);
                return p;
            }

            return null;
        }
    }
}