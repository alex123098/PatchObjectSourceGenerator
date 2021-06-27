namespace PatchModel.Generator
{
    internal sealed class DeclaredClass
    {
        public string Key { get; }
        public string QualifiedName { get; }
        public string SourceCode { get; }

        public DeclaredClass(string key, string qualifiedName, string sourceCode)
        {
            Key = key;
            QualifiedName = qualifiedName;
            SourceCode = sourceCode;
        }
    }
}
