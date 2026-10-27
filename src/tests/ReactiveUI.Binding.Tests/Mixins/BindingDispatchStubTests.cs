// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Covers the binding overloads that exist only to be replaced by a generated one.</summary>
/// <remarks>
/// A generated overload wins overload resolution for every call site of its types, including the ones whose
/// lambdas it could not read. The unsuffixed overload it displaces therefore refuses, naming the
/// <c>Unsafe</c> twin that resolves the expression by reflection, so nothing a consumer calls by its plain
/// name carries <c>RequiresUnreferencedCode</c>. Both halves are reached on the declaring class rather than as
/// extension methods, because written as an extension call a generated dispatch wins and the stub is never
/// entered.
/// </remarks>
public class BindingDispatchStubTests
{
    /// <summary>The value the source carries into the binding.</summary>
    private const string BoundValue = "bound";

    /// <summary>The value written on the target side of a two-way binding.</summary>
    private const string ReverseValue = "reversed";

    /// <summary>The overload a refused BindTo points the caller at.</summary>
    private const string BindToTwin = "BindToUnsafe";

    /// <summary>The overload a refused command binding points the caller at.</summary>
    private const string BindCommandTwin = "BindCommandUnsafe";

    /// <summary>The converter a BindTo overload names, so the call reaches the overload taking one.</summary>
    private static readonly IBindingTypeConverter PassThrough =
        new StubBindingTypeConverter(typeof(string), typeof(string), static (from, _) => new(true, from));

    /// <summary>The source of a binding.</summary>
    private readonly DispatchStubViewModel _viewModel = new();

    /// <summary>The target of a binding.</summary>
    private readonly DispatchStubView _view = new();

    /// <summary>The stream a BindTo call writes from.</summary>
    private readonly ManualObservable<string> _source = new();

    /// <summary>A one-way binding with no generated overload runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithNoGeneratedOverload_BindsThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        _viewModel.Caption = BoundValue;

        using var binding = ReactiveUIBindingExtensions.BindOneWayUnsafe(
            _viewModel,
            _view,
            x => x.Caption,
            x => x.Caption);

