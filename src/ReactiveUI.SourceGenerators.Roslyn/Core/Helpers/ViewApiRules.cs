// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>The rules that pick the assembly whose view API, and property observation, generated hosts use.</summary>
/// <remarks>
/// The generators and the code-fix analyzers both compile this file, so a host the generator writes without
/// <c>ObservedProperty</c> is exactly the host the analyzer reports.
/// </remarks>
internal static class ViewApiRules
{
    /// <summary>The namespace of the lean ReactiveUI.Binding.</summary>
    internal const string BindingNamespace = "ReactiveUI.Binding";

    /// <summary>The namespace of the System.Reactive ReactiveUI.Binding.</summary>
    internal const string BindingReactiveNamespace = "ReactiveUI.Binding.Reactive";

    /// <summary>The first ReactiveUI.Binding release with <c>ObservedProperty</c>.</summary>
    internal const string ObservedPropertyMinimumBindingVersion = "8.4.0";

    /// <summary>The metadata name of the lean ReactiveUI.Binding's <c>IViewFor&lt;T&gt;</c>.</summary>
    private const string BindingViewForMetadataName = $"{BindingNamespace}.IViewFor`1";

    /// <summary>The metadata name of the System.Reactive ReactiveUI.Binding's <c>IViewFor&lt;T&gt;</c>.</summary>
    private const string BindingReactiveViewForMetadataName = $"{BindingReactiveNamespace}.IViewFor`1";

    /// <summary>Gets the namespace declaring the <c>IViewFor</c> interfaces and the view locator a compilation uses.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The namespace, without the <c>global::</c> alias.</returns>
    /// <remarks>
    /// The ReactiveUI.Binding flavour matching the ReactiveUI flavour is used: the System.Reactive one when
    /// <c>ReactiveUI.Reactive</c> is referenced or it is the only one present.
    /// </remarks>
    internal static string GetViewNamespace(Compilation compilation) =>
        compilation.GetTypeByMetadataName(BindingReactiveViewForMetadataName) is not null
        && (compilation.GetTypeByMetadataName(BindingViewForMetadataName) is null
            || compilation.GetTypeByMetadataName("ReactiveUI.Reactive.ReactiveCommand") is not null)
            ? BindingReactiveNamespace
            : BindingNamespace;

    /// <summary>Determines whether the view API a compilation uses has ReactiveUI.Binding's <c>ObservedProperty</c>.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>
    /// <see langword="true"/> when the compilation uses a ReactiveUI.Binding flavour at version
    /// <see cref="ObservedPropertyMinimumBindingVersion"/> or later.
    /// </returns>
    internal static bool HasObservedProperty(Compilation compilation) =>
        compilation.GetTypeByMetadataName($"{GetViewNamespace(compilation)}.ObservedProperty") is not null;
}
