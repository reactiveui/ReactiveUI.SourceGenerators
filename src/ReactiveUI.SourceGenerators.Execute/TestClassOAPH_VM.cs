// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>TestClassOAPH VM.</summary>
[ExcludeFromCodeCoverage]
public partial class TestClassOAPH_VM : ReactiveObject
{
    /// <summary>Stores the observable boolean field.</summary>
    [ObservableAsProperty]
    private bool _observableTestField;

    /// <summary>Stores the reactive boolean field.</summary>
    [Reactive]
    private bool _reactiveTestField;

    /// <summary>Stores the reactive string value.</summary>
    [Reactive]
    private string _value = string.Empty;

    /// <summary>Stores the nullable test property.</summary>
    [Reactive]
    private string? _testProperty;

    /// <summary>Initializes a new instance of the <see cref="TestClassOAPH_VM"/> class.</summary>
    public TestClassOAPH_VM()
    {
        _observableTestPropertyHelper = CreateObservableTestPropertyHelper();
        _observableTestFieldHelper = CreateObservableTestFieldHelper();
        _testHelper = CreateTestHelper();
        TestProperty = "Test2";
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

    /// <summary>Creates the helper that projects the reactive property to its observable counterpart.</summary>
    /// <returns>The initialized observable property helper.</returns>
    private ObservableAsPropertyHelper<bool> CreateObservableTestPropertyHelper() =>
        this.WhenAnyValue(x => x.ReactiveTestProperty).ToProperty(this, x => x.ObservableTestProperty);

    /// <summary>Creates the helper that projects the reactive field to its observable counterpart.</summary>
    /// <returns>The initialized observable field helper.</returns>
    private ObservableAsPropertyHelper<bool> CreateObservableTestFieldHelper() =>
        this.WhenAnyValue(x => x.ReactiveTestField).ToProperty(this, x => x.ObservableTestField);

    /// <summary>Creates the helper that projects the test property.</summary>
    /// <returns>The initialized test helper.</returns>
    private ObservableAsPropertyHelper<string?> CreateTestHelper() =>
        this.WhenAnyValue(x => x.TestProperty).ToProperty(this, x => x.Test);
}
