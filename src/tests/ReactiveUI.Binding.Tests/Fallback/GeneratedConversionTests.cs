// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>
/// Tests for <see cref="GeneratedConversion"/>, the conversion stage a generated binding hands its compile-time
/// conversion to. A named converter always converts, a registered one only when it outranks the generated
/// conversion, and the generated conversion otherwise.
/// </summary>
public class GeneratedConversionTests
{
    /// <summary>The hint that makes a stub converter decline.</summary>
    private const string DeclineHint = "decline";

    /// <summary>What a generated conversion that must not run would produce.</summary>
    private const string Generated = "generated";

    /// <summary>A value every conversion accepts.</summary>
    private const int Accepted = 7;

    /// <summary>A value the generated conversion rejects.</summary>
    private const int Rejected = -1;

    /// <summary>An affinity below the stub converter's.</summary>
    private const int BeatableAffinity = 1;

    /// <summary>The stub converter's own affinity, which it does not outrank.</summary>
    private const int MatchingAffinity = 10;

    /// <summary>An identity conversion with no converter hands the source back, adding no stage.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Convert_IdentityWithNoConverter_ReturnsTheSource()
    {
        var source = new ManualObservable<Unregistered>();

        var converted = GeneratedConversion.Convert(
            source,
            null,
            null,
            BeatableAffinity,
            true,
            static (value, _) => (true, value),
            null);

        await Assert.That(converted).IsSameReferenceAs(source);
    }

    /// <summary>The generated conversion runs with the hint, and a value it rejects is dropped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Convert_WithNoConverter_RunsTheGeneratedConversion()
    {
        var source = new ManualObservable<int>();
        var observer = new RecordingObserver<Unregistered>();

        using var subscription = GeneratedConversion.Convert(
            source,
            null,
            "hint",
            BeatableAffinity,
            false,
            static (value, hint) => value >= 0 ? (true, new Unregistered($"{hint}:{value}")) : (false, null!),
            static _ => new("fallback")).Subscribe(observer);
        source.Observer!.OnNext(Accepted);
        source.Observer.OnNext(Rejected);

        await Assert.That(observer.Values.Count).IsEqualTo(1);
        await Assert.That(observer.Values[0].Text).IsEqualTo("hint:7");
    }

    /// <summary>A named converter that implements the typed interface converts without the generated conversion.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Convert_WithATypedOverride_UsesTheOverride()
    {
        var source = new ManualObservable<int>();
        var observer = new RecordingObserver<string>();

        using var subscription = GeneratedConversion.Convert(
            source,
            new TypedConverter(),
            null,
            int.MaxValue,
            false,
            static (_, _) => (true, Generated),
            static _ => "fallback").Subscribe(observer);
        source.Observer!.OnNext(Accepted);
        source.Observer.OnNext(Rejected);

        // The call site named this converter, so the value it declines is dropped rather than assigned.
        await Assert.That(observer.Values.Count).IsEqualTo(1);
        await Assert.That(observer.Values[0]).IsEqualTo("typed:7");
    }

    /// <summary>A named converter without the typed interface converts through its boxing method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Convert_WithAnUntypedOverride_UsesTheOverride()
    {
        var source = new ManualObservable<int>();
        var observer = new RecordingObserver<string>();
        var converter = new StubBindingTypeConverter(
            typeof(int),
            typeof(string),
            static (value, hint) => new(!Equals(hint, DeclineHint), $"untyped:{value}"));

        using var subscription = GeneratedConversion.Convert(
            source,
            converter,
            null,
            BeatableAffinity,
            false,
            static (_, _) => (true, Generated),
            null).Subscribe(observer);
        source.Observer!.OnNext(Accepted);

        await Assert.That(observer.Values.Count).IsEqualTo(1);
        await Assert.That(observer.Values[0]).IsEqualTo("untyped:7");
    }

    /// <summary>A registered converter that outranks the generated conversion converts in its place.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Convert_WithAHigherRegisteredConverter_UsesTheRegistration()
    {
        RegisterConverter();
        var source = new ManualObservable<RegisteredFrom>();
        var observer = new RecordingObserver<RegisteredTo>();

        using var subscription = GeneratedConversion.Convert<RegisteredFrom, RegisteredTo>(
            source,
            null,
            null,
            BeatableAffinity,
            true,
            static (_, _) => (true, new(Generated)),
            null).Subscribe(observer);
        source.Observer!.OnNext(new("a"));

        await Assert.That(observer.Values.Count).IsEqualTo(1);
        await Assert.That(observer.Values[0].Text).IsEqualTo("registered:a");
    }

