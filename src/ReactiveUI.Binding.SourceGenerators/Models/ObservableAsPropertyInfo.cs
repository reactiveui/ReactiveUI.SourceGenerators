// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>A partial property marked <c>[ObservableAsProperty]</c>, whose body the generator writes.</summary>
/// <param name="Declaration">The partial declarations that reach the property's type, outermost first.</param>
/// <param name="HintName">The generated file name for the property's type.</param>
/// <param name="Modifiers">The property's modifiers as declared, which the implementing part repeats.</param>
/// <param name="TypeFullName">The property type, fully qualified with its nullable annotation.</param>
/// <param name="PropertyName">The property name.</param>
/// <param name="HelperModifiers">The helper field's modifiers: its accessibility, then <c>readonly</c> when asked for.</param>
/// <param name="InitialValue">
/// The expression the property returns until its helper is assigned, or null for the type's default.
/// </param>
/// <param name="StoresInitialValue">
/// Whether the initial value is held in a backing field, evaluated once, rather than written into the getter.
/// </param>
/// <param name="Usings">
/// The <c>extern alias</c> and <c>using</c> directives of the file declaring the property, which an initial value
/// expression is written against. Empty when the property stores no initial value.
/// </param>
internal sealed record ObservableAsPropertyInfo(
    PartialTypeDeclaration Declaration,
    string HintName,
    string Modifiers,
    string TypeFullName,
    string PropertyName,
    string HelperModifiers,
    string? InitialValue,
    bool StoresInitialValue,
    EquatableArray<string> Usings);
