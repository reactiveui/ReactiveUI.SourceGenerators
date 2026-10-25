// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.Fallback;
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Covers every scheduler overload that resolves its expression by reflection.</summary>
/// <remarks>
/// The scheduler surface is the one place where the converter-taking overloads had no caller, so each shape is
/// exercised here rather than left to the shapes a consumer happens to reach for. The file the methods live in
/// sits inside a type marked <c>ExcludeFromCodeCoverage</c>, so these assertions are the only record that the
/// overloads work.
/// </remarks>
[NotInParallel]
public class SchedulerUnsafeBindingTests
{
    /// <summary>The value carried across a binding.</summary>
    private const string BoundValue = "bound";

    /// <summary>The value written back on the target side of a two-way binding.</summary>
    private const string ReverseValue = "reversed";

    /// <summary>The scheduler these overloads exist to take, left unset so writes stay on the calling thread.</summary>
    private const ISequencer? OnThisThread = null;

    /// <summary>A one-way scheduled binding carries the source value to the target.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWayUnsafe_CarriesTheSourceValue()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var source = new DispatchStubViewModel { Caption = BoundValue };
        var target = new DispatchStubView();

        using var binding = source.BindOneWayUnsafe(target, s => s.Caption, t => t.Caption, OnThisThread);

        await Assert.That(target.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>A one-way scheduled binding applies the conversion it is given.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWayUnsafe_WithAConversion_AppliesIt()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var source = new DispatchStubViewModel { Caption = BoundValue };
        var target = new DispatchStubView();

        using var binding = source.BindOneWayUnsafe(
            target,
            s => s.Caption,
            t => t.Caption,
            static value => $"{value}!",
            OnThisThread);

        await Assert.That(target.Caption).IsEqualTo($"{BoundValue}!");
    }

    /// <summary>A one-way scheduled binding applies the converter it is given.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWayUnsafe_WithAConverter_AppliesIt()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var source = new DispatchStubViewModel { Caption = BoundValue };
        var target = new DispatchStubView();

        using var binding = source.BindOneWayUnsafe(
            target,
            s => s.Caption,
            t => t.Caption,
            new SuffixConverter(),
            OnThisThread,
            null);

