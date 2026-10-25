// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET9_0_OR_GREATER
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

/// <summary>Gives a member priority over lower-priority overloads that are also applicable to a call.</summary>
/// <remarks>The C# 13 compiler reads it by name, so an internal copy in the declaring assembly is enough.</remarks>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, Inherited = false)]
internal sealed class OverloadResolutionPriorityAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="OverloadResolutionPriorityAttribute"/> class.</summary>
    /// <param name="priority">The priority; higher wins over lower among applicable overloads.</param>
    public OverloadResolutionPriorityAttribute(int priority) => Priority = priority;

    /// <summary>Gets the priority; higher wins over lower among applicable overloads.</summary>
    public int Priority { get; }
}
#endif
