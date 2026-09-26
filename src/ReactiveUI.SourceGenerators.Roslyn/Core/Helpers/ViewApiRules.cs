// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
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
    /// <summary>The namespace of ReactiveUI's own view API, on releases not built on ReactiveUI.Binding.</summary>
    internal const string ReactiveUINamespace = "ReactiveUI";

    /// <summary>The namespace of the lean ReactiveUI.Binding.</summary>
    internal const string BindingNamespace = "ReactiveUI.Binding";

    /// <summary>The namespace of the System.Reactive ReactiveUI.Binding.</summary>
    internal const string BindingReactiveNamespace = "ReactiveUI.Binding.Reactive";

    /// <summary>The first ReactiveUI.Binding release with <c>ObservedProperty</c>.</summary>
    internal const string ObservedPropertyMinimumBindingVersion = "8.4.0";

    /// <summary>The metadata name of ReactiveUI's own <c>IViewFor&lt;T&gt;</c>.</summary>
    private const string ReactiveUIViewForMetadataName = $"{ReactiveUINamespace}.IViewFor`1";

    /// <summary>The metadata name of the lean ReactiveUI.Binding's <c>IViewFor&lt;T&gt;</c>.</summary>
    private const string BindingViewForMetadataName = $"{BindingNamespace}.IViewFor`1";

    /// <summary>The metadata name of the System.Reactive ReactiveUI.Binding's <c>IViewFor&lt;T&gt;</c>.</summary>
    private const string BindingReactiveViewForMetadataName = $"{BindingReactiveNamespace}.IViewFor`1";

    /// <summary>Gets the namespace declaring the <c>IViewFor</c> interfaces and the view locator a compilation uses.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The namespace, without the <c>global::</c> alias.</returns>
    /// <remarks>
    /// A ReactiveUI that declares its own <c>IViewFor&lt;T&gt;</c> is not built on ReactiveUI.Binding, so its interface
    /// wins even when ReactiveUI.Binding is referenced too. Otherwise the ReactiveUI.Binding flavour matching the
    /// ReactiveUI flavour is used: the System.Reactive one when <c>ReactiveUI.Reactive</c> is referenced or it is the only
    /// one present.
    /// </remarks>
    internal static string GetViewNamespace(Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName(ReactiveUIViewForMetadataName) is not null)
        {
            return ReactiveUINamespace;
        }

        var hasBinding = compilation.GetTypeByMetadataName(BindingViewForMetadataName) is not null;
        if (compilation.GetTypeByMetadataName(BindingReactiveViewForMetadataName) is not null
            && (!hasBinding || compilation.GetTypeByMetadataName("ReactiveUI.Reactive.ReactiveCommand") is not null))
        {
            return BindingReactiveNamespace;
        }

        return hasBinding ? BindingNamespace : ReactiveUINamespace;
    }

    /// <summary>Determines whether the view API a compilation uses has ReactiveUI.Binding's <c>ObservedProperty</c>.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>
    /// <see langword="true"/> when the compilation uses a ReactiveUI.Binding flavour at version
    /// <see cref="ObservedPropertyMinimumBindingVersion"/> or later.
    /// </returns>
    internal static bool HasObservedProperty(Compilation compilation)
    {
        var viewNamespace = GetViewNamespace(compilation);
        return viewNamespace != ReactiveUINamespace
            && compilation.GetTypeByMetadataName($"{viewNamespace}.ObservedProperty") is not null;
    }
}
