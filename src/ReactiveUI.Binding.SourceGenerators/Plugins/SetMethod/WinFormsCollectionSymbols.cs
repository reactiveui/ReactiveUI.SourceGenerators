// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.SetMethod;

/// <summary>Validates WinForms collection mutation members from the consumer's symbols.</summary>
internal static class WinFormsCollectionSymbols
{
    /// <summary>The native collection setter's affinity.</summary>
    private const int CollectionAffinity = 10;

    /// <summary>Checks the element, collection and layout contracts before offering direct mutation.</summary>
    /// <param name="source">The incoming enumerable type.</param>
    /// <param name="target">The destination collection type.</param>
    /// <param name="collectionName">The exact native collection type.</param>
    /// <param name="layoutOwner">The property providing layout suspension.</param>
    /// <returns>The verified native setter, or null.</returns>
    internal static SetMethodInfo? Select(ITypeSymbol source, ITypeSymbol target, string collectionName, string layoutOwner)
    {
        if (target is not INamedTypeSymbol collection || !NativeTypeIdentity.Matches(target, collectionName) || !HasCollectionContract(collection, layoutOwner))
        {
            return null;
        }

        foreach (var contract in source.AllInterfaces)
        {
            if (contract.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
                && contract.TypeArguments[0] is INamedTypeSymbol { BaseType: { } elementBase }
                && PlatformSymbols.DerivesFrom(elementBase, "System.Windows.Forms.Control"))
            {
                return new(CollectionAffinity, layoutOwner) { SourceIsArray = source is IArrayTypeSymbol { IsSZArray: true } };
            }
        }

        return null;
    }

    /// <summary>Checks the public collection and layout operations used by generated code.</summary>
    /// <param name="collection">The native collection type.</param>
    /// <param name="layoutOwner">The property exposing its layout owner.</param>
    /// <returns>True when direct mutation is supported.</returns>
    internal static bool HasCollectionContract(INamedTypeSymbol collection, string layoutOwner) =>
        PlatformSymbols.FindMember(collection, layoutOwner) is IPropertySymbol { IsStatic: false, GetMethod.DeclaredAccessibility: Accessibility.Public, Type: INamedTypeSymbol owner }
        && HasMethod(owner, "SuspendLayout") && HasMethod(owner, "ResumeLayout")
        && HasMethod(collection, "Clear") && HasAddRange(collection);

    /// <summary>Checks for an accessible parameterless native operation.</summary>
    /// <param name="type">The declaring hierarchy.</param>
    /// <param name="name">The operation name.</param>
    /// <returns>True when the method can be called directly.</returns>
    private static bool HasMethod(INamedTypeSymbol type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers(name))
            {
                if (member is IMethodSymbol { IsStatic: false, DeclaredAccessibility: Accessibility.Public, Parameters.Length: 0 })
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Checks that AddRange accepts a concrete control array.</summary>
    /// <param name="type">The collection hierarchy.</param>
    /// <returns>True when the native array overload exists.</returns>
    private static bool HasAddRange(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers("AddRange"))
            {
                if (member is IMethodSymbol { IsStatic: false, DeclaredAccessibility: Accessibility.Public, Parameters.Length: 1 } method
                    && method.Parameters[0].Type is IArrayTypeSymbol array && NativeTypeIdentity.Matches(array.ElementType, "System.Windows.Forms.Control"))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
