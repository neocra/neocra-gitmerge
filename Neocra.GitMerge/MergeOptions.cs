// using CommandLine;

namespace Neocra.GitMerge
{
    public class MergeOptions
    {
        // [Value(0, MetaName = "ancestor", HelpText = "Ancestor of the merge", Required = true)]
        public string Ancestor { get; set; } = null!;

        // [Value(1, MetaName = "current", HelpText = "Current branch", Required = true)]
        public string Current { get; set; } = null!;

        // [Value(2, MetaName = "other", HelpText = "Other branch", Required = true)]
        public string Other { get; set; } = null!;

        // [Value(3, MetaName = "pathname", HelpText = "Path of the destination file", Required = false)]
        public string? PathName { get; set; }
        
        // [OptionAttribute(shortName:'v', longName: "verbose", HelpText = "Verbose option")]
        public bool Verbose { get; set; }
    }
}