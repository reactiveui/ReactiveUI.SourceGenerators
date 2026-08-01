// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Represents a person in the test data.</summary>
/// <seealso cref="ReactiveUI.Reactive.ReactiveObject" />
[ExcludeFromCodeCoverage]
[IReactiveObject]
public partial class Person
{
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Person"/> is deleted.
    /// </summary>
    /// <value>
    ///   <c>true</c> if deleted; otherwise, <c>false</c>.
    /// </value>
    [Reactive]
    public partial bool Deleted { get; set; }
}
