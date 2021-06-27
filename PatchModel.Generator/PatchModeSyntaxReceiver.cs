using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace PatchModel.Generator
{
    internal sealed class PatchModeSyntaxReceiver : ISyntaxReceiver
    {
        private readonly List<TypeDeclarationSyntax> patchers = new();

        public IReadOnlyList<TypeDeclarationSyntax> PossiblePatchers => patchers;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax { AttributeLists: { Count: > 0} attributes } declaration &&
                declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                patchers.Add(declaration);
            }
        }
    }
}
