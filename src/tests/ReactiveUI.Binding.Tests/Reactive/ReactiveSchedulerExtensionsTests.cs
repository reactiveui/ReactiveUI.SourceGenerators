// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Reactive;

namespace ReactiveUI.Binding.Tests.Reactive;

/// <summary>Covers the System.Reactive flavour of the scheduler overloads, which take an <see cref="IScheduler"/>.</summary>
/// <remarks>
/// Each overload is split in two: the unsuffixed member resolves at compile time and throws when no generated
/// dispatch claimed the call site, and the <c>Unsafe</c> twin walks the chain by reflection. What is checked here
/// is the refusal and the message that names the twin, because that message is how a consumer learns which
/// overload to call. The twins' behaviour is covered against the lean flavour, which compiles the same source.
/// </remarks>
public class ReactiveSchedulerExtensionsTests
{
    /// <summary>BindOneWay refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindOneWay_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new TestModel().BindOneWay(
            new TestModel(),
            s => s.Name,
            t => t.Name,
            ImmediateScheduler.Instance));

    /// <summary>BindOneWay with a conversion refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindOneWay_ConvertingWithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new TestModel().BindOneWay(
            new TestModel(),
            s => s.Name,
            t => t.Name,
            static v => v,
            ImmediateScheduler.Instance));

    /// <summary>BindTwoWay refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTwoWay_WithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new TestModel().BindTwoWay(
            new TestModel(),
            s => s.Name,
            t => t.Name,
            ImmediateScheduler.Instance));

    /// <summary>BindTwoWay with conversions refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTwoWay_ConvertingWithNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new TestModel().BindTwoWay(
            new TestModel(),
            s => s.Name,
            t => t.Name,
            static v => v,
            static v => v,
            ImmediateScheduler.Instance));

    /// <summary>BindOneWay with a converter refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindOneWay_WithAConverterAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new TestModel().BindOneWay(
            new TestModel(),
            s => s.Name,
            t => t.Name,
            new PassThroughConverter(),
            ImmediateScheduler.Instance));

    /// <summary>BindTwoWay with converters refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTwoWay_WithConvertersAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new TestModel().BindTwoWay(
            new TestModel(),
            s => s.Name,
            t => t.Name,
            new PassThroughConverter(),
            new PassThroughConverter(),
            ImmediateScheduler.Instance));

    /// <summary>OneWayBind with a selector refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task OneWayBind_WithASelectorAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new TestView().OneWayBind(
            new TestModel(),
            vm => vm.Name,
            v => v.Caption,
            static value => value,
            ImmediateScheduler.Instance));

    /// <summary>OneWayBind with a converter refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task OneWayBind_WithAConverterAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new TestView().OneWayBind(
            new TestModel(),
            vm => vm.Name,
            v => v.Caption,
            new PassThroughConverter(),
            ImmediateScheduler.Instance));

    /// <summary>Bind with conversions refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Bind_WithConversionsAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new TestView().Bind(
            new TestModel(),
            vm => vm.Name,
            v => v.Caption,
            static value => value,
            static value => value,
            ImmediateScheduler.Instance));

    /// <summary>Bind with converters refuses a call no generated dispatch claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Bind_WithConvertersAndNoGeneratedOverload_NamesTheUnsafeTwin() =>
        AssertRefused(static () => new TestView().Bind(
            new TestModel(),
            vm => vm.Name,
            v => v.Caption,
            new PassThroughConverter(),
            new PassThroughConverter(),
            ImmediateScheduler.Instance));

    /// <summary>Asserts that a call refuses and names the overload that resolves it.</summary>
    /// <param name="call">The call expected to refuse.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertRefused(Func<object?> call)
    {
        var error = await Assert.That(call).Throws<InvalidOperationException>();

        await Assert.That(error!.Message).Contains("Unsafe");
    }

    /// <summary>A converter these overloads take, which never converts because the call refuses first.</summary>
    private sealed class PassThroughConverter : ReactiveUI.Binding.Reactive.IBindingTypeConverter
    {
        /// <summary>The affinity a converter returns when it is the only candidate.</summary>
        private const int SoleCandidateAffinity = 2;

        /// <inheritdoc/>
        public Type FromType => typeof(string);

        /// <inheritdoc/>
        public Type ToType => typeof(string);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects() => SoleCandidateAffinity;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
        {
            result = from;
            return true;
        }
    }

    /// <summary>A view the view-first scheduler overloads name.</summary>
    private sealed class TestView : ReactiveUI.Binding.Reactive.IViewFor
    {
        /// <inheritdoc/>
        public object? ViewModel { get; set; }

        /// <summary>Gets or sets the bound caption.</summary>
        public string? Caption { get; set; }
    }

    /// <summary>A simple test model implementing <see cref="INotifyPropertyChanged"/>.</summary>
    private sealed class TestModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the name.</summary>
        public string? Name
        {
            get => field;
            set
            {
                if (string.Equals(field, value, StringComparison.Ordinal))
                {
                    return;
                }

                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Name)));
            }
        }
    }
}
