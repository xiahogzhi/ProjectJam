using System;

namespace Framework.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string name { set; get; }

        public string comment { set; get; }

        public CommandAttribute(string name = null, string comment = null)
        {
            this.name = name;
            this.comment = comment;
        }
    }
}