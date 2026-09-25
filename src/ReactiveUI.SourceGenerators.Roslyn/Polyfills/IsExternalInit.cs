// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

/// <summary>Reserved to be used by the compiler for tracking metadata. This class should not be used by developers in source code.</summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[SuppressMessage("Design", "SST1436:Empty type", Justification = "The compiler requires this exact empty type to emit init accessors on targets that do not ship it.")]
internal static class IsExternalInit;

#else
[assembly: TypeForwardedTo(typeof(IsExternalInit))]
#endif
