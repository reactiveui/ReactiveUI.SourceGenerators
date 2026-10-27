// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Shares WinUI-compatible dependency-property symbol checks and callback-token emission.</summary>
internal static class DependencyPropertyObservationEmitter
{
    /// <summary>Creates the plugin for one framework's dependency objects.</summary>
    /// <param name="kind">The platform mechanism identity.</param>
    /// <param name="frameworkNamespace">The namespace declaring native dependency objects.</param>
    /// <returns>The plugin that observes that framework's dependency properties.</returns>
    internal static NativeObservationPlugin CreatePlugin(string kind, string frameworkNamespace) =>
        new(
            kind,
            BindingAffinity.WinUiDependencyObject,
            (owner, property) => Inspect(owner, property, kind, frameworkNamespace),
            AppendSubscription);

    /// <summary>Offers a native candidate only for a property with a concrete dependency-property member.</summary>
    /// <param name="owner">The concrete property owner.</param>
    /// <param name="property">The observed CLR property.</param>
    /// <param name="kind">The platform mechanism identity.</param>
    /// <param name="frameworkNamespace">The namespace declaring native dependency objects.</param>
    /// <returns>The eligible candidate, or null.</returns>
    internal static PlatformObservationInfo? Inspect(INamedTypeSymbol owner, IPropertySymbol property, string kind, string frameworkNamespace)
    {
        var dependencyObject = $"{frameworkNamespace}.DependencyObject";
        return PlatformSymbols.DerivesFromOrImplements(owner, dependencyObject)
            && PlatformSymbols.HasDependencyProperty(owner, property.Name, $"{frameworkNamespace}.DependencyProperty")
            ? new(kind, BindingAffinity.WinUiDependencyObject, default, null, $"global::{dependencyObject}", null)
            : null;
    }

    /// <summary>Pairs a native callback token with deterministic unregistration.</summary>
    /// <param name="sb">The writer, inside the subscription callback.</param>
    /// <param name="segment">The concrete observed property.</param>
    /// <param name="info">The verified native mechanism.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendSubscription(SourceWriter sb, PropertyPathSegment segment, PlatformObservationInfo info) =>
        sb.BeginVar("__token").Append("__source.RegisterPropertyChangedCallback(")
            .Append(segment.DeclaringTypeFullName).Append('.').Append(segment.PropertyName)
            .Line("Property, (__sender, __property) => __notify());")
            .Line($"return new {GeneratedTypeNames.ActionDisposable}(() =>")
            .Indent()
            .Append("__source.UnregisterPropertyChangedCallback(").Append(segment.DeclaringTypeFullName)
            .Append('.').Append(segment.PropertyName).Line("Property, __token));")
            .Outdent();
}
