// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter may be <see langword="null"/> even if the corresponding type disallows it.</summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class MaybeNullWhenAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="MaybeNullWhenAttribute"/> class.</summary>
    /// <param name="returnValue">The return value after which the parameter may be null.</param>
    public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

    /// <summary>Gets the return value after which the parameter may be null.</summary>
    public bool ReturnValue { get; }
}

#else
[assembly: TypeForwardedTo(typeof(MaybeNullWhenAttribute))]
#endif
