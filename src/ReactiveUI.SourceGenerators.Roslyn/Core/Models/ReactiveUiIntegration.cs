// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Describes the ReactiveUI API surface referenced by a compilation.</summary>
/// <param name="Api">The implementation API selected by the compilation.</param>
/// <param name="IsNewerThan22">Whether the compilation references ReactiveUI 22 or later.</param>
/// <param name="ViewApi">The assembly declaring the <c>IViewFor</c> interfaces and the view locator.</param>
/// <param name="HasObservedProperty">Whether the view API's assembly has ReactiveUI.Binding's <c>ObservedProperty</c>.</param>
internal readonly record struct ReactiveUiIntegration(
    ReactiveUiApi Api,
    bool IsNewerThan22,
    ReactiveUiViewApi ViewApi = ReactiveUiViewApi.ReactiveUI,
    bool HasObservedProperty = false)
{
    /// <summary>Gets ReactiveUI.Binding's <c>ObservedProperty</c> in the view API's flavour.</summary>
    /// <remarks>Only meaningful when <see cref="HasObservedProperty"/> is set.</remarks>
    internal string ObservedProperty => $"{ViewNamespace}.ObservedProperty";

    /// <summary>Gets the namespace containing the selected ReactiveUI implementation types.</summary>
    internal string Namespace => Api == ReactiveUiApi.SystemReactive
        ? "global::ReactiveUI.Reactive"
        : "global::ReactiveUI";

    /// <summary>Gets the non-global namespace used in generated declarations that historically omitted the global alias qualifier.</summary>
    internal string DeclarationNamespace => Api == ReactiveUiApi.SystemReactive
        ? "ReactiveUI.Reactive"
        : "ReactiveUI";

    /// <summary>Gets the type used for an empty command input or output.</summary>
    internal string VoidTypeName => Api == ReactiveUiApi.Primitives
        ? "global::ReactiveUI.Primitives.RxVoid"
        : "global::System.Reactive.Unit";

    /// <summary>Gets the using directives needed for implementation types and the common interfaces.</summary>
    internal string UsingDirectives => Api == ReactiveUiApi.SystemReactive
        ? "using ReactiveUI;\nusing ReactiveUI.Reactive;"
        : "using ReactiveUI;";

    /// <summary>Gets the qualified namespace declaring <c>IViewFor</c>, <c>IViewLocator</c> and <c>ViewLocator</c>.</summary>
    internal string ViewNamespace => ViewApi switch
    {
        ReactiveUiViewApi.Binding => "global::ReactiveUI.Binding",
        ReactiveUiViewApi.BindingReactive => "global::ReactiveUI.Binding.Reactive",
        _ => "global::ReactiveUI",
    };

    /// <summary>Gets the expression for the application's current view locator.</summary>
    internal string CurrentViewLocator => ViewApi switch
    {
        ReactiveUiViewApi.Binding => "global::ReactiveUI.Binding.ViewLocator.GetCurrent()",
        ReactiveUiViewApi.BindingReactive => "global::ReactiveUI.Binding.Reactive.ViewLocator.GetCurrent()",
        _ => "global::ReactiveUI.ViewLocator.Current",
    };
}