    /// <summary>A value a registered converter declines is assigned through the fallback, or dropped without one.</summary>
    /// <param name="hasFallback">Whether the types are assignable, so a fallback exists.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Convert_WhenARegisteredConverterDeclines_FallsBackToAssignment(bool hasFallback)
    {
        RegisterConverter();
        var source = new ManualObservable<RegisteredFrom>();
        var observer = new RecordingObserver<RegisteredTo>();
        Func<RegisteredFrom, RegisteredTo>? fallback = hasFallback ? static value => new($"assigned:{value.Text}") : null;

        using var subscription = GeneratedConversion.Convert(
            source,
            null,
            DeclineHint,
            BeatableAffinity,
            false,
            static (_, _) => (true, new(Generated)),
            fallback).Subscribe(observer);
        source.Observer!.OnNext(new("a"));

        string[] expected = hasFallback ? ["assigned:a"] : [];
        await Assert.That(observer.Values.Select(static value => value.Text)).IsEquivalentTo(expected);
    }

    /// <summary>A registered converter that does not outrank the generated conversion is left out.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Convert_WithARegisteredConverterAtTheSameAffinity_KeepsTheGeneratedConversion()
    {
        RegisterConverter();
        var source = new ManualObservable<RegisteredFrom>();
        var observer = new RecordingObserver<RegisteredTo>();

        using var subscription = GeneratedConversion.Convert<RegisteredFrom, RegisteredTo>(
            source,
            null,
            null,
            MatchingAffinity,
            false,
            static (value, _) => (true, new($"generated:{value.Text}")),
            null).Subscribe(observer);
        source.Observer!.OnNext(new("a"));

        await Assert.That(observer.Values.Count).IsEqualTo(1);
        await Assert.That(observer.Values[0].Text).IsEqualTo("generated:a");
    }

    /// <summary>The source is required.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Convert_NullSource_Throws() =>
        await Assert.That(static () => GeneratedConversion.Convert<int, int>(null!, null, null, 0, true, static (value, _) => (true, value), null))
            .Throws<ArgumentNullException>();

    /// <summary>The generated conversion is required.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Convert_NullConversion_Throws() =>
        await Assert.That(static () => GeneratedConversion.Convert<int, int>(new ManualObservable<int>(), null, null, 0, true, null!, null))
            .Throws<ArgumentNullException>();

    /// <summary>Registers the stub converter between the two fixture types, which no other test converts.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RegisterConverter() =>
        BindingConverters.Current.TypedConverters.Register(new StubBindingTypeConverter(
            typeof(RegisteredFrom),
            typeof(RegisteredTo),
            static (value, hint) => new(!Equals(hint, DeclineHint), new RegisteredTo($"registered:{((RegisteredFrom)value!).Text}"))));

    /// <summary>A converter that implements the typed interface and declines negative numbers.</summary>
    private sealed class TypedConverter : IBindingTypeConverter<int, string>
    {
        /// <inheritdoc/>
        public Type FromType => typeof(int);

        /// <inheritdoc/>
        public Type ToType => typeof(string);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects() => BeatableAffinity;

        /// <inheritdoc/>
        public bool TryConvert(int from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
        {
            result = from >= 0 ? FormattableString.Invariant($"typed:{from}") : null;
            return result is not null;
        }

        /// <inheritdoc/>
        public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
        {
            result = null;
            return false;
        }
    }

    /// <summary>A type no converter is registered for.</summary>
    /// <param name="Text">What the value holds.</param>
    private sealed record Unregistered(string Text);

    /// <summary>The source type of the registered stub converter.</summary>
    /// <param name="Text">What the value holds.</param>
    private sealed record RegisteredFrom(string Text);

    /// <summary>The target type of the registered stub converter.</summary>
    /// <param name="Text">What the value holds.</param>
    private sealed record RegisteredTo(string Text);
}
