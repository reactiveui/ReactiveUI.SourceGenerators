// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// Person.
/// </summary>
/// <seealso cref="ReactiveUI.ReactiveObject" />
public partial class Person : ReactiveObject
{
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Person"/> is deleted.
    /// </summary>
    /// <value>
    ///   <c>true</c> if deleted; otherwise, <c>false</c>.
    /// </value>
    [Reactive]
    public bool Deleted { get; set; }
}
