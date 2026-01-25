using System;

namespace Framework.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CommandParamAttribute : Attribute
    {
        /// <summary>
        /// 参数名
        /// </summary>
        public string name { set; get; }

        /// <summary>
        /// 参数解释
        /// </summary>
        public string comment { set; get; }

        /// <summary>
        /// 参数建议方法
        /// </summary>
        public string suggestAction { set; get; }

        /// <summary>
        /// 缓存建议
        /// </summary>
        public bool cacheSuggest { set; get; }

        public CommandParamAttribute(string name = null, string comment = null, string suggestAction = null,
            bool cacheSuggest = true)
        {
            this.comment = comment;
            this.name = name;
            this.suggestAction = suggestAction;
            this.cacheSuggest = cacheSuggest;
        }
    }
}