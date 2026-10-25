// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Covers a two-way binding whose writes wait on a sequencer that queues them, where the two sides start unequal.</summary>
[NotInParallel]
public class QueuedTwoWayBindingSettlingTests
{
    /// <summary>What the model starts with.</summary>
    private const string ModelValue = "Rent March";

    /// <summary>What the view starts with.</summary>
    private const string ViewValue = "";

    /// <summary>What a later view edit types.</summary>
    private const string ViewEdit = "typed by the user";

    /// <summary>What a later model edit sets.</summary>
    private const string ModelEdit = "Rent April";

    /// <summary>The amount a numeric binding is edited to.</summary>
    private const decimal Amount = 125.50M;

    /// <summary>The amount as the view shows it.</summary>
    private const string AmountText = "125.50";

    /// <summary>The marker the converting pair appends on the way to the view and strips on the way back.</summary>
    private const string Marker = "!";

    /// <summary>The most passes a binding may take to go idle.</summary>
    private const int PassLimit = 20;

    /// <summary>The per-test time limit, in milliseconds, that turns a hang into a failure.</summary>
    private const int TimeoutMilliseconds = 30_000;

    /// <summary>The entry point that creates a decimal-to-text binding.</summary>
    public enum NumericBindingKind
    {
        /// <summary>The model is the receiver, with a converter function each way.</summary>
        ModelFirstFunctions = 0,

        /// <summary>The model is the receiver, with a converter registration each way.</summary>
        ModelFirstConverters = 1,

        /// <summary>The view is the receiver, with a converter function each way.</summary>
        ViewFirstFunctions = 2,

        /// <summary>The view is the receiver, with a converter registration each way.</summary>
        ViewFirstConverters = 3,
    }

    /// <summary>The entry point that creates the binding.</summary>
    public enum BindingKind
    {
        /// <summary>The model is the receiver, as in <c>BindTwoWayUnsafe</c>, with no converters.</summary>
        ModelFirst = 0,

        /// <summary>The model is the receiver, as in <c>BindTwoWayUnsafe</c>, with a converter pair.</summary>
        ModelFirstConverting = 1,

        /// <summary>The view is the receiver, as in <c>BindUnsafe</c>, with a converter pair.</summary>
        ViewFirstConverting = 2,
    }

    /// <summary>Gets the ways a queued two-way binding can be made.</summary>
    /// <returns>Each entry point.</returns>
    public static IEnumerable<BindingKind> Bindings() =>
    [
        BindingKind.ModelFirst,
        BindingKind.ModelFirstConverting,
        BindingKind.ViewFirstConverting,
    ];

    /// <summary>Gets the ways a queued decimal-to-text binding can be made.</summary>
    /// <returns>Each entry point.</returns>
    public static IEnumerable<NumericBindingKind> NumericBindings() =>
    [
        NumericBindingKind.ModelFirstFunctions,
        NumericBindingKind.ModelFirstConverters,
        NumericBindingKind.ViewFirstFunctions,
        NumericBindingKind.ViewFirstConverters,
    ];

    /// <summary>A binding that starts with unequal sides goes idle within a bounded number of passes.</summary>
    /// <param name="kind">The entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [MethodDataSource(nameof(Bindings))]
    public async Task Bind_WithUnequalSides_GoesIdle(BindingKind kind, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var (_, _, sequencer, connection) = Connect(kind);
        using var owned = connection;

        var passes = sequencer.RunUntilIdle(PassLimit);

        await Assert.That(passes).IsNotEqualTo(-1);
    }

    /// <summary>The model's value reaches the view on subscribe and the model keeps it.</summary>
    /// <param name="kind">The entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [MethodDataSource(nameof(Bindings))]
    public async Task Bind_WithUnequalSides_CarriesTheModelToTheView(BindingKind kind, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var (model, view, sequencer, connection) = Connect(kind);
        using var owned = connection;

        _ = sequencer.RunUntilIdle(PassLimit);

        await Assert.That(model.Caption).IsEqualTo(ModelValue);
        await Assert.That(view.Caption).IsEqualTo(Show(kind, ModelValue));
    }

