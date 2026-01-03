// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// Person.
/// </summary>
/// <seealso cref="ReactiveUI.ReactiveObject" />
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
