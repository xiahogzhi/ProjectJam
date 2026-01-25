namespace Framework.Commands.Suggest
{
    public interface ISuggestItem
    {
        string GetName();
        string GetComment();
        object GetValue();
    }
}