namespace Neocra.GitMerge.Csharp
{
    public interface ISyntaxList<T, TCollection>
    {
        int Count { get; }
        
        TCollection Insert(int index, T value);

        TCollection RemoveAt(int index);
    }
}