// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET7_0_OR_GREATER
using System.Diagnostics;

using Targets = System.AttributeTargets;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Indicates that the method requires generating code at run time.</summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(
    validOn: Targets.Method
             | Targets.Constructor
             | Targets.Class,
    Inherited = false)]
internal sealed class RequiresDynamicCodeAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="RequiresDynamicCodeAttribute"/> class with the specified message.</summary>
    /// <param name="message">A message that contains information about the usage of dynamic code.</param>
    public RequiresDynamicCodeAttribute(string message) =>
        Message = message;

    /// <summary>Gets or sets a value indicating whether the annotation should not apply to static members.</summary>
    public bool ExcludeStatics { get; set; }

    /// <summary>Gets a message that contains information about the usage of dynamic code.</summary>
    public string Message { get; }

    /// <summary>Gets or sets an optional URL with more information about the requirement.</summary>
    public string? Url { get; set; }
}
#else
[assembly: TypeForwardedTo(typeof(RequiresDynamicCodeAttribute))]
#endif
