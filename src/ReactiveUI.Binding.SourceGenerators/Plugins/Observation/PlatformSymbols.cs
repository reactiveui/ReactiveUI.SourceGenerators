// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Provides symbol navigation shared by platform plugins.</summary>
internal static class PlatformSymbols
{
    /// <summary>The sender and event-arguments parameters of a native event delegate.</summary>
    private const int EventParameterCount = 2;

    /// <summary>Checks whether a type has any property eligible for a mechanism.</summary>
    /// <param name="info">The extracted type data.</param>
    /// <param name="kind">The mechanism identity.</param>
    /// <returns>True when at least one property offers that candidate.</returns>
    internal static bool HasCandidate(ClassBindingInfo info, string kind)
    {
        for (var i = 0; i < info.Properties.Length; i++)
        {
            if (Candidate(info, info.Properties[i].PropertyName, kind) is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Checks a framework base identity, including the type itself.</summary>
    /// <param name="type">The consumer type.</param>
    /// <param name="metadataName">The framework type name.</param>
    /// <returns>True when the type derives from the named framework type.</returns>
    internal static bool DerivesFrom(INamedTypeSymbol type, string metadataName)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (NativeTypeIdentity.Matches(current, metadataName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Checks a framework base identity, or an interface of that name the type implements.</summary>
    /// <param name="type">The consumer type.</param>
    /// <param name="metadataName">The framework type name.</param>
    /// <returns>True when the type derives from or implements the named framework type.</returns>
    /// <remarks>Uno declares <c>DependencyObject</c> as an interface on its non-Windows heads.</remarks>
    internal static bool DerivesFromOrImplements(INamedTypeSymbol type, string metadataName)
    {
        if (DerivesFrom(type, metadataName))
        {
            return true;
        }

        var interfaces = type.AllInterfaces;
        for (var i = 0; i < interfaces.Length; i++)
        {
            if (NativeTypeIdentity.Matches(interfaces[i], metadataName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Finds a member through the base chain, honoring member hiding.</summary>
    /// <param name="owner">The type exposing the member.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The nearest declaration, or null.</returns>
    internal static ISymbol? FindMember(INamedTypeSymbol owner, string name)
    {
        for (var type = owner; type is not null; type = type.BaseType)
        {
            var members = type.GetMembers(name);
            if (!members.IsEmpty)
            {
                return members[0];
            }
        }

        return null;
    }

    /// <summary>Accepts public instance events with two-argument void delegates.</summary>
    /// <param name="owner">The type exposing the event.</param>
    /// <param name="name">The event name.</param>
    /// <returns>Concrete event metadata, or null.</returns>
    internal static NotificationEventInfo? FindEvent(INamedTypeSymbol owner, string name) =>
        FindMember(owner, name) is IEventSymbol { IsStatic: false, DeclaredAccessibility: Accessibility.Public, Type: INamedTypeSymbol delegateType }
            && delegateType.DelegateInvokeMethod is { ReturnsVoid: true, Parameters.Length: EventParameterCount } invoke
            && invoke.Parameters[0].RefKind == RefKind.None && invoke.Parameters[1].RefKind == RefKind.None
            ? new(name, delegateType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            : null;

    /// <summary>Verifies the public native member backing a CLR dependency property.</summary>
    /// <param name="owner">The type exposing the member.</param>
    /// <param name="propertyName">The observed CLR property name.</param>
    /// <param name="metadataName">The framework dependency-property type.</param>
    /// <returns>True when a static field or readable property has the required type.</returns>
    internal static bool HasDependencyProperty(INamedTypeSymbol owner, string propertyName, string metadataName)
    {
        var type = FindMember(owner, $"{propertyName}Property") switch
        {
            IFieldSymbol { IsStatic: true, DeclaredAccessibility: Accessibility.Public } field => field.Type,
            IPropertySymbol { IsStatic: true, GetMethod.DeclaredAccessibility: Accessibility.Public } property => property.Type,
            _ => null,
        };
        return NativeTypeIdentity.Matches(type, metadataName);
    }

    /// <summary>Gets one plugin's verified candidate for a property.</summary>
    /// <param name="info">The type's extracted property data.</param>
    /// <param name="name">The property name.</param>
    /// <param name="kind">The plugin identity.</param>
    /// <returns>The matching candidate, or null.</returns>
    internal static PlatformObservationInfo? Candidate(ClassBindingInfo? info, string name, string kind)
    {
        var property = info is null ? null : ObservedProperties.Find(info, name);
        if (property is null)
        {
            return null;
        }

        for (var i = 0; i < property.PlatformObservations.Length; i++)
        {
            var candidate = property.PlatformObservations[i];
            if (candidate.Kind == kind)
            {
                return candidate;
            }
        }

        return null;
    }
}