        await Assert.That(_view.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>A converting one-way binding with no generated overload runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ConvertingWithNoGeneratedOverload_BindsThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        _viewModel.Caption = BoundValue;

        using var binding = ReactiveUIBindingExtensions.BindOneWayUnsafe(
            _viewModel,
            _view,
            x => x.Caption,
            x => x.Caption,
            static value => value);

        await Assert.That(_view.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>A two-way binding with no generated overload drives both sides through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_WithNoGeneratedOverload_BindsBothSidesThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        _viewModel.Caption = BoundValue;

        using var binding = ReactiveUIBindingExtensions.BindTwoWayUnsafe(
            _viewModel,
            _view,
            x => x.Caption,
            x => x.Caption);

        var afterBind = _view.Caption;
        _view.Caption = ReverseValue;

        await Assert.That(afterBind).IsEqualTo(BoundValue);
        await Assert.That(_viewModel.Caption).IsEqualTo(ReverseValue);
    }

    /// <summary>A converting two-way binding with no generated overload runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_ConvertingWithNoGeneratedOverload_BindsThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        _viewModel.Caption = BoundValue;

        using var binding = ReactiveUIBindingExtensions.BindTwoWayUnsafe(
            _viewModel,
            _view,
            x => x.Caption,
            x => x.Caption,
            static value => value,
            static value => value);

        await Assert.That(_view.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>The view-first spelling of a one-way binding runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_WithNoGeneratedOverload_BindsThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        _viewModel.Caption = BoundValue;

        using var binding = ReactiveUIBindingExtensions.OneWayBindUnsafe(
            _view,
            _viewModel,
            x => x.Caption,
            x => x.Caption);

        await Assert.That(_view.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>The converting view-first spelling of a one-way binding runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_ConvertingWithNoGeneratedOverload_BindsThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        _viewModel.Caption = BoundValue;

        using var binding = ReactiveUIBindingExtensions.OneWayBindUnsafe(
            _view,
            _viewModel,
            x => x.Caption,
            x => x.Caption,
            static value => value);

        await Assert.That(_view.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>The view-first spelling of a two-way binding runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_WithNoGeneratedOverload_BindsBothSidesThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        _viewModel.Caption = BoundValue;

        using var binding = ReactiveUIBindingExtensions.BindUnsafe(
            _view,
            _viewModel,
            x => x.Caption,
            x => x.Caption);

        var afterBind = _view.Caption;
        _view.Caption = ReverseValue;

        await Assert.That(afterBind).IsEqualTo(BoundValue);
        await Assert.That(_viewModel.Caption).IsEqualTo(ReverseValue);
    }

    /// <summary>The converting view-first spelling of a two-way binding runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_ConvertingWithNoGeneratedOverload_BindsThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        _viewModel.Caption = BoundValue;

        using var binding = ReactiveUIBindingExtensions.BindUnsafe(
            _view,
            _viewModel,
            x => x.Caption,
            x => x.Caption,
            static value => value,
            static value => value);

        await Assert.That(_view.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>Writing a stream into a property with no generated overload runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_WithNoGeneratedOverload_WritesThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        using var binding = ReactiveUIBindingExtensions.BindToUnsafe(_source, _view, x => x.Caption);
        _source.Observer!.OnNext(BoundValue);

        await Assert.That(_view.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>Writing a stream into a property with a conversion hint runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_WithAConversionHint_WritesThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        using var binding = ReactiveUIBindingExtensions.BindToUnsafe(_source, _view, x => x.Caption, conversionHint: null);
        _source.Observer!.OnNext(BoundValue);

        await Assert.That(_view.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>Writing a stream into a property with a converter runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_WithAConverter_WritesThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        using var binding = ReactiveUIBindingExtensions.BindToUnsafe(_source, _view, x => x.Caption, converterOverride: null);
        _source.Observer!.OnNext(BoundValue);

        await Assert.That(_view.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>Writing a stream into a property with both a hint and a converter runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_WithAConversionHintAndAConverter_WritesThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        using var binding = ReactiveUIBindingExtensions.BindToUnsafe(
            _source,
            _view,
            x => x.Caption,
            conversionHint: null,
            converterOverride: null);
        _source.Observer!.OnNext(BoundValue);

        await Assert.That(_view.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>A command binding with no generated overload runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithNoGeneratedOverload_BindsThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        using var binding = ReactiveUIBindingExtensions.BindCommandUnsafe(
            _view,
            _viewModel,
            x => x.Run,
            x => x.Control,
            null);

        await Assert.That(binding).IsNotNull();
    }

    /// <summary>A command binding taking its parameter from a stream runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithAParameterStream_BindsThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        using var binding = ReactiveUIBindingExtensions.BindCommandUnsafe(
            _view,
            _viewModel,
            x => x.Run,
            x => x.Control,
            _source,
            null);

        await Assert.That(binding).IsNotNull();
    }

    /// <summary>A command binding taking its parameter from a property runs through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithAParameterProperty_BindsThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        using var binding = ReactiveUIBindingExtensions.BindCommandUnsafe(
            _view,
            _viewModel,
            x => x.Run,
            x => x.Control,
            x => x.Parameter,
            null);

        await Assert.That(binding).IsNotNull();
    }

    /// <summary>An interaction binding handled asynchronously reaches the handler through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_WithAnAsynchronousHandler_RegistersThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var interaction = new Interaction<string, bool>();
        _viewModel.Confirm = interaction;

        using var binding = ReactiveUIBindingExtensions.BindInteractionUnsafe(
            _view,
            _viewModel,
            x => x.Confirm,
            static context =>
            {
                context.SetOutput(true);
                return Task.CompletedTask;
            });

        var handled = await interaction.Handle(BoundValue);

        await Assert.That(handled).IsTrue();
    }

    /// <summary>An interaction binding handled by a stream reaches the handler through the runtime engine.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_WithAStreamHandler_RegistersThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var interaction = new Interaction<string, bool>();
        _viewModel.Confirm = interaction;

        using var binding = ReactiveUIBindingExtensions.BindInteractionUnsafe(
            _view,
            _viewModel,
            x => x.Confirm,
            static IObservable<string> (context) =>
            {
                context.SetOutput(true);
                return ReactiveUI.Primitives.Signals.Signal.Empty<string>();
            });

        var handled = await interaction.Handle(BoundValue);

        await Assert.That(handled).IsTrue();
    }

    /// <summary>WhenChanged refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenChanged_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.WhenChanged(_viewModel, x => x.Caption),
            "WhenChangedUnsafe");