    /// <summary>A view edit after the binding settles reaches the model, and the binding goes idle again.</summary>
    /// <param name="kind">The entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [MethodDataSource(nameof(Bindings))]
    public async Task Bind_AfterSettling_CarriesAViewEditToTheModel(BindingKind kind, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var (model, view, sequencer, connection) = Connect(kind);
        using var owned = connection;
        _ = sequencer.RunUntilIdle(PassLimit);

        view.Caption = Show(kind, ViewEdit);
        var passes = sequencer.RunUntilIdle(PassLimit);

        await Assert.That(passes).IsNotEqualTo(-1);
        await Assert.That(model.Caption).IsEqualTo(ViewEdit);
        await Assert.That(view.Caption).IsEqualTo(Show(kind, ViewEdit));
    }

    /// <summary>A model edit after the binding settles reaches the view, and the binding goes idle again.</summary>
    /// <param name="kind">The entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [MethodDataSource(nameof(Bindings))]
    public async Task Bind_AfterSettling_CarriesAModelEditToTheView(BindingKind kind, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var (model, view, sequencer, connection) = Connect(kind);
        using var owned = connection;
        _ = sequencer.RunUntilIdle(PassLimit);

        model.Caption = ModelEdit;
        var passes = sequencer.RunUntilIdle(PassLimit);

        await Assert.That(passes).IsNotEqualTo(-1);
        await Assert.That(model.Caption).IsEqualTo(ModelEdit);
        await Assert.That(view.Caption).IsEqualTo(Show(kind, ModelEdit));
    }

    /// <summary>An edit made before the initial value is delivered goes idle on that edit.</summary>
    /// <param name="kind">The entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [MethodDataSource(nameof(NumericBindings))]
    public async Task Bind_WithAnEditBeforeTheFirstDelivery_GoesIdleOnTheEdit(NumericBindingKind kind, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var (viewModel, view, sequencer, connection) = ConnectAmounts(kind);
        using var owned = connection;

        viewModel.Amount = Amount;
        var passes = sequencer.RunUntilIdle(PassLimit);

        await Assert.That(passes).IsNotEqualTo(-1);
        await Assert.That(viewModel.Amount).IsEqualTo(Amount);
        await Assert.That(view.Text).IsEqualTo(AmountText);
    }

    /// <summary>Two edits made before anything is delivered leave both sides on the second.</summary>
    /// <param name="kind">The entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [MethodDataSource(nameof(NumericBindings))]
    public async Task Bind_WithABurstOfEdits_SettlesOnTheLatest(NumericBindingKind kind, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var (viewModel, view, sequencer, connection) = ConnectAmounts(kind);
        using var owned = connection;

        viewModel.Amount = 1M;
        viewModel.Amount = Amount;
        var passes = sequencer.RunUntilIdle(PassLimit);

        await Assert.That(passes).IsNotEqualTo(-1);
        await Assert.That(viewModel.Amount).IsEqualTo(Amount);
        await Assert.That(view.Text).IsEqualTo(AmountText);
    }

    /// <summary>Creates a decimal-to-text binding on a sequencer that queues.</summary>
    /// <param name="kind">The entry point to bind through.</param>
    /// <returns>The view model, the view, the sequencer, and the binding.</returns>
    private static (AmountViewModel ViewModel, AmountView View, ManualSequencer Sequencer, IDisposable Connection) ConnectAmounts(NumericBindingKind kind)
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var viewModel = new AmountViewModel();
        var view = new AmountView { ViewModel = viewModel };
        var sequencer = new ManualSequencer();

        var connection = kind switch
        {
            NumericBindingKind.ModelFirstFunctions => viewModel.BindTwoWayUnsafe(
                view,
                static vm => vm.Amount,
                static v => v.Text,
                FormatAmount,
                ParseAmount,
                sequencer),
            NumericBindingKind.ModelFirstConverters => viewModel.BindTwoWayUnsafe(
                view,
                static vm => vm.Amount,
                static v => v.Text,
                new AmountToTextConverter(),
                new TextToAmountConverter(),
                sequencer,
                null),
            NumericBindingKind.ViewFirstFunctions => view.BindUnsafe(
                viewModel,
                static vm => vm.Amount,
                static v => v.Text,
                FormatAmount,
                ParseAmount,
                sequencer),
            _ => view.BindUnsafe(
                viewModel,
                static vm => vm.Amount,
                static v => v.Text,
                new AmountToTextConverter(),
                new TextToAmountConverter(),
                sequencer,
                null),
        };

