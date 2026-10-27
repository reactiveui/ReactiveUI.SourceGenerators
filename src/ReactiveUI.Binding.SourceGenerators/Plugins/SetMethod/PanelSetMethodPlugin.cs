// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.SetMethod;

/// <summary>Populates the control collection owned by a WinForms panel.</summary>
internal sealed class PanelSetMethodPlugin : ISetMethodPlugin
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SetMethodInfo? Select(ITypeSymbol source, ITypeSymbol target) =>
        WinFormsCollectionSymbols.Select(source, target, "System.Windows.Forms.Control.ControlCollection", "Owner");
}