        await Assert.That(target.Caption).IsEqualTo($"{BoundValue}{SuffixConverter.Suffix}");
    }

    /// <summary>A two-way scheduled binding carries a value in both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWayUnsafe_CarriesBothDirections()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var source = new DispatchStubViewModel { Caption = BoundValue };
        var target = new DispatchStubView();

        using var binding = source.BindTwoWayUnsafe(target, s => s.Caption, t => t.Caption, OnThisThread);
        var afterBind = target.Caption;
        target.Caption = ReverseValue;

        await Assert.That(afterBind).IsEqualTo(BoundValue);
        await Assert.That(source.Caption).IsEqualTo(ReverseValue);
    }

    /// <summary>A two-way scheduled binding applies the conversions it is given.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWayUnsafe_WithConversions_AppliesThem()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var source = new DispatchStubViewModel { Caption = BoundValue };
        var target = new DispatchStubView();

        using var binding = source.BindTwoWayUnsafe(
            target,
            s => s.Caption,
            t => t.Caption,
            static value => value,
            static value => value,
            OnThisThread);

        await Assert.That(target.Caption).IsEqualTo(BoundValue);
    }

    /// <summary>A two-way scheduled binding applies the converters it is given.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWayUnsafe_WithConverters_AppliesThem()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var source = new DispatchStubViewModel { Caption = BoundValue };
        var target = new DispatchStubView();

        using var binding = source.BindTwoWayUnsafe(
            target,
            s => s.Caption,
            t => t.Caption,
            new SuffixConverter(),
            new SuffixConverter(),
            OnThisThread,
            null);

        await Assert.That(target.Caption).IsEqualTo($"{BoundValue}{SuffixConverter.Suffix}");
    }

    /// <summary>A view-first scheduled one-way binding projects through its selector.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBindUnsafe_WithASelector_ProjectsThroughIt()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var viewModel = new DispatchStubViewModel { Caption = BoundValue };
        var view = new DispatchStubView { ViewModel = viewModel };

        using var binding = view.OneWayBindUnsafe(
            viewModel,
            vm => vm.Caption,
            v => v.Caption,
            static value => $"{value}!",
            OnThisThread);

        await Assert.That(view.Caption).IsEqualTo($"{BoundValue}!");
    }

    /// <summary>A view-first scheduled one-way binding applies the converter it is given.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBindUnsafe_WithAConverter_AppliesIt()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var viewModel = new DispatchStubViewModel { Caption = BoundValue };
        var view = new DispatchStubView { ViewModel = viewModel };

        using var binding = view.OneWayBindUnsafe(
            viewModel,
            vm => vm.Caption,
            v => v.Caption,
            new SuffixConverter(),
            OnThisThread,
            null);

        await Assert.That(view.Caption).IsEqualTo($"{BoundValue}{SuffixConverter.Suffix}");
    }

    /// <summary>A view-first scheduled two-way binding carries a value in both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindUnsafe_CarriesBothDirections()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var viewModel = new DispatchStubViewModel { Caption = BoundValue };
        var view = new DispatchStubView { ViewModel = viewModel };

        using var binding = view.BindUnsafe(
            viewModel,
            vm => vm.Caption,
            v => v.Caption,
            static value => value,
            static value => value,
            OnThisThread);

        var afterBind = view.Caption;
        view.Caption = ReverseValue;

        await Assert.That(afterBind).IsEqualTo(BoundValue);
        await Assert.That(viewModel.Caption).IsEqualTo(ReverseValue);
    }

    /// <summary>A view-first scheduled two-way binding applies the converters it is given.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindUnsafe_WithConverters_AppliesThem()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var viewModel = new DispatchStubViewModel { Caption = BoundValue };
        var view = new DispatchStubView { ViewModel = viewModel };

        using var binding = view.BindUnsafe(
            viewModel,
            vm => vm.Caption,
            v => v.Caption,
            new SuffixConverter(),
            new SuffixConverter(),
            OnThisThread,
            null);

        await Assert.That(view.Caption).IsEqualTo($"{BoundValue}{SuffixConverter.Suffix}");
    }

    /// <summary>A view-first scheduled two-way binding converts a write from the view side too.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    /// <remarks>
    /// The pair round-trips: the forward converter appends the marker and the reverse one strips it, so the
    /// binding settles. A pair that changed the value on both passes would feed each write back through the
    /// other side forever.
    /// </remarks>
    [Test]
    public async Task BindUnsafe_WithConverters_ConvertsTheViewWrite()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var viewModel = new DispatchStubViewModel { Caption = BoundValue };
        var view = new DispatchStubView { ViewModel = viewModel };

        using var binding = view.BindUnsafe(
            viewModel,
            vm => vm.Caption,
            v => v.Caption,
            new SuffixConverter(),
            new StripSuffixConverter(),
            OnThisThread,
            null);

        view.Caption = $"{ReverseValue}{SuffixConverter.Suffix}";

        await Assert.That(viewModel.Caption).IsEqualTo(ReverseValue);
    }

    /// <summary>BindOneWay refuses a scheduled call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindOneWay_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new DispatchStubViewModel().BindOneWay(
            new DispatchStubView(),
            s => s.Caption,
            t => t.Caption,
            ImmediateSequencer.Instance));

    /// <summary>BindOneWay with a conversion refuses a scheduled call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindOneWay_ConvertingWithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new DispatchStubViewModel().BindOneWay(
            new DispatchStubView(),
            s => s.Caption,
            t => t.Caption,
            static value => value,
            ImmediateSequencer.Instance));

    /// <summary>BindOneWay with a converter refuses a scheduled call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindOneWay_WithAConverterAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new DispatchStubViewModel().BindOneWay(
            new DispatchStubView(),
            s => s.Caption,
            t => t.Caption,
            new SuffixConverter(),
            ImmediateSequencer.Instance));

    /// <summary>BindTwoWay refuses a scheduled call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTwoWay_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new DispatchStubViewModel().BindTwoWay(
            new DispatchStubView(),
            s => s.Caption,
            t => t.Caption,
            ImmediateSequencer.Instance));

    /// <summary>BindTwoWay with conversions refuses a scheduled call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTwoWay_ConvertingWithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new DispatchStubViewModel().BindTwoWay(
            new DispatchStubView(),
            s => s.Caption,
            t => t.Caption,
            static value => value,
            static value => value,
            ImmediateSequencer.Instance));

    /// <summary>BindTwoWay with converters refuses a scheduled call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTwoWay_WithConvertersAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new DispatchStubViewModel().BindTwoWay(
            new DispatchStubView(),
            s => s.Caption,
            t => t.Caption,
            new SuffixConverter(),
            new SuffixConverter(),
            ImmediateSequencer.Instance));

    /// <summary>OneWayBind with a selector refuses a scheduled call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task OneWayBind_WithASelectorAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new DispatchStubView().OneWayBind(
            new DispatchStubViewModel(),
            vm => vm.Caption,
            v => v.Caption,
            static value => value,
            ImmediateSequencer.Instance));

    /// <summary>OneWayBind with a converter refuses a scheduled call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task OneWayBind_WithAConverterAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new DispatchStubView().OneWayBind(
            new DispatchStubViewModel(),
            vm => vm.Caption,
            v => v.Caption,
            new SuffixConverter(),
            ImmediateSequencer.Instance));

    /// <summary>Bind with conversions refuses a scheduled call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Bind_WithConversionsAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new DispatchStubView().Bind(
            new DispatchStubViewModel(),
            vm => vm.Caption,
            v => v.Caption,
            static value => value,
            static value => value,
            ImmediateSequencer.Instance));

    /// <summary>Bind with converters refuses a scheduled call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Bind_WithConvertersAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new DispatchStubView().Bind(
            new DispatchStubViewModel(),
            vm => vm.Caption,
            v => v.Caption,
            new SuffixConverter(),
            new SuffixConverter(),
            ImmediateSequencer.Instance));

    /// <summary>Asserts that a scheduled call refuses and names the overload that resolves it.</summary>
    /// <param name="call">The call expected to refuse.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertRefused(Func<object?> call)
    {
        var error = await Assert.That(call).Throws<InvalidOperationException>();

        await Assert.That(error!.Message).Contains("Unsafe");
    }

    /// <summary>The reverse of <see cref="SuffixConverter"/>, so a two-way pair round-trips and settles.</summary>
    private sealed class StripSuffixConverter : IBindingTypeConverter
    {
        /// <summary>An affinity high enough to outrank every registered converter.</summary>
        private const int WinningAffinity = 100;

        /// <inheritdoc/>
        public Type FromType => typeof(string);

        /// <inheritdoc/>
        public Type ToType => typeof(string);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects() => WinningAffinity;

        /// <inheritdoc/>
        public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
        {
            result = from is string text && text.EndsWith(SuffixConverter.Suffix, StringComparison.Ordinal)
                ? text[..^SuffixConverter.Suffix.Length]
                : from;
            return true;
        }
    }

    /// <summary>A converter that appends a marker, so a test can see that it ran.</summary>
    private sealed class SuffixConverter : IBindingTypeConverter
    {
        /// <summary>The marker a converted value carries.</summary>
        internal const string Suffix = "-converted";

        /// <summary>An affinity high enough to outrank every registered converter.</summary>
        private const int WinningAffinity = 100;

        /// <inheritdoc/>
        public Type FromType => typeof(string);

        /// <inheritdoc/>
        public Type ToType => typeof(string);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects() => WinningAffinity;

        /// <inheritdoc/>
        public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
        {
            result = from is string text ? $"{text}{Suffix}" : from;
            return true;
        }
    }
}
