// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestClassOAPH VM.
/// </summary>
public partial class TestClassOAPH_VM : ReactiveObject
{
    [ObservableAsProperty]
    private bool _observableTestField;

    [Reactive]
    private bool _reactiveTestField;

    [Reactive]
#pragma warning disable SX1309 // Field names should begin with underscore
    private string value = string.Empty;
#pragma warning restore SX1309 // Field names should begin with underscore

    /// <summary>
    /// Initializes a new instance of the <see cref="TestClassOAPH_VM"/> class.
    /// </summary>
    public TestClassOAPH_VM()
    {
        _observableTestPropertyHelper = this.WhenAnyValue(x => x.ReactiveTestProperty)
            .ToProperty(this, x => x.ObservableTestProperty);

        _observableTestFieldHelper = this.WhenAnyValue(x => x.ReactiveTestField)
            .ToProperty(this, x => x.ObservableTestField);
    }

    /// <summary>
    /// Gets a value indicating whether [observable test property].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [observable test property]; otherwise, <c>false</c>.
    /// </value>
    [ObservableAsProperty]
    public partial bool ObservableTestProperty { get; }

    /// <summary>
    /// Gets or sets a value indicating whether [reactive test property].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [reactive test property]; otherwise, <c>false</c>.
    /// </value>
    [Reactive]
    public partial bool ReactiveTestProperty { get; set; }
}