    /// <summary>WhenChanging refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenChanging_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.WhenChanging(_viewModel, x => x.Caption),
            "WhenChangingUnsafe");

    /// <summary>WhenAnyValue refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.WhenAnyValue(_viewModel, x => x.Caption),
            "WhenAnyValueUnsafe");

    /// <summary>The multi-property selector overload refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenChangedWithSelector_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.WhenChanged(
                _viewModel,
                x => x.Caption,
                x => x.Caption,
                static (first, second) => first + second),
            "WhenChangedUnsafe");

    /// <summary>BindOneWay refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindOneWay_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindOneWay(_viewModel, _view, x => x.Caption, x => x.Caption),
            "BindOneWayUnsafe");

    /// <summary>BindTwoWay refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTwoWay_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindTwoWay(_viewModel, _view, x => x.Caption, x => x.Caption),
            "BindTwoWayUnsafe");

    /// <summary>OneWayBind refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task OneWayBind_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.OneWayBind(_view, _viewModel, x => x.Caption, x => x.Caption),
            "OneWayBindUnsafe");

    /// <summary>Bind refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Bind_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.Bind(_view, _viewModel, x => x.Caption, x => x.Caption),
            "BindUnsafe");

    /// <summary>BindTo refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTo_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindTo(_source, _view, x => x.Caption),
            BindToTwin);

    /// <summary>BindCommand refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommand_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindCommand(_view, _viewModel, x => x.Run, x => x.Control),
            BindCommandTwin);

    /// <summary>BindInteraction refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindInteraction_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindInteraction(
                _view,
                _viewModel,
                x => x.Confirm,
                static context =>
                {
                    context.SetOutput(true);
                    return Task.CompletedTask;
                }),
            "BindInteractionUnsafe");

    /// <summary>InvokeCommand refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task InvokeCommand_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.InvokeCommand(_source, _viewModel, x => x.Run),
            "InvokeCommandUnsafe");

    /// <summary>A command binding executes with the value its parameter stream produced.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommandUnsafe_WithAParameterStream_ExecutesWithTheValueProduced()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var command = new RecordingStubCommand();
        _viewModel.Run = command;

        using var binding = ReactiveUIBindingExtensions.BindCommandUnsafe(
            _view,
            _viewModel,
            x => x.Run,
            x => x.Control,
            _source,
            null);

        _source.Observer?.OnNext(BoundValue);
        _view.Control.PerformClick();

        await Assert.That(command.LastParameter).IsEqualTo(BoundValue);
    }

    /// <summary>A command binding executes with the value its parameter property holds.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommandUnsafe_WithAParameterProperty_ExecutesWithTheValueHeld()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var command = new RecordingStubCommand();
        _viewModel.Run = command;
        _viewModel.Parameter = ReverseValue;

        using var binding = ReactiveUIBindingExtensions.BindCommandUnsafe(
            _view,
            _viewModel,
            x => x.Run,
            x => x.Control,
            x => x.Parameter,
            null);

        _view.Control.PerformClick();

        await Assert.That(command.LastParameter).IsEqualTo(ReverseValue);
    }

    /// <summary>A command binding named against an event reaches the command through that event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommandUnsafe_WithANamedEvent_ExecutesThroughThatEvent()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var command = new RecordingStubCommand();
        _viewModel.Run = command;

        using var binding = ReactiveUIBindingExtensions.BindCommandUnsafe(
            _view,
            _viewModel,
            x => x.Run,
            x => x.Control,
            _source,
            nameof(DispatchStubControl.Click));

        _source.Observer?.OnNext(BoundValue);
        _view.Control.PerformClick();

        await Assert.That(command.LastParameter).IsEqualTo(BoundValue);
    }

    /// <summary>A command binding with no view model has no parameter property to observe.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommandUnsafe_WithAParameterPropertyAndNoViewModel_BindsNothing()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var command = new RecordingStubCommand();
        _viewModel.Run = command;

        using var binding = ReactiveUIBindingExtensions.BindCommandUnsafe(
            _view,
            (DispatchStubViewModel?)null,
            x => x.Run,
            x => x.Control,
            x => x.Parameter,
            null);

        _view.Control.PerformClick();

        await Assert.That(command.LastParameter).IsNull();
    }

    /// <summary>WhenAny refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.WhenAny(_viewModel, x => x.Caption, static c => c.Value),
            "WhenAnyUnsafe");

    /// <summary>WhenAnyObservable refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.WhenAnyObservable(_viewModel, x => x.Signal!),
            "WhenAnyObservableUnsafe");

    /// <summary>The converting one-way overload refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindOneWayWithConversion_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindOneWay(
                _viewModel,
                _view,
                x => x.Caption,
                x => x.Caption,
                static value => value + ReverseValue),
            "BindOneWayUnsafe");

    /// <summary>The converting two-way overload refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTwoWayWithConversions_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindTwoWay(
                _viewModel,
                _view,
                x => x.Caption,
                x => x.Caption,
                static value => value + ReverseValue,
                static value => value + BoundValue),
            "BindTwoWayUnsafe");

    /// <summary>The selecting view binding refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task OneWayBindWithSelector_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.OneWayBind(
                _view,
                _viewModel,
                x => x.Caption,
                x => x.Caption,
                static value => value + ReverseValue),
            "OneWayBindUnsafe");

    /// <summary>The converting view binding refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindWithConverters_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.Bind(
                _view,
                _viewModel,
                x => x.Caption,
                x => x.Caption,
                static value => value + ReverseValue,
                static value => value + BoundValue),
            "BindUnsafe");

    /// <summary>BindTo with a conversion hint refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    /// <remarks>
    /// The hint is named rather than positional: it is typed <c>object</c>, so a string passed positionally
    /// binds to the plain overload's captured expression text instead.
    /// </remarks>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindToWithConversionHint_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindTo(_source, _view, x => x.Caption, conversionHint: BoundValue),
            BindToTwin);

    /// <summary>BindTo with a converter refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindToWithConverterOverride_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindTo(_source, _view, x => x.Caption, PassThrough),
            BindToTwin);

    /// <summary>BindTo with a hint and a converter refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindToWithHintAndConverter_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindTo(_source, _view, x => x.Caption, BoundValue, PassThrough),
            BindToTwin);

    /// <summary>A command binding taking a parameter stream refuses a call no dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandWithParameterStream_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindCommand(
                _view,
                _viewModel,
                x => x.Run,
                x => x.Control,
                _source),
            BindCommandTwin);

    /// <summary>A command binding taking a parameter property refuses a call no dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandWithParameterProperty_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindCommand(
                _view,
                _viewModel,
                x => x.Run,
                x => x.Control,
                x => x.Parameter),
            BindCommandTwin);

    /// <summary>An interaction binding handling with a stream refuses a call no dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindInteractionWithObservableHandler_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefusedNaming(
            () => ReactiveUIBindingExtensions.BindInteraction(
                _view,
                _viewModel,
                x => x.Confirm,
                context => _source),
            "BindInteractionUnsafe");

    /// <summary>Asserts that a call refuses and points the caller at the overload that resolves it.</summary>
    /// <param name="call">The call expected to refuse.</param>
    /// <param name="twin">The name the message has to offer.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertRefusedNaming(Func<object?> call, string twin)
    {
        var error = await Assert.That(call).Throws<InvalidOperationException>();

        await Assert.That(error!.Message).Contains(twin);
    }
}
