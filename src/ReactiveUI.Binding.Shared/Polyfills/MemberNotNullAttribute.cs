// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET

using Targets = System.AttributeTargets;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Specifies that the method or property will ensure that the listed field and property members have not-<see langword="null"/> values.</summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(
    validOn: Targets.Method
             | Targets.Property,
    Inherited = false,
    AllowMultiple = true)]
internal sealed class MemberNotNullAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="MemberNotNullAttribute"/> class.</summary>
    /// <param name="members">Field or property member names.</param>
    public MemberNotNullAttribute(params string[] members) =>
        Members = members;

    /// <summary>Gets field or property member names.</summary>
    public string[] Members { get; }
}

#else
[assembly: TypeForwardedTo(typeof(MemberNotNullAttribute))]
#endif
