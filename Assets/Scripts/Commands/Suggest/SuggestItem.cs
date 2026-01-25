namespace Framework.Commands.Suggest
{
    public  struct SuggestItem<T> : ISuggestItem
    {
        public string name { get; }

        public string comment { get; }

        public T value { get; }

        public SuggestItem(string name, T value, string comment = null)
        {
            this.name = name;
            this.comment = comment;
            this.value = value;
        }

        public string GetName()
        {
            return name;
        }

        public string GetComment()
        {
            return comment;
        }

        public object GetValue()
        {
            return value;
        }
    }
}