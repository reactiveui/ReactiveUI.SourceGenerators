// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Binds Android clicks and enabled state through concrete native members.</summary>
internal sealed class AndroidCommandBindingPlugin : IPlatformCommandBindingPlugin
{
    /// <summary>The Android View command score.</summary>
    private const int ClickAffinity = 9;

    /// <inheritdoc/>
    public int Affinity => ClickAffinity;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => true;

    /// <inheritdoc/>
    public NativeCommandInfo? InspectControl(INamedTypeSymbol control) =>
        PlatformSymbols.DerivesFrom(control, "Android.Views.View")
            ? NativeCommandSymbols.Event(control, NativeCommandKind.AndroidClick, "Click")
            : null;

    /// <inheritdoc/>
    public bool CanHandle(BindCommandInvocationInfo inv) => !inv.HasExplicitEvent && inv.NativeCommand?.Kind == NativeCommandKind.AndroidClick;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitBinding(SourceWriter sb, BindCommandInvocationInfo inv, string controlAccess, bool supportsNullable) =>
        NativeCommandEmitter.EmitEvent(sb, inv, controlAccess, supportsNullable);
}
