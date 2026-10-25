// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Registers an <see cref="IViewFor{T}"/> view in the generated view dispatch under a contract string,
/// so one view model can have a different view per contract.
/// </summary>
/// <param name="contract">The contract value for view resolution.</param>
[DebuggerDisplay("ViewContractAttribute: Contract = {Contract}")]
[AttributeUsage(AttributeTargets.Class)]
public sealed class ViewContractAttribute(string contract) : Attribute
{
    /// <summary>Gets the contract to use when resolving the view.</summary>
    public string Contract { get; } = contract;
}
