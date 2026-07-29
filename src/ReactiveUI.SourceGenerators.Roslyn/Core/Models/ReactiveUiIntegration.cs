// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Describes the ReactiveUI API surface referenced by a compilation.</summary>
/// <param name="Api">The implementation API selected by the compilation.</param>
/// <param name="IsNewerThan22">Whether the compilation references ReactiveUI 22 or later.</param>
internal readonly record struct ReactiveUiIntegration(ReactiveUiApi Api, bool IsNewerThan22)
{
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
}
