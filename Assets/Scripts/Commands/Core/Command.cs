using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Commands.Attributes;

namespace Framework.Commands.Core
{
    public class Command
    {
        /// <summary>
        /// 命令名
        /// </summary>
        public string name { get; }

        /// <summary>
        /// 命令全名
        /// </summary>
        public string fullName { get; }

        /// <summary>
        /// 命令类型
        /// </summary>
        public Type type { get; }

        /// <summary>
        /// 命令属性
        /// </summary>
        public CommandAttribute attribute { get; }

        /// <summary>
        /// 命令方法
        /// </summary>
        public MethodInfo method { get; }

        /// <summary>
        /// 命令参数
        /// </summary>
        public CommandParam[] @params { get; }

        public Command(Type type, MethodInfo method)
        {
            CommandAttribute ca = method.GetCustomAttribute<CommandAttribute>();
            name = ca.name;
            if (string.IsNullOrEmpty(name))
                name = method.Name;

            attribute = ca;
            this.type = type;
            this.method = method;

            if (type.IsDefined(typeof(CommandGroupAttribute)))
            {
                var g = type.GetCustomAttributes<CommandGroupAttribute>();
                var commandGroupAttributes = g.ToList();
                foreach (var variable in commandGroupAttributes)
                    fullName += variable.groupName + ".";
            }

            if (method.IsDefined(typeof(CommandGroupAttribute)))
            {
                var g = method.GetCustomAttributes<CommandGroupAttribute>();
                var commandGroupAttributes = g.ToList();
                foreach (var variable in commandGroupAttributes)
                    fullName += variable.groupName + ".";
            }

            //解析参数
            List<CommandParam> ps = new List<CommandParam>();

            foreach (var parameter in method.GetParameters())
            {
                CommandParam cp = new CommandParam(this, parameter);
                ps.Add(cp);
            }

            this.@params = ps.ToArray();
            fullName += name;
        }

        public void Invoke(params object[] @params)
        {
            method?.Invoke(null, @params);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(attribute.comment))
                return fullName;

            return $"{fullName}  #{attribute.comment}";
        }

        public CommandParam TryGetParam(int index)
        {
            if (index < 0)
                return null;
            if (index >= @params.Length)
                return null;
            return @params[index];
        }
    }
}