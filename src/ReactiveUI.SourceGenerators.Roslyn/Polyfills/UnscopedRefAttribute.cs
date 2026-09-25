// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET7_0_OR_GREATER
using System.Diagnostics;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Lets a <see langword="ref"/> to a member of <see langword="this"/>, or a <see langword="ref"/> parameter, escape the method.</summary>
/// <remarks>
/// Polyfill for the targets that predate <c>UnscopedRefAttribute</c>. The compiler honours the attribute by name, so
/// the internal copy has the same effect as the framework type.
/// </remarks>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class UnscopedRefAttribute : Attribute;

#else
[assembly: TypeForwardedTo(typeof(UnscopedRefAttribute))]
#endif
