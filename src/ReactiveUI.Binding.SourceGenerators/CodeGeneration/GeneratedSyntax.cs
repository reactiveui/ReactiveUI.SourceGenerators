// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>The fixed fragments of generated C# that more than one emitter writes, named once.</summary>
/// <remarks>
/// A fragment carries no indentation: the <see cref="SourceWriter"/> indents a line from its level when the
/// line's first character is written, so the same fragment reads correctly at any depth. Every member is a
/// <c>const</c>, so referencing it costs nothing at run time.
/// </remarks>
internal static class GeneratedSyntax
{
    /// <summary>Opens a delegate type, ready for its type arguments.</summary>
    internal const string FuncTypeOpen = $"{GeneratedTypeNames.Func}<";

    /// <summary>Opens a property-selector type, ready for its source and produced types.</summary>
    internal const string SelectorTypeOpen = $"{GeneratedTypeNames.Expression}<{GeneratedTypeNames.Func}<";

    /// <summary>Closes the cast of an observation callback's argument, ready for the property name.</summary>
    internal const string ObserverCastClose = ")__o).";
}
