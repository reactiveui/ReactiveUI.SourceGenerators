// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>The metadata names of the attributes the generators read, and names the generated code uses.</summary>
/// <remarks>
/// The attributes are public types in the ReactiveUI.SourceGenerators assembly. The generators never
/// declare a type in a consumer, so no two assemblies that share internals can each hold a copy.
/// </remarks>
internal static class AttributeDefinitions
{
    /// <summary>The name of the ReactiveUI assembly.</summary>
    public const string ReactiveUI = "ReactiveUI";

    /// <summary>The metadata name of <c>[IReactiveObject]</c>.</summary>
    public const string ReactiveObjectAttributeType = "ReactiveUI.SourceGenerators.IReactiveObjectAttribute";

    /// <summary>The metadata name of <c>[ReactiveCommand]</c>.</summary>
    public const string ReactiveCommandAttributeType = "ReactiveUI.SourceGenerators.ReactiveCommandAttribute";

    /// <summary>The metadata name of <c>[Reactive]</c>.</summary>
    public const string ReactiveAttributeType = "ReactiveUI.SourceGenerators.ReactiveAttribute";

    /// <summary>The metadata name of <c>[ViewModelControlHost]</c>.</summary>
    public const string ViewModelControlHostAttributeType = "ReactiveUI.SourceGenerators.WinForms.ViewModelControlHostAttribute";

    /// <summary>The metadata name of <c>[RoutedControlHost]</c>.</summary>
    public const string RoutedControlHostAttributeType = "ReactiveUI.SourceGenerators.WinForms.RoutedControlHostAttribute";

    /// <summary>The metadata name of <c>[BindableDerivedList]</c>.</summary>
    public const string BindableDerivedListAttributeType = "ReactiveUI.SourceGenerators.BindableDerivedListAttribute";

    /// <summary>The metadata name of <c>[ReactiveCollection]</c>.</summary>
    public const string ReactiveCollectionAttributeType = "ReactiveUI.SourceGenerators.ReactiveCollectionAttribute";

    /// <summary>The attribute that excludes a generated control host from code coverage.</summary>
    public const string ExcludeFromCodeCoverage = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
}
