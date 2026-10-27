// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Indicates which members of a type are accessed dynamically.</summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(
    validOn: AttributeTargets.Class
             | AttributeTargets.Field
             | AttributeTargets.GenericParameter
             | AttributeTargets.Interface
             | AttributeTargets.Method
             | AttributeTargets.Parameter
             | AttributeTargets.Property
             | AttributeTargets.ReturnValue
             | AttributeTargets.Struct,
    Inherited = false)]
internal sealed class DynamicallyAccessedMembersAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="DynamicallyAccessedMembersAttribute"/> class.</summary>
    /// <param name="memberTypes">The kinds of members accessed dynamically.</param>
    public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes) =>
        MemberTypes = memberTypes;

    /// <summary>Gets the kinds of members accessed dynamically.</summary>
    public DynamicallyAccessedMemberTypes MemberTypes { get; }
}

#else
[assembly: TypeForwardedTo(typeof(DynamicallyAccessedMembersAttribute))]
#endif
