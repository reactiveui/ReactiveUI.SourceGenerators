// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be <see langword="null"/> even if the corresponding type allows it.</summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class NotNullWhenAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="NotNullWhenAttribute"/> class.</summary>
    /// <param name="returnValue">The return value after which the parameter is not null.</param>
    public NotNullWhenAttribute(bool returnValue) =>
        ReturnValue = returnValue;

    /// <summary>Gets the return value after which the parameter is not null.</summary>
    public bool ReturnValue { get; }
}

#else
[assembly: TypeForwardedTo(typeof(NotNullWhenAttribute))]
#endif
