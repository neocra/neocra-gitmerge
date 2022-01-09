using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.Diffs
{

    public class MemberDeclarationDiff : Diff<MemberDeclarationSyntax>, IDiffChildren
    {
        public List<Diff>? Children { get; }

        public MemberDeclarationDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, MemberDeclarationSyntax value, List<Diff>? children = null) : base(mode, indexOfChild, moveIndexOfChild, value)
        {
            this.Children = children;
        }

        protected override string GetName()
        {
            return base.Value switch
            {
                ClassDeclarationSyntax c => c.Identifier.ValueText,
                NamespaceDeclarationSyntax { Name: IdentifierNameSyntax i } => i.Identifier.ValueText,
                NamespaceDeclarationSyntax { Name: QualifiedNameSyntax q } => q.ToString(),
                PropertyDeclarationSyntax p => p.Identifier.ValueText,
                _ => base.GetName()
            };
        }
    }
}