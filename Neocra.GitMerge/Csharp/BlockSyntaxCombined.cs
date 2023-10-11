using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp;

internal class BlockSyntaxCombined : IList<BlockSyntax, SyntaxList<StatementSyntax>, StatementSyntax>
{
    private readonly BlockSyntax blockSyntax;

    public BlockSyntaxCombined(BlockSyntax blockSyntax)
    {
        this.blockSyntax = blockSyntax;
    }

    public SyntaxList<StatementSyntax> Members => this.blockSyntax.Statements;
    public BlockSyntax WithMembers(SyntaxList<StatementSyntax> members)
    {
        return this.blockSyntax.WithStatements(members);
    }

    public BlockSyntax WithMembers(IEnumerable<StatementSyntax> members)
    {
        return this.blockSyntax.WithStatements(new SyntaxList<StatementSyntax>(members));
    }
}