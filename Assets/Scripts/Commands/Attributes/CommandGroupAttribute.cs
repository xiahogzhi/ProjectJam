using System;

namespace Framework.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class CommandGroupAttribute : Attribute
    {
        /// <summary>
        /// 命令组
        /// </summary>
        public string groupName { set; get; }

        public CommandGroupAttribute(string groupName)
        {
            this.groupName = groupName;
        }
    }
}