// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Binds Cocoa controls through their concrete Target, Action and optional Enabled properties.</summary>
internal sealed class AppKitCommandBindingPlugin : IPlatformCommandBindingPlugin
{
    /// <summary>The native Cocoa target/action score.</summary>
    private const int TargetActionAffinity = 4;

    /// <inheritdoc/>
    public int Affinity => TargetActionAffinity;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => true;

    /// <inheritdoc/>
    public NativeCommandInfo? InspectControl(INamedTypeSymbol control) => !AppKitCommandSymbols.CanBind(control)
        ? null
        : new(
                NativeCommandKind.AppKitTargetAction,
                null,
                null,
                NativeCommandSymbols.HasWritableProperty(control, "Enabled", "bool"),
                NativeCommandSymbols.HasWritableProperty(control, nameof(Action), "ObjCRuntime.Selector"));

    /// <inheritdoc/>
    public bool CanHandle(BindCommandInvocationInfo inv) => !inv.HasExplicitEvent && inv.NativeCommand?.Kind == NativeCommandKind.AppKitTargetAction;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitBinding(SourceWriter sb, BindCommandInvocationInfo inv, string controlAccess, bool supportsNullable) =>
        AppKitCommandEmitter.EmitBinding(sb, inv, controlAccess);
}
