// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies the types of members that are dynamically accessed.
/// This enumeration has a <see cref="FlagsAttribute"/> attribute that allows a
/// bitwise combination of its member values.
/// </summary>
[Flags]
[SuppressMessage("Design", "SST2303:Flags enum members should be distinct bit values", Justification = "Mirrors the framework enum bit-for-bit, including its composite members.")]
[SuppressMessage(
    "Correctness",
    "SST2461:A combined flags value should only set bits the enum defines",
    Justification = "Bit 1 is only ever set as part of PublicConstructors, exactly as in the BCL type this mirrors."
                    + " Every value must stay numerically identical because the newer targets type-forward to the real"
                    + " enum, and naming that bit would invent a member the BCL does not have.")]
internal enum DynamicallyAccessedMemberTypes
{
    /// <summary>Specifies no members.</summary>
    None = 0,

    /// <summary>Specifies the default, parameterless public constructor.</summary>
    PublicParameterlessConstructor = 1 << 0,

    /// <summary>Specifies all public constructors.</summary>
    PublicConstructors = 0x0002 | PublicParameterlessConstructor,

    /// <summary>Specifies all non-public constructors.</summary>
    NonPublicConstructors = 1 << 2,

    /// <summary>Specifies all public methods.</summary>
    PublicMethods = 1 << 3,

    /// <summary>Specifies all non-public methods.</summary>
    NonPublicMethods = 1 << 4,

    /// <summary>Specifies all public fields.</summary>
    PublicFields = 1 << 5,

    /// <summary>Specifies all non-public fields.</summary>
    NonPublicFields = 1 << 6,

    /// <summary>Specifies all public nested types.</summary>
    PublicNestedTypes = 1 << 7,

    /// <summary>Specifies all non-public nested types.</summary>
    NonPublicNestedTypes = 1 << 8,

    /// <summary>Specifies all public properties.</summary>
    PublicProperties = 1 << 9,

    /// <summary>Specifies all non-public properties.</summary>
    NonPublicProperties = 1 << 10,

    /// <summary>Specifies all public events.</summary>
    PublicEvents = 1 << 11,

    /// <summary>Specifies all non-public events.</summary>
    NonPublicEvents = 1 << 12,

    /// <summary>Specifies all interfaces implemented by the type.</summary>
    Interfaces = 1 << 13,

    /// <summary>Specifies all members.</summary>
    All = ~None,
}

#else
[assembly: TypeForwardedTo(typeof(DynamicallyAccessedMemberTypes))]
#endif
