using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Neocra.GitMerge
{
    public class MergerSelecter : IMerger
    {
        private readonly IEnumerable<IMerger> mergers;
        private readonly ILogger<MergerSelecter> logger;

        public MergerSelecter(IEnumerable<IMerger> mergers, ILogger<MergerSelecter> logger)
        {
            this.mergers = mergers;
            this.logger = logger;
        }

        public MergeStatus Merge(MergeOptions opts)
        {
            this.logger.LogInformation("Search provider for {ancestor} {current} {other} {PathName}", 
                opts.Ancestor,
                opts.Current,
                opts.Other,
                opts.PathName);

            var file = new FileInfo(opts.PathName ?? opts.Current);

            var provider = file switch
            {
                { Extension: ".xml" } => "xml",
                { Extension: ".xaml" } => "xml",
                { Extension: ".csproj" } => "csproj",
                { Extension: ".cs" } => "csharp",
                { Extension: ".resx" } => "xml",
                { Name: "package.config" } => "xml",
                _ => throw new NotSupportedException("This file is not supported.")
            };
            this.logger.LogInformation("Provider to use {provider}", provider);

            var mergeStatus = this.mergers.First(m => m.ProviderCode == provider)
                .Merge(opts);
            this.logger.LogInformation("Result {mergeStatus}", mergeStatus);
            return mergeStatus;
        }

        public string ProviderCode => "auto";
    }
}