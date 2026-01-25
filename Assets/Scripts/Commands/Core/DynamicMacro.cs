using System;

namespace Framework.Commands.Core
{
    public class DynamicMacro
    {
        public string name { set; get; }
        public string comment { set; get; }
        public Delegate command { set; get; }
    }
}