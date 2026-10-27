// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Shares native command member validation between generation and diagnostics.</summary>
internal static class NativeCommandMembers
{
    /// <summary>The parameter count of a native event delegate or UIKit target overload.</summary>
    private const int NativeParameterCount = 2;

    /// <summary>Checks a framework hierarchy by its declared CLR name.</summary>
    /// <param name="type">The concrete type.</param>
    /// <param name="name">The framework base type.</param>
    /// <returns>True when the type belongs to the hierarchy.</returns>
    internal static bool DerivesFrom(INamedTypeSymbol type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (NativeTypeIdentity.Matches(current, name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Finds a member through the base chain while respecting member hiding.</summary>
    /// <param name="type">The concrete type.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The nearest declaration.</returns>
    internal static ISymbol? FindMember(INamedTypeSymbol type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            var members = current.GetMembers(name);
            if (!members.IsEmpty)
            {
                return members[0];
            }
        }

        return null;
    }

    /// <summary>Checks for a public instance event with the native two-argument void delegate.</summary>
    /// <param name="type">The concrete owner.</param>
    /// <param name="name">The event name.</param>
    /// <returns>True when generated code can attach its handler.</returns>
    internal static bool HasEvent(INamedTypeSymbol type, string name) =>
        FindMember(type, name) is IEventSymbol { IsStatic: false, DeclaredAccessibility: Accessibility.Public, Type: INamedTypeSymbol delegateType }
        && delegateType.DelegateInvokeMethod is { ReturnsVoid: true, Parameters.Length: NativeParameterCount } invoke
        && invoke.Parameters[0].RefKind == RefKind.None && invoke.Parameters[1].RefKind == RefKind.None;

    /// <summary>Checks the public setter and exact type of a native property.</summary>
    /// <param name="type">The concrete owner.</param>
    /// <param name="name">The property name.</param>
    /// <param name="propertyType">The native property type.</param>
    /// <returns>True when the generated assignment is legal.</returns>
    internal static bool HasWritableProperty(INamedTypeSymbol type, string name, string propertyType) =>
        FindMember(type, name) is IPropertySymbol { IsStatic: false, SetMethod.DeclaredAccessibility: Accessibility.Public } property
        && (propertyType == "bool" ? property.Type.SpecialType == SpecialType.System_Boolean : NativeTypeIdentity.Matches(property.Type, propertyType));

    /// <summary>Checks UIKit's delegate-based target method overload.</summary>
    /// <param name="type">The concrete control.</param>
    /// <param name="name">The native method name.</param>
    /// <returns>True when the native overload is accessible.</returns>
    internal static bool HasTargetMethod(INamedTypeSymbol type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers(name))
            {
                if (member is IMethodSymbol { IsStatic: false, DeclaredAccessibility: Accessibility.Public, Parameters.Length: NativeParameterCount } method
                    && NativeTypeIdentity.Matches(method.Parameters[0].Type, "System.EventHandler")
                    && NativeTypeIdentity.Matches(method.Parameters[1].Type, "UIKit.UIControlEvent"))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
