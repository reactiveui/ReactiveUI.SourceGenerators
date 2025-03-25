// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
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
