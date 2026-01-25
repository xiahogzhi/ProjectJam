using System.Collections.Generic;

namespace Framework.Commands.Suggest
{
    public class CommandSuggestList<T> : List<SuggestItem<T>>
    {
        public void Add(string name, T value, string comment = null)
            => this.Add(new SuggestItem<T>(name, value, comment));

        public void Add(T value)
            => this.Add(new SuggestItem<T>(value.ToString(), value));
    }
}