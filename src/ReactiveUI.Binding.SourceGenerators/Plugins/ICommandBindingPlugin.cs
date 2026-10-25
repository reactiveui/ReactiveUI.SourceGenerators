// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>
/// Compile-time plugin interface for command binding code generation.
/// Implementations determine how a command is bound to a control
/// (via Command property, event + Enabled, or event only).
/// </summary>
internal interface ICommandBindingPlugin
{
    /// <summary>Gets the affinity score for this plugin. Higher values win when multiple plugins can handle the same invocation.</summary>
    int Affinity { get; }

    /// <summary>
    /// Gets a value indicating whether the code generator should emit the custom binder
    /// fallback check before this plugin's binding code. Event-based plugins return
    /// <see langword="true"/> to allow runtime-registered custom binders to override;
    /// the <c>CommandProperty</c> plugin returns <see langword="false"/> because
    /// frameworks handle command binding internally.
    /// </summary>
    bool RequiresCustomBinderFallback { get; }

    /// <summary>Determines whether this plugin can handle the given BindCommand invocation.</summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <returns>True if this plugin can generate binding code for this invocation.</returns>
    bool CanHandle(BindCommandInvocationInfo inv);

    /// <summary>Emits the command binding code into the string builder.</summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain expression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+); emits nullable annotations on emitted code.</param>
    void EmitBinding(SourceWriter sb, BindCommandInvocationInfo inv, string controlAccess, bool supportsNullable);
}
