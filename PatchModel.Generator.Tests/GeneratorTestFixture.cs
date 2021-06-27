// Many thanks to Sergey Teplyakov and his awesome StructRecordsGenerator (https://github.com/SergeyTeplyakov/StructRecordsGenerator)
// for the idea on how to actually test source generators

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace PatchModel.Generator.Tests
{
    public sealed class GeneratorTestFixture
    {
        public ITestOutputHelper OutputHelper { get; set; } = new ConsoleOutputHelper();

        public string GetGeneratedCode(string source)
        {
            var compilation = Compile(source);

            AssertValid(compilation, beforeGenerator: true);

            var generator = new PatchModelGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var resultCompilation, out var diagnostics);

            var output = resultCompilation.SyntaxTrees.Last().ToString();
            OutputHelper.WriteLine(new string('*', 15));
            OutputHelper.WriteLine("Generated output:");
            OutputHelper.WriteLine(output);
            OutputHelper.WriteLine(new string('*', 15));
            OutputHelper.WriteLine(string.Empty);

            if (diagnostics.Length > 0)
            {
                OutputHelper.WriteLine(new string('*', 15));
                OutputHelper.WriteLine("Generated diagnostics:");
                OutputHelper.WriteLine(string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString())));

                var errorsAndWarnings = string.Join(
                    Environment.NewLine,
                    diagnostics.Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning));
                if (!string.IsNullOrEmpty(errorsAndWarnings))
                {
                    Assert.False(true, $"Unexpected diagnostics after source generation: {errorsAndWarnings}");
                }
                OutputHelper.WriteLine(new string('*', 15));
                OutputHelper.WriteLine(string.Empty);
            }

            AssertValid(resultCompilation, beforeGenerator: false);

            return output;
        }

        private static CSharpCompilation Compile(string source)
        {
            var ast = CSharpSyntaxTree.ParseText(source);
            var references = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(asm => !asm.IsDynamic && !string.IsNullOrEmpty(asm.Location))
                .Select(asm => MetadataReference.CreateFromFile(asm.Location))
                .ToArray();
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            return CSharpCompilation.Create("test_generated", new SyntaxTree[] { ast }, references, options);
        }

        private static void AssertValid(Compilation compilation, bool beforeGenerator)
        {
            var diagnostics = compilation
                .GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Where(d => d.Id != "CS0518");

            if (beforeGenerator) 
            {
                diagnostics = diagnostics.Where(d => d.Id != "CS0246" && d.GetMessage().Contains("The type or namespace \"PatchModel"));
            }

            var errorsString = string.Join(Environment.NewLine, diagnostics);
            if (!string.IsNullOrEmpty(errorsString))
            {
                var errorMessage = string.Concat(beforeGenerator ? "Original code is invalid" : "Generated code is invalid", " ", errorsString);
                Assert.False(true, errorMessage);
            }
        }
    }
}
