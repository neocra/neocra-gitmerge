namespace Neocra.GitMerge
{
    public interface IMerger
    {
        MergeStatus Merge(MergeSettings opts);
        string ProviderCode { get; }
    }

}