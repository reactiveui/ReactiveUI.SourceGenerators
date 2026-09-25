// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Read-only properties that follow reactive properties.</summary>
/// <remarks>
/// <c>[ObservableAsProperty]</c> moved to ReactiveUI.Binding, so this sample writes each
/// <see cref="ObservableAsPropertyHelper{T}"/> by hand with ReactiveUI's <c>ToProperty</c>. On ReactiveUI.Binding, mark a
/// <c>partial</c> property <c>[ObservableAsProperty]</c> and the helper field is generated for you.
/// </remarks>
[ExcludeFromCodeCoverage]
public partial class TestClassOAPH_VM : ReactiveObject
{
    /// <summary>Backs <see cref="ObservableTestProperty"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _observableTestPropertyHelper;

    /// <summary>Backs <see cref="ObservableTestField"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _observableTestFieldHelper;

    /// <summary>Backs <see cref="Test"/>.</summary>
    private readonly ObservableAsPropertyHelper<string?> _testHelper;

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

    /// <summary>Gets a value indicating whether <see cref="ReactiveTestProperty"/> is set.</summary>
    public bool ObservableTestProperty => _observableTestPropertyHelper.Value;

    /// <summary>Gets a value indicating whether <see cref="ReactiveTestField"/> is set.</summary>
    public bool ObservableTestField => _observableTestFieldHelper.Value;

    /// <summary>
    /// Gets or sets a value indicating whether [reactive test property].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [reactive test property]; otherwise, <c>false</c>.
    /// </value>
    [Reactive]
    public partial bool ReactiveTestProperty { get; set; }

    /// <summary>Gets the latest <see cref="TestProperty"/>.</summary>
    public string? Test => _testHelper.Value;

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
