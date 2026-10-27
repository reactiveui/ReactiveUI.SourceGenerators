// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

/// <summary>Names a compiler feature a consumer of the marked member has to understand.</summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="CompilerFeatureRequiredAttribute"/> class.</summary>
    /// <param name="featureName">The name of the required compiler feature.</param>
    public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;

    /// <summary>Gets the name of the required compiler feature.</summary>
    public string FeatureName { get; }

    /// <summary>Gets or sets a value indicating whether a compiler that does not understand the feature may ignore it.</summary>
    public bool IsOptional { get; set; }
}
