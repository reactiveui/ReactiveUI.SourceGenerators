// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestClassOAPH VM.
/// </summary>
[ExcludeFromCodeCoverage]
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

    [Reactive]
    private string? _testProperty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestClassOAPH_VM"/> class.
    /// </summary>
    public TestClassOAPH_VM()
    {
        _observableTestPropertyHelper = this.WhenAnyValue(x => x.ReactiveTestProperty)
            .ToProperty(this, x => x.ObservableTestProperty);

        _observableTestFieldHelper = this.WhenAnyValue(x => x.ReactiveTestField)
            .ToProperty(this, x => x.ObservableTestField);

        _testHelper = this.WhenAnyValue(x => x.TestProperty).ToProperty(this, x => x.Test);

        TestProperty = null;
        var t0 = Test;

        TestProperty = "Test";

        var t1 = Test;

        TestProperty = null;
        var t2 = Test;

        TestProperty = "Test2";
        var t3 = Test;
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

    /// <summary>
    /// Gets the test.
    /// </summary>
    /// <value>
    /// The test.
    /// </value>
    [ObservableAsProperty]
    public partial string? Test { get; }
}
