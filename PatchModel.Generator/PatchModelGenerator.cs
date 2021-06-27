using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PatchModel.Generator
{
    [Generator]
    public sealed class PatchModelGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(ctx => ctx.AddSource(
                Constants.PatchesTypeAttribute.Key, 
                Constants.PatchesTypeAttribute.SourceCode));
            context.RegisterForSyntaxNotifications(() => new PatchModeSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not PatchModeSyntaxReceiver receiver)
            {
                return;
            }
            foreach (var patcher in GetDeclaredPatchers(context, receiver))
            {
                var codeGemeratorResult = PatcherCodeGenerator.Generate(patcher);
                Console.WriteLine($"Added {codeGemeratorResult.Key}: ");
                Console.WriteLine(codeGemeratorResult.SourceCode);
                context.AddSource(codeGemeratorResult.Key, codeGemeratorResult.SourceCode);
            }
        }

        private IEnumerable<PatcherInfo> GetDeclaredPatchers(
            in GeneratorExecutionContext context, 
            PatchModeSyntaxReceiver receiver)
        {
            var compilation = context.Compilation;
            return receiver.PossiblePatchers
                .Select(decl => compilation.GetSemanticModel(decl.SyntaxTree).GetDeclaredSymbol(decl))
                .SelectMany(symbol => symbol?
                    .GetAttributes()
                    .Where(attr => attr.AttributeClass?.ToDisplayString() == Constants.PatchesTypeAttribute.QualifiedName)
                    .Select(attr => attr.ConstructorArguments.First().Value)
                    .OfType<INamedTypeSymbol>()
                    .Select(arg => new PatcherInfo((INamedTypeSymbol) symbol!, arg!)));
        }
    }
}
