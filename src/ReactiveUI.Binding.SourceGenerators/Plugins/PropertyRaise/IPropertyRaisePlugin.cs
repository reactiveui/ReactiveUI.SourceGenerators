// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

/// <summary>A way for generated code to raise a type's change notifications for a property it backs.</summary>
/// <remarks>
/// <c>ToProperty</c> needs the object that declares the property to announce each new value. A type can only
/// raise its own events, so each plugin names a member that does it and that generated code can reach.
/// </remarks>
internal interface IPropertyRaisePlugin
{
    /// <summary>Gets the score that ranks this mechanism; the highest applicable score wins.</summary>
    int Affinity { get; }

    /// <summary>Describes how generated code raises the type's notifications, when this mechanism applies to it.</summary>
    /// <param name="type">The type that declares the property.</param>
    /// <param name="compilation">The consumer compilation, which the generated code is compiled into.</param>
    /// <returns>The raise description, or null when this mechanism cannot raise the type's notifications.</returns>
    PropertyRaiseInfo? Select(INamedTypeSymbol type, Compilation compilation);
}
