// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Primitives.Concurrency;
using Stubs = ReactiveUI.Binding.ReactiveUIBindingExtensions;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Covers the <c>ToProperty</c> overloads that exist only to be replaced by a generated one.</summary>
/// <remarks>
/// Each overload is reached on the declaring class rather than as an extension method, because written as an
/// extension call a generated dispatch wins and the stub is never entered. The property is an <see langword="int"/>
/// so no initial value can bind to a string parameter, which keeps every call on the overload it names.
/// </remarks>
public class ToPropertyDispatchStubTests
{
    /// <summary>The property the calls name by string.</summary>
    private const string PropertyName = nameof(Owner.Count);

    /// <summary>The initial value the calls pass.</summary>
    private const int InitialValue = 1;

    /// <summary>What the refusal names, so the caller knows which call no dispatch claimed.</summary>
    private const string RefusedApi = "ToProperty";

    /// <summary>The initial-value factory the calls pass.</summary>
    private static readonly Func<int> InitialFactory = static () => InitialValue;

    /// <summary>The object the property belongs to.</summary>
    private static readonly Owner Target = new();

    /// <summary>The stream the property would take its values from.</summary>
    private static readonly IObservable<int> Source = new ManualObservable<int>();

    /// <summary>Gets a scheduler argument typed so the call reaches the overload taking one.</summary>
    private static ISequencer? NoScheduler => null;

