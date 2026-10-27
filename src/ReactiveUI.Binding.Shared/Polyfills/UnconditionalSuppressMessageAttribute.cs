// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET
using System.Diagnostics;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Suppresses reporting of a specific rule violation, allowing multiple suppressions on a single code artifact.</summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(
    AttributeTargets.All,
    Inherited = false,
    AllowMultiple = true)]
internal sealed class UnconditionalSuppressMessageAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="UnconditionalSuppressMessageAttribute"/> class.</summary>
    /// <param name="category">The category identifying the classification of the attribute.</param>
    /// <param name="checkId">The identifier of the analysis tool rule to be suppressed.</param>
    public UnconditionalSuppressMessageAttribute(string category, string checkId)
    {
        Category = category;
        CheckId = checkId;
    }

    /// <summary>Gets the category identifying the classification of the attribute.</summary>
    public string Category { get; }

    /// <summary>Gets the identifier of the analysis tool rule to be suppressed.</summary>
    public string CheckId { get; }

    /// <summary>Gets or sets the scope of the code that is relevant for the attribute.</summary>
    internal string? Scope { get; set; }

    /// <summary>Gets or sets a fully qualified path that represents the target of the attribute.</summary>
    internal string? Target { get; set; }

    /// <summary>Gets or sets an optional argument expanding on exclusion criteria.</summary>
    internal string? MessageId { get; set; }

    /// <summary>Gets or sets the justification for suppressing the code analysis message.</summary>
    internal string? Justification { get; set; }
}

#else
[assembly: TypeForwardedTo(typeof(UnconditionalSuppressMessageAttribute))]
#endif
