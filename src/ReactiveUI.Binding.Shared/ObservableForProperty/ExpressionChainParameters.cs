// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Linq.Expressions;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>How one chain of property accesses is to be observed.</summary>
/// <typeparam name="TSender">The root sender type surfaced on the emitted change.</typeparam>
/// <param name="Source">The root object of the chain.</param>
/// <param name="Expression">The full expression surfaced on the emitted change.</param>
/// <param name="Links">The member-access links of the chain, in order.</param>
/// <param name="BeforeChange">Whether values are observed before they change.</param>
/// <param name="SkipInitial">Whether the first value the chain produces is dropped.</param>
/// <param name="IsDistinct">Whether consecutive equal leaf values are suppressed.</param>
/// <param name="SuppressWarnings">Whether the warning a property with no notification mechanism raises is suppressed.</param>
[DebuggerDisplay("ExpressionChainParameters: ExpressionChain: {Expression}, BeforeChange = {BeforeChange}, Distinct = {IsDistinct}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct ExpressionChainParameters<TSender>(
    TSender? Source,
    Expression? Expression,
    Expression[] Links,
    bool BeforeChange,
    bool SkipInitial,
    bool IsDistinct,
    bool SuppressWarnings);
