using Xunit;
using Xunit.Abstractions;

namespace PatchModel.Generator.Tests
{
    public class RecordPatchGeneratorTests : IClassFixture<GeneratorTestFixture>
    {
        private readonly GeneratorTestFixture fixture;

        public RecordPatchGeneratorTests(GeneratorTestFixture fixture, ITestOutputHelper outputHelper)
        {
            this.fixture = fixture;
            this.fixture.OutputHelper = outputHelper;
        }

        [Fact]
        public void CanAddPatcher()
        {
            const string sourceCode =
@"using PatchModel.Attributes;
namespace TestValueType
{
    public class TargetClass
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
    }

    [PatchesType(typeof(TargetClass))]
    public partial record SourceRecord(string StringValue, int IntValue);
}";
            const string expectedResult =
@"namespace TestValueType
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial record SourceRecord
    {
        public void Patch(global::TestValueType.TargetClass target)
        {
            target.StringValue = this.StringValue;
target.IntValue = this.IntValue;

        }
    }
}";

            var output = fixture.GetGeneratedCode(sourceCode);

            Assert.Equal(expectedResult, output);

        }

        [Fact]
        public void CanAddPatcherFromDifferentNamespace()
        {
            const string sourceCode =
@"using PatchModel.Attributes;
namespace TargetNamespace
{
    public class TargetClass
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
    }
}
namespace SourceNamespace
{
    using TargetNamespace;

    [PatchesType(typeof(TargetClass))]
    public partial record Source(string StringValue, int IntValue);
}";
            const string expectedResult =
@"namespace SourceNamespace
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial record Source
    {
        public void Patch(global::TargetNamespace.TargetClass target)
        {
            target.StringValue = this.StringValue;
target.IntValue = this.IntValue;

        }
    }
}";

            var output = fixture.GetGeneratedCode(sourceCode);

            Assert.Equal(expectedResult, output);
        }

        [Fact]
        public void CanAddPatcherWithNullableReferenceTypes()
        {
            const string sourceCode =
@"using PatchModel.Attributes;
#nullable enable

namespace TargetNamespace
{
    public class TargetClass
    {
        public string StringValue { get; set; }
        public string? OptionalValue { get; set; }
    }
}
namespace SourceNamespace
{
    using TargetNamespace;

    [PatchesType(typeof(TargetClass))]
    public partial record Source(string StringValue, string? OptionalValue);
}";
            const string expectedResult =
@"namespace SourceNamespace
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial record Source
    {
        public void Patch(global::TargetNamespace.TargetClass target)
        {
            target.StringValue = this.StringValue;
target.OptionalValue = this.OptionalValue;

        }
    }
}";

            var output = fixture.GetGeneratedCode(sourceCode);

            Assert.Equal(expectedResult, output);
        }

        [Fact]
        public void HandlesNullableReferenceTypesInSource()
        {
            const string sourceCode =
@"using PatchModel.Attributes;
#nullable enable

namespace TargetNamespace
{
    public class TargetClass
    {
        public string StringValue { get; set; }
        public string OptionalValue { get; set; }
    }
}
namespace SourceNamespace
{
    using TargetNamespace;

    [PatchesType(typeof(TargetClass))]
    public partial record Source(string StringValue, string? OptionalValue);
}";
            const string expectedResult =
    @"namespace SourceNamespace
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial record Source
    {
        public void Patch(global::TargetNamespace.TargetClass target)
        {
            target.StringValue = this.StringValue;
if (this.OptionalValue != null) target.OptionalValue = this.OptionalValue;

        }
    }
}";

            var output = fixture.GetGeneratedCode(sourceCode);

            Assert.Equal(expectedResult, output);
        }

        [Fact]
        public void HandlesNullableValueTypesInSource()
        {
            const string sourceCode =
@"using PatchModel.Attributes;
#nullable enable

namespace TargetNamespace
{
    public class TargetClass
    {
        public string StringValue { get; set; } = string.Empty;
        public int OptionalValue { get; set; }
    }
}
namespace SourceNamespace
{
    using TargetNamespace;

    [PatchesType(typeof(TargetClass))]
    public partial record Source(string StringValue, int? OptionalValue);
}";
            const string expectedResult =
    @"namespace SourceNamespace
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial record Source
    {
        public void Patch(global::TargetNamespace.TargetClass target)
        {
            target.StringValue = this.StringValue;
if (this.OptionalValue != null) target.OptionalValue = this.OptionalValue.Value;

        }
    }
}";

            var output = fixture.GetGeneratedCode(sourceCode);

            Assert.Equal(expectedResult, output);
        }
    }
}
