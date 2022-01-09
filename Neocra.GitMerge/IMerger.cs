namespace Neocra.GitMerge
{
    public interface IMerger
    {
        MergeStatus Merge(MergeOptions opts);
        string ProviderCode { get; }
    }

}