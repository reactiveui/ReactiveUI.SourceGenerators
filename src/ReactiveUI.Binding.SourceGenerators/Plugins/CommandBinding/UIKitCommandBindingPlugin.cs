// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Binds UIKit touch actions through the native AddTarget and RemoveTarget APIs.</summary>
internal sealed class UIKitCommandBindingPlugin : IPlatformCommandBindingPlugin
{
    /// <summary>The native UIControl command score.</summary>
    private const int TouchAffinity = 9;

    /// <inheritdoc/>
    public int Affinity => TouchAffinity;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => true;

    /// <inheritdoc/>
    public NativeCommandInfo? InspectControl(INamedTypeSymbol control) =>
        UIKitCommandSymbols.CanBindTouch(control)
            ? new(NativeCommandKind.UIKitTouch, null, null, true, false)
            : null;

    /// <inheritdoc/>
    public bool CanHandle(BindCommandInvocationInfo inv) => !inv.HasExplicitEvent && inv.NativeCommand?.Kind == NativeCommandKind.UIKitTouch;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitBinding(SourceWriter sb, BindCommandInvocationInfo inv, string controlAccess, bool supportsNullable) =>
        NativeCommandEmitter.EmitTouch(sb, inv, controlAccess);
}
