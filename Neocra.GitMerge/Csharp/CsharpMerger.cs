using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Neocra.GitMerge.Csharp.DiffTools;

namespace Neocra.GitMerge.Csharp;

public class CsharpMerger : IMerger
{
    private readonly Tools.DiffTools diffTools;
    private readonly ILogger<CsharpMerger> logger;
    private readonly CsharpApply csharpApply;

    public CsharpMerger(Tools.DiffTools diffTools, ILogger<CsharpMerger> logger, CsharpApply csharpApply)
    {
        this.diffTools = diffTools;
        this.logger = logger;
        this.csharpApply = csharpApply;
    }

    public MergeStatus Merge(MergeSettings opts)
    {
        var ancestor = this.ParseFile(opts.Ancestor);
        var current = this.ParseFile(opts.Current);
        var other = this.ParseFile(opts.Other);

        var diffCurrent = this.Diffs(ancestor, current);
        var diffOther = this.Diffs(ancestor, other);
            
        if (DetectConflict(diffCurrent, diffOther))
        {
            return MergeStatus.Conflict;
        }
            
        this.LogDiffs("", diffCurrent.Union(diffOther).ToList());
        
        ancestor = this.csharpApply.Apply(ancestor, diffCurrent.Union(diffOther).ToList());
            
        File.Delete(opts.Current);
            
        using (var streamWriter = new StreamWriter(File.OpenWrite(opts.Current)))
        {
            ancestor.WriteTo(streamWriter);
        }

        return MergeStatus.Good;
    }

    private List<Diff> Diffs(CompilationUnitSyntax ancestor, CompilationUnitSyntax current)
    {
        var diffUsingCurrent = this.diffTools.GetDiffOfChildrenFusion(
            new UsingDirectiveDiffToolsConfig(),
            ancestor.Usings.ToList(),
            current.Usings.ToList()).ToList();

        var diffMembersCurrent = this.diffTools.GetDiffOfChildrenFusion(
            new MemberDeclarationDiffToolsConfig(this.diffTools),
            ancestor.Members.ToList(),
            current.Members.ToList()).ToList();
        return diffUsingCurrent.OfType<Diff>()
            .Union(diffMembersCurrent).ToList();
    }

    private bool DetectConflict(List<Diff> diffsCurrent, List<Diff> diffsOther)
    {
        foreach (var diffCurrent in diffsCurrent)
        {
            var diffOther = diffsOther.FirstOrDefault(d => d.IndexOfChild == diffCurrent.IndexOfChild);

            if (diffOther == null)
            {
                continue;
            }

            switch (diffCurrent.Mode, diffOther.Mode)
            {
                case (DiffMode.Update, DiffMode.Update):
                    if (this.MergeConflict(diffsOther, diffCurrent, diffOther)) return true;
                    break;
                case (DiffMode.Delete, DiffMode.Delete):
                    diffsOther.Remove(diffOther);
                    break; 
                case (DiffMode.Add, DiffMode.Add):
                    break;
                case (DiffMode.Move, DiffMode.Move):
                    diffsOther.Remove(diffOther);
                    break;
                case (DiffMode.Move, DiffMode.Add):
                case (DiffMode.Add, DiffMode.Move):
                    break;
                case (DiffMode.Move, DiffMode.Update):
                    if (this.MergeConflict(diffsOther, diffCurrent, diffOther)) return true;
                    break;
                case (DiffMode.Update, DiffMode.Move):
                    if (this.MergeConflict(diffsCurrent, diffOther, diffCurrent)) return true;
                    break;
                case var value:
                    throw new NotSupportedException(value.ToString());
            }
        }

        return false;
    }

    private bool MergeConflict(List<Diff> diffsOther, Diff diffCurrent, Diff diffOther)
    {
        if (diffCurrent is IDiffChildren c1 && diffOther is IDiffChildren c2)
        {
            if (c1.Children != null && c2.Children != null)
            {
                if (this.DetectConflict(c1.Children, c2.Children))
                {
                    return true;
                }

                c1.Children.AddRange(c2.Children);
                diffsOther.Remove(diffOther);
            }
        }

        return false;
    }

    private void LogDiffs(string parentDiff, List<Diff> diffUsing)
    {
        foreach (var diff in diffUsing)
        {
            if (diff is IDiffChildren { Children: { } } parent && parent.Children.Any())
            {
                LogDiffs(parentDiff + " > " + parent, parent.Children);
            }
            else
            {
                this.logger.LogInformation("{diffUsing}", parentDiff + " > " + diff);
            }
        }
    }

    private CompilationUnitSyntax ParseFile(string file)
    {
        var fileContent = File.ReadAllText(file); 
        SyntaxTree tree = CSharpSyntaxTree.ParseText(fileContent);
        return tree.GetCompilationUnitRoot();
    }

    public string ProviderCode => "csharp";
}