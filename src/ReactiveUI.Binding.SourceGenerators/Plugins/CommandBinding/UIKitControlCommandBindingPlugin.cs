// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Binds UIKit refresh controls and bar-button items through their native events.</summary>
internal sealed class UIKitControlCommandBindingPlugin : IPlatformCommandBindingPlugin
{
    /// <summary>The specialized UIKit control score.</summary>
    private const int ControlAffinity = 10;

    /// <inheritdoc/>
    public int Affinity => ControlAffinity;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => true;

    /// <inheritdoc/>
    public NativeCommandInfo? InspectControl(INamedTypeSymbol control) =>
        UIKitCommandSymbols.ControlEvent(control) is { } name
            ? NativeCommandSymbols.Event(control, NativeCommandKind.UIKitEvent, name)
            : null;

    /// <inheritdoc/>
    public bool CanHandle(BindCommandInvocationInfo inv) => !inv.HasExplicitEvent && inv.NativeCommand?.Kind == NativeCommandKind.UIKitEvent;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitBinding(SourceWriter sb, BindCommandInvocationInfo inv, string controlAccess, bool supportsNullable) =>
        NativeCommandEmitter.EmitEvent(sb, inv, controlAccess, supportsNullable);
}
