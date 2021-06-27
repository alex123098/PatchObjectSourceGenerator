using Microsoft.CodeAnalysis;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace PatchModel.Generator
{
    internal enum Nullability
    {
        None,
        NullableValueType,
        NullableReferenceType
    }

    internal static class NullabillityResolutionExtensions
    {
        private const string NullableAttributeName = "System.Runtime.CompilerServices.NullableAttribute";
        private const string NullableContextAttributeName = "System.Runtime.CompilerServices.NullableContextAttribute";

        public static Nullability GetNullability(this PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            if (!propertyType.IsValueType && IsOfNullableReferenceType(property))
            {
                return Nullability.NullableReferenceType;
            }
            if (propertyType.IsValueType && 
                propertyType.IsGenericType && 
                propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return Nullability.NullableValueType;
            }
            return Nullability.None;
        }

        public static Nullability GetNullability(this IPropertySymbol property)
        {
            var propertyType = (INamedTypeSymbol) property.Type;
            if (propertyType.IsValueType && 
                (propertyType.ConstructedFrom.SpecialType & SpecialType.System_Nullable_T) == SpecialType.System_Nullable_T)
            {
                return Nullability.NullableValueType;
            }
            if (!propertyType.IsValueType && 
                (propertyType.NullableAnnotation & NullableAnnotation.NotAnnotated) != NullableAnnotation.NotAnnotated)
            {
                return Nullability.NullableReferenceType;
            }
            return Nullability.None;
        }

        private static bool IsOfNullableReferenceType(PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            var ownerType = property.DeclaringType;
            var attributes = property.CustomAttributes;
            var nullableAttribute = attributes.FirstOrDefault(
                attr => attr.AttributeType.FullName == NullableAttributeName);
            if (nullableAttribute is { ConstructorArguments: { Count: 1} nullableArguments })
            {
                var nullableTraitParameter = nullableArguments[0];
                if (nullableTraitParameter.ArgumentType == typeof(byte[]))
                {
                    var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)nullableTraitParameter.Value;
                    if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                    {
                        return (byte)args[0].Value! != 1;
                    }
                }
                else if (nullableTraitParameter.ArgumentType == typeof(byte))
                {
                    return (byte)nullableTraitParameter.Value! != 1;
                }
            }

            while(ownerType is not null)
            {
                var contextAttribute = ownerType.CustomAttributes
                    .FirstOrDefault(attr => attr.AttributeType.FullName == NullableContextAttributeName);
                if (contextAttribute is { ConstructorArguments: {Count: 1 } contextArguments } && 
                    contextArguments[0].ArgumentType == typeof(byte))
                {
                    return (byte)contextArguments[0].Value! != 1;
                }
                ownerType = ownerType.DeclaringType;
            }

            return false;
        }
    }
}
