using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PatchModel.Generator
{
    internal static partial class PatcherCodeGenerator
    {
        private const string PatcherCodeTemplate =
@"namespace $$PATCHER_NAMESPACE$$
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    partial $$PATCHER_SYMBOL$$ $$PATCHER_NAME$$
    {
        public void Patch($$PATCHED_TYPE$$ target)
        {
            $$PATCH_METHOD_BODY$$
        }
    }
}";

        private const string NullableRefConditionTemplate =
@"if (this.$$SOURCE_PROPERTY$$ != null) target.$$TARGET_PROPERTY$$ = this.$$SOURCE_PROPERTY$$;";

        private const string NullableValConditionTemplate =
@"if (this.$$SOURCE_PROPERTY$$ != null) target.$$TARGET_PROPERTY$$ = this.$$SOURCE_PROPERTY$$.Value;";

        private const string UnconditionalTemplate = @"target.$$TARGET_PROPERTY$$ = this.$$SOURCE_PROPERTY$$;";

        public static DeclaredClass Generate(PatcherInfo patcher)
        {
            var sourceType = patcher.PatcherTypeSymbol;
            var namespaceName = sourceType.ContainingNamespace.ToString();
            var methodBody = GenerateMethodBody(patcher);
            var patcherSymbol = GetDeclaringSymbol(sourceType);
            var patcherTypeName = sourceType.Name;
            var patchedTypeName = patcher.PatchingObjectType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var classKey = $"{patcherTypeName}.Patch{patcher.PatchingObjectType.Name}.g.cs";

            return new DeclaredClass(
                classKey,
                sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                PatcherCodeTemplate
                    .Replace("$$PATCHER_NAMESPACE$$", namespaceName)
                    .Replace("$$PATCHER_SYMBOL$$", patcherSymbol)
                    .Replace("$$PATCHER_NAME$$", patcherTypeName)
                    .Replace("$$PATCHED_TYPE$$", patchedTypeName)
                    .Replace("$$PATCH_METHOD_BODY$$", methodBody));
        }

        private static string GetDeclaringSymbol(INamedTypeSymbol type)
        {
            return type switch
            {
                { IsValueType: true } => "struct",
                { IsRecord: true } => "record",
                _ => "class"
            };
        }

        private static string GenerateMethodBody(PatcherInfo patcher)
        {
            var targetProperties = ExtractProperties(patcher.PatchingObjectType, false);
            var sourceProperties = ExtractProperties(patcher.PatcherTypeSymbol, true);

            var propertyPairs = sourceProperties
                .Join(
                    targetProperties,
                    s => (s.Name, s.TypeName),
                    t => (t.Name, t.TypeName),
                    (source, target) => (source, target));
            var methodBodyBuilder = new StringBuilder();

            foreach (var (source, target) in propertyPairs)
            {
                GenerateAssignment(source, target, methodBodyBuilder);
            }
            return methodBodyBuilder.ToString();
        }

        private static void GenerateAssignment(ShortPropertyInfo source, ShortPropertyInfo target, StringBuilder builder)
        {
            var template = SelectAssignementTemplate(source, target);
            builder.AppendLine(template
                .Replace("$$SOURCE_PROPERTY$$", source.Name)
                .Replace("$$TARGET_PROPERTY$$", target.Name));
        }

        private static string SelectAssignementTemplate(ShortPropertyInfo source, ShortPropertyInfo target)
        {
            if (target.Nullability == Nullability.None)
            {
                return source.Nullability switch
                {
                    Nullability.NullableReferenceType => NullableRefConditionTemplate,
                    Nullability.NullableValueType => NullableValConditionTemplate,
                    _ => UnconditionalTemplate
                };
            }
            return UnconditionalTemplate;
        }

        private static IEnumerable<ShortPropertyInfo> ExtractProperties(INamedTypeSymbol type, bool source) 
            => type
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => (source && !p.IsWriteOnly || !source && !p.IsReadOnly) &&
                            p.DeclaredAccessibility == Accessibility.Public &&
                            !p.IsStatic &&
                            p.Type is INamedTypeSymbol)
                .Select(CreatePropertyInfo);

        private static ShortPropertyInfo CreatePropertyInfo(IPropertySymbol property)
        {
            var name = property.Name;

            var propertyType = (INamedTypeSymbol)property.Type;

            var nullability = property.GetNullability();
            var typeName = nullability == Nullability.NullableValueType
                ? propertyType.TypeArguments.First().ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                : propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            return new ShortPropertyInfo(name, typeName, nullability);
        }


        private sealed class ShortPropertyInfo
        {
            public string Name { get; }
            public string TypeName { get; }
            public Nullability Nullability { get; }

            public ShortPropertyInfo(string name, string typeName, Nullability nullability)
            {
                Name = name;
                TypeName = typeName;
                Nullability = nullability;
            }
        }
    }
}