    /// <summary>Gets every <c>ToProperty</c> overload, called with the arguments that select it.</summary>
    /// <returns>One call per overload.</returns>
    public static IEnumerable<Func<StubCall>> Overloads()
    {
        // Selector overloads.
        yield return static () => new("selector", static () => Stubs.ToProperty(Source, Target, static x => x.Count));
        yield return static () => new("selector, defer", static () => Stubs.ToProperty(Source, Target, static x => x.Count, true));
        yield return static () => new("selector, scheduler", static () => Stubs.ToProperty(Source, Target, static x => x.Count, NoScheduler));
        yield return static () => new("selector, defer, scheduler", static () => Stubs.ToProperty(Source, Target, static x => x.Count, true, NoScheduler));
        yield return static () => new("selector, initial", static () => Stubs.ToProperty(Source, Target, static x => x.Count, InitialValue));
        yield return static () => new("selector, initial, scheduler", static () => Stubs.ToProperty(Source, Target, static x => x.Count, InitialValue, NoScheduler));
        yield return static () => new("selector, initial, defer", static () => Stubs.ToProperty(Source, Target, static x => x.Count, InitialValue, true));
        yield return static () => new("selector, initial, defer, scheduler", static () => Stubs.ToProperty(Source, Target, static x => x.Count, InitialValue, true, NoScheduler));
        yield return static () => new("selector, factory", static () => Stubs.ToProperty(Source, Target, static x => x.Count, InitialFactory));
        yield return static () => new("selector, factory, scheduler", static () => Stubs.ToProperty(Source, Target, static x => x.Count, InitialFactory, NoScheduler));
        yield return static () => new("selector, factory, defer", static () => Stubs.ToProperty(Source, Target, static x => x.Count, InitialFactory, true));
        yield return static () => new("selector, factory, defer, scheduler", static () => Stubs.ToProperty(Source, Target, static x => x.Count, InitialFactory, true, NoScheduler));

        // Selector overloads that hand the helper back through an out parameter.
        yield return static () => new("selector, out", static () => Stubs.ToProperty(Source, Target, static x => x.Count, out _));
        yield return static () => new("selector, out, defer", static () => Stubs.ToProperty(Source, Target, static x => x.Count, out _, true));
        yield return static () => new("selector, out, defer, scheduler", static () => Stubs.ToProperty(Source, Target, static x => x.Count, out _, true, NoScheduler));
        yield return static () => new("selector, out, initial", static () => Stubs.ToProperty(Source, Target, static x => x.Count, out _, InitialValue));
        yield return static () => new("selector, out, initial, defer", static () => Stubs.ToProperty(Source, Target, static x => x.Count, out _, InitialValue, true));
        yield return static () => new("selector, out, initial, defer, scheduler", static () => Stubs.ToProperty(Source, Target, static x => x.Count, out _, InitialValue, true, NoScheduler));
        yield return static () => new("selector, out, factory", static () => Stubs.ToProperty(Source, Target, static x => x.Count, out _, InitialFactory));
        yield return static () => new("selector, out, factory, defer", static () => Stubs.ToProperty(Source, Target, static x => x.Count, out _, InitialFactory, true));
        yield return static () => new("selector, out, factory, defer, scheduler", static () => Stubs.ToProperty(Source, Target, static x => x.Count, out _, InitialFactory, true, NoScheduler));

        // Name overloads.
        yield return static () => new("name, initial", static () => Stubs.ToProperty(Source, Target, PropertyName, InitialValue));
        yield return static () => new("name, initial, scheduler", static () => Stubs.ToProperty(Source, Target, PropertyName, InitialValue, NoScheduler));
        yield return static () => new("name, initial, defer", static () => Stubs.ToProperty(Source, Target, PropertyName, InitialValue, true));
        yield return static () => new("name, initial, defer, scheduler", static () => Stubs.ToProperty(Source, Target, PropertyName, InitialValue, true, NoScheduler));
        yield return static () => new("name", static () => Stubs.ToProperty(Source, Target, PropertyName));
        yield return static () => new("name, defer", static () => Stubs.ToProperty(Source, Target, PropertyName, true));
        yield return static () => new("name, scheduler", static () => Stubs.ToProperty(Source, Target, PropertyName, NoScheduler));
        yield return static () => new("name, defer, scheduler", static () => Stubs.ToProperty(Source, Target, PropertyName, true, NoScheduler));
        yield return static () => new("name, factory", static () => Stubs.ToProperty(Source, Target, PropertyName, InitialFactory));
        yield return static () => new("name, factory, defer", static () => Stubs.ToProperty(Source, Target, PropertyName, InitialFactory, true));
        yield return static () => new("name, factory, defer, scheduler", static () => Stubs.ToProperty(Source, Target, PropertyName, InitialFactory, true, NoScheduler));

        // Name overloads that hand the helper back through an out parameter.
        yield return static () => new("name, out", static () => Stubs.ToProperty(Source, Target, PropertyName, out _));
        yield return static () => new("name, out, defer", static () => Stubs.ToProperty(Source, Target, PropertyName, out _, true));
        yield return static () => new("name, out, defer, scheduler", static () => Stubs.ToProperty(Source, Target, PropertyName, out _, true, NoScheduler));
        yield return static () => new("name, out, factory", static () => Stubs.ToProperty(Source, Target, PropertyName, out _, InitialFactory));
        yield return static () => new("name, out, factory, defer", static () => Stubs.ToProperty(Source, Target, PropertyName, out _, InitialFactory, true));
        yield return static () => new("name, out, factory, defer, scheduler", static () => Stubs.ToProperty(Source, Target, PropertyName, out _, InitialFactory, true, NoScheduler));
    }

    /// <summary>A call no generated dispatch claimed refuses, and names the API it was made through.</summary>
    /// <param name="call">One overload, called with the arguments that select it.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodDataSource(nameof(Overloads))]
    public async Task ToProperty_WithNoGeneratedOverload_Refuses(StubCall call)
    {
        ArgumentNullException.ThrowIfNull(call);

        var error = await Assert.That(call.Invoke).Throws<InvalidOperationException>();

        await Assert.That(error!.Message).Contains(RefusedApi);
    }

    /// <summary>A type that declares the property the calls name.</summary>
    private sealed class Owner
    {
        /// <summary>Gets the property the calls back with a stream.</summary>
        public int Count { get; }
    }

    /// <summary>One overload's call, named by the arguments that select it.</summary>
    /// <param name="Shape">The arguments that select the overload, for the test's display name.</param>
    /// <param name="Invoke">Makes the call.</param>
    public sealed record StubCall(string Shape, Func<object?> Invoke)
    {
        /// <inheritdoc/>
        public override string ToString() => Shape;
    }
}