        return (viewModel, view, sequencer, connection);
    }

    /// <summary>Formats an amount as the view shows it.</summary>
    /// <param name="amount">The amount.</param>
    /// <returns>The text.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FormatAmount(decimal amount) => amount.ToString("F2", CultureInfo.InvariantCulture);

    /// <summary>Reads the amount a view's text holds.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The amount.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static decimal ParseAmount(string text) => decimal.Parse(text, CultureInfo.InvariantCulture);

    /// <summary>Renders a model value the way the view holds it.</summary>
    /// <param name="kind">The entry point that created the binding.</param>
    /// <param name="value">The model value.</param>
    /// <returns>The value the view holds.</returns>
    private static string Show(BindingKind kind, string value) => kind == BindingKind.ModelFirst ? value : value + Marker;

    /// <summary>Appends the marker the converting pair adds on the way to the view.</summary>
    /// <param name="value">The model value.</param>
    /// <returns>The view value.</returns>
    private static string Mark(string value) => value + Marker;

    /// <summary>Removes the marker the converting pair appends.</summary>
    /// <param name="value">The view value.</param>
    /// <returns>The model value.</returns>
    private static string Strip(string value) =>
        value.EndsWith(Marker, StringComparison.Ordinal) ? value[..^Marker.Length] : value;

    /// <summary>Creates a binding over a model and view that start unequal, on a sequencer that queues.</summary>
    /// <param name="kind">The entry point to bind through.</param>
    /// <returns>The model, the view, the sequencer, and the binding.</returns>
    private static (DispatchStubViewModel Model, DispatchStubView View, ManualSequencer Sequencer, IDisposable Connection) Connect(BindingKind kind)
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var model = new DispatchStubViewModel { Caption = ModelValue };
        var view = new DispatchStubView { Caption = ViewValue, ViewModel = model };
        var sequencer = new ManualSequencer();

        var connection = kind switch
        {
            BindingKind.ModelFirst => model.BindTwoWayUnsafe(view, static m => m.Caption, static v => v.Caption, sequencer),
            BindingKind.ModelFirstConverting => model.BindTwoWayUnsafe(
                view,
                static m => m.Caption,
                static v => v.Caption,
                Mark,
                Strip,
                sequencer),
            _ => view.BindUnsafe(
                model,
                static m => m.Caption,
                static v => v.Caption,
                Mark,
                Strip,
                sequencer),
        };

        return (model, view, sequencer, connection);
    }

    /// <summary>A converter registration from an amount to its text.</summary>
    private sealed class AmountToTextConverter : IBindingTypeConverter
    {
        /// <summary>An affinity high enough to outrank every registered converter.</summary>
        private const int WinningAffinity = 100;

        /// <inheritdoc/>
        public Type FromType => typeof(decimal);

        /// <inheritdoc/>
        public Type ToType => typeof(string);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects() => WinningAffinity;

        /// <inheritdoc/>
        public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
        {
            result = from is decimal amount ? FormatAmount(amount) : null;
            return result is not null;
        }
    }

    /// <summary>A converter registration from text to the amount it holds.</summary>
    private sealed class TextToAmountConverter : IBindingTypeConverter
    {
        /// <summary>An affinity high enough to outrank every registered converter.</summary>
        private const int WinningAffinity = 100;

        /// <inheritdoc/>
        public Type FromType => typeof(string);

        /// <inheritdoc/>
        public Type ToType => typeof(decimal);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects() => WinningAffinity;

        /// <inheritdoc/>
        public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
        {
            if (from is string text && decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                result = amount;
                return true;
            }

            result = null;
            return false;
        }
    }
}
