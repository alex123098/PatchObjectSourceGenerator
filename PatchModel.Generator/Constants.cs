namespace PatchModel.Generator
{
    internal static class Constants
    {
        public static readonly DeclaredClass PatchesTypeAttribute = new(
            "PatchesTypeAttribute.g.cs",
            "PatchModel.Attributes.PatchesTypeAttribute",
@"namespace PatchModel.Attributes 
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    [System.Runtime.CompilerServices.CompilerGenerated]
    public sealed class PatchesTypeAttribute : System.Attribute
    {
        public System.Type TargetType { get; set; }

        public PatchesTypeAttribute(System.Type targetType)
        {
            TargetType = targetType;
        }
    }
}");
    }
}
