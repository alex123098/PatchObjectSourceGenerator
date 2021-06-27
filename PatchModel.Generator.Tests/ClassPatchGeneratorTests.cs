using Xunit;
using Xunit.Abstractions;

namespace PatchModel.Generator.Tests
{
    public class ClassPatchGeneratorTests : IClassFixture<GeneratorTestFixture>
    {
        private readonly GeneratorTestFixture fixture;

        public ClassPatchGeneratorTests(GeneratorTestFixture fixture, ITestOutputHelper outputHelper)
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
    public struct TargetStruct
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
    }

    [PatchesType(typeof(TargetStruct))]
    public partial class SourceStruct
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
    }
}";
            const string expectedResult =
@"namespace TestValueType
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial class SourceStruct
    {
        public void Patch(global::TestValueType.TargetStruct target)
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
    public class TargetStruct
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
    }
}
namespace SourceNamespace
{
    using TargetNamespace;

    [PatchesType(typeof(TargetStruct))]
    public partial class SourceStruct
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
    }
}";
            const string expectedResult =
@"namespace SourceNamespace
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial class SourceStruct
    {
        public void Patch(global::TargetNamespace.TargetStruct target)
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
    public struct TargetStruct
    {
        public string StringValue { get; set; }
        public string? OptionalValue { get; set; }
    }
}
namespace SourceNamespace
{
    using TargetNamespace;

    [PatchesType(typeof(TargetStruct))]
    public partial class SourceStruct
    {
        public string StringValue { get; set; } = string.Empty;
        public string? OptionalValue { get; set; }
    }
}";
            const string expectedResult =
@"namespace SourceNamespace
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial class SourceStruct
    {
        public void Patch(global::TargetNamespace.TargetStruct target)
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
    public struct TargetStruct
    {
        public string StringValue { get; set; }
        public string OptionalValue { get; set; }
    }
}
namespace SourceNamespace
{
    using TargetNamespace;

    [PatchesType(typeof(TargetStruct))]
    public partial class SourceStruct
    {
        public string StringValue { get; set; } = string.Empty;
        public string? OptionalValue { get; set; }
    }
}";
            const string expectedResult =
    @"namespace SourceNamespace
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial class SourceStruct
    {
        public void Patch(global::TargetNamespace.TargetStruct target)
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
    public class TargetStruct
    {
        public string StringValue { get; set; } = string.Empty;
        public int OptionalValue { get; set; }
    }
}
namespace SourceNamespace
{
    using TargetNamespace;

    [PatchesType(typeof(TargetStruct))]
    public partial class SourceStruct
    {
        public string StringValue { get; set; } = string.Empty;
        public int? OptionalValue { get; set; }
    }
}";
            const string expectedResult =
    @"namespace SourceNamespace
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial class SourceStruct
    {
        public void Patch(global::TargetNamespace.TargetStruct target)
        {
            target.StringValue = this.StringValue;
if (this.OptionalValue != null) target.OptionalValue = this.OptionalValue.Value;

        }
    }
}";

            var output = fixture.GetGeneratedCode(sourceCode);

            Assert.Equal(expectedResult, output);
        }

        [Fact]
        public void IgnoresReadOnlyTargetProperties()
        {
            const string sourceCode = @"using PatchModel.Attributes;
namespace TargetNamespace
{
    public class TargetStruct
    {
        public string StringValue { get; } = string.Empty;
        public int OptionalValue { get; set; }
    }
}
namespace SourceNamespace
{
    using TargetNamespace;

    [PatchesType(typeof(TargetStruct))]
    public partial class SourceStruct
    {
        public string StringValue { get; set; } = string.Empty;
        public int OptionalValue { get; set; }
    }
}";
            const string expectedResult =
    @"namespace SourceNamespace
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial class SourceStruct
    {
        public void Patch(global::TargetNamespace.TargetStruct target)
        {
            target.OptionalValue = this.OptionalValue;

        }
    }
}";
            var output = fixture.GetGeneratedCode(sourceCode);

            Assert.Equal(expectedResult, output);
        }
    }
}
