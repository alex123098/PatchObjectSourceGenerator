using Microsoft.CodeAnalysis;
using System;

namespace PatchModel.Generator
{
    internal sealed class PatcherInfo
    {
        public INamedTypeSymbol PatcherTypeSymbol { get; }
        public INamedTypeSymbol PatchingObjectType { get; }

        public PatcherInfo(INamedTypeSymbol patcherTypeSymbol, INamedTypeSymbol patchingObjectType)
        {
            PatcherTypeSymbol = patcherTypeSymbol;
            PatchingObjectType = patchingObjectType;
        }
    }
}
