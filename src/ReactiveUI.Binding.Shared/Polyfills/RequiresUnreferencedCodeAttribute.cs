// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET
using System.Diagnostics;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Indicates that the method requires access to code that is not statically referenced.</summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(
    AttributeTargets.Method
    | AttributeTargets.Constructor
    | AttributeTargets.Class,
    Inherited = false)]
internal sealed class RequiresUnreferencedCodeAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="RequiresUnreferencedCodeAttribute"/> class with the specified message.</summary>
    /// <param name="message">A message that contains information about the usage of unreferenced code.</param>
    public RequiresUnreferencedCodeAttribute(string message) =>
        Message = message;

    /// <summary>Gets a message that contains information about the usage of unreferenced code.</summary>
    public string Message { get; }

    /// <summary>Gets or sets an optional URL with more information about the requirement.</summary>
    internal string? Url { get; set; }
}

#else
[assembly: TypeForwardedTo(typeof(RequiresUnreferencedCodeAttribute))]
#endif
