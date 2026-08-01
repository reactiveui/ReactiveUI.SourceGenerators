// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Extension methods for the <see cref="Compilation"/> type.</summary>
internal static class CompilationExtensions
{
    /// <summary>Provides extension members for a compilation.</summary>
    /// <param name="compilation">The compilation to extend.</param>
    extension(Compilation compilation)
    {
        /// <summary>Checks whether a type with a specified metadata name is accessible from this compilation.</summary>
        /// <param name="fullyQualifiedMetadataName">The fully-qualified metadata type name to find.</param>
        /// <returns>Whether a type with the specified metadata name can be accessed from this compilation.</returns>
        internal bool HasAccessibleTypeWithMetadataName(string fullyQualifiedMetadataName)
        {
            var type = compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);

            if (type is not null)
            {
                return type.CanBeAccessedFrom(compilation.Assembly);
            }

            type = compilation.Assembly.GetTypeByMetadataName(fullyQualifiedMetadataName);

            if (type is not null)
            {
                return type.CanBeAccessedFrom(compilation.Assembly);
            }

            foreach (var module in compilation.Assembly.Modules)
            {
                foreach (var referencedAssembly in module.ReferencedAssemblySymbols)
                {
                    if (referencedAssembly.GetTypeByMetadataName(fullyQualifiedMetadataName) is not INamedTypeSymbol currentType)
                    {
                        continue;
                    }

                    switch (currentType.GetEffectiveAccessibility())
                    {
                        case Accessibility.Public:
                        case Accessibility.Internal when referencedAssembly.GivesAccessTo(compilation.Assembly):
                            return true;
                        default:
                            continue;
                    }
                }
            }

            return false;
        }
    }
}
