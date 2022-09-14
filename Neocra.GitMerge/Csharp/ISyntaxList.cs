namespace Neocra.GitMerge.Csharp
{
    public interface ISyntaxList<in T, out TCollection>
    {
        int Count { get; }
        
        TCollection Insert(int index, T value);

        TCollection RemoveAt(int index);
        TCollection Move(int index, int movedIndex);
    }
}