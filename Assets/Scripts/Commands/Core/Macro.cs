using System.Collections.Generic;
using Sirenix.Serialization;

namespace Framework.Commands.Core
{
    public class Macro
    {
        [OdinSerialize] public string name { set; get; }
        [OdinSerialize] public List<string> commands { get; private set; } = new List<string>();
    }
}