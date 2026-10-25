// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.WhenAny;

/// <summary>Tests for SubscribeToExpressionChain with various options.</summary>
public class ExpressionChainTests
{
    /// <summary>The expected number of emitted values when two notifications are produced.</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>The value a fixture starts out holding.</summary>
    private const string StartValue = "Start";

    /// <summary>The value a before-change fixture starts out holding.</summary>
    private const string InitialValue = "Before";

    /// <summary>The value the first change moves to, which the second change then leaves behind.</summary>
    private const string ReplacementValue = "After";

    /// <summary>Verifies basic usage notifies on change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicUsage_NotifiesOnChange()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = StartValue };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
                expr.Body,
                false,
                false,
                true)
            .Select(static x => x.Value)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(StartValue);

        fixture.IsNotNullString = "End";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[1]).IsEqualTo("End");
    }

    /// <summary>
    /// A before-change chain reports the value a property is about to leave behind. The first change leaves
    /// the value the initial emission already carried, and a repeat that immediately follows the initial read
    /// is the one the subscribe-then-read window can produce on its own, so it is dropped. The second change
    /// leaves a value nobody has seen, and is reported.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithBeforeChange_ReportsTheValueEachChangeLeavesBehind()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = InitialValue };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
                expr.Body,
                true,
                false,
                false)
            .Select(static x => x.Value)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialValue);

        fixture.IsNotNullString = ReplacementValue;
        fixture.IsNotNullString = "Third";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[1]).IsEqualTo(ReplacementValue);
    }

    /// <summary>Verifies that skipInitial skips the first emission.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSkipInitial_SkipsFirstEmission()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Initial" };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
                expr.Body)
            .Select(static x => x.Value)
            .Subscribe(values.Add);

        // Should NOT have emitted the initial value
        await Assert.That(values.Count).IsEqualTo(0);

        fixture.IsNotNullString = "Changed";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Changed");
    }

    /// <summary>Verifies that isDistinct deduplicates same values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithIsDistinct_DeduplicatesSameValues()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Same" };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
                expr.Body)
            .Select(static x => x.Value)
            .Subscribe(values.Add);

        fixture.IsNotNullString = "A";
        fixture.IsNotNullString = "B";
        fixture.IsNotNullString = "B"; // Same value — setter will short-circuit

        await Assert.That(values.Count).IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[1]).IsEqualTo("B");
    }

    /// <summary>
    /// A link with no change notification emits once on subscribe. The chain reads the same value straight after,
    /// so it reports the initial value once, and the notifying leaf behind it still reports its changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PocoLinkBeforeNotifyingLeaf_EmitsTheInitialValueOnce()
    {
        EnsureInitialized();

        var root = new PocoChainRoot();
        root.Leaf.IsNotNullString = StartValue;
        Expression<Func<PocoChainRoot, string>> expr = x => x.Leaf.IsNotNullString;
        var values = new List<string>();

        using var sub = root.SubscribeToExpressionChain<PocoChainRoot, string>(
                expr.Body,
                false,
                false,
                false)
            .Select(static x => x.Value)
            .Subscribe(values.Add);
        var initial = values.ToArray();

        root.Leaf.IsNotNullString = ReplacementValue;

        using (Assert.Multiple())
        {
            await Assert.That(initial).IsEquivalentTo([StartValue]);
            await Assert.That(values).IsEquivalentTo([StartValue, ReplacementValue]);
        }
    }

    /// <summary>A leaf with no change notification reports its value once, and skipping the initial value leaves nothing.</summary>
    /// <param name="skipInitial">Whether the first value is dropped.</param>
    /// <param name="expectedCount">The number of values the chain reports.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false, 1)]
    [Arguments(true, 0)]
    public async Task PocoLeaf_ReportsTheInitialValueAtMostOnce(bool skipInitial, int expectedCount)
    {
        EnsureInitialized();

        var fixture = new PocoModel { Value = StartValue };
        Expression<Func<PocoModel, string>> expr = x => x.Value;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<PocoModel, string>(
                expr.Body,
                false,
                skipInitial,
                false,
                true)
            .Select(static x => x.Value)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsEqualTo(expectedCount);
    }

    /// <summary>Verifies that null in a chain propagates correctly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullInChain_PropagatesCorrectly()
    {
        EnsureInitialized();

        var fixture = new HostTestFixture();
        Expression<Func<HostTestFixture, string>> expr = x => x.Child!.IsNotNullString;
        var values = new List<IObservedChange<HostTestFixture, string>>();

        // Child is null initially
        using var sub = fixture.SubscribeToExpressionChain<HostTestFixture, string>(
                expr.Body,
                false,
                false,
                false)
            .Subscribe(values.Add);

        // Set Child to something
        fixture.Child = new() { IsNotNullString = "Hello" };

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>Verifies that an expression with no member access yields a chain that never emits.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IdentityExpression_HasNoLinksAndNeverEmits()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = StartValue };
        Expression<Func<TestFixture, TestFixture>> expr = x => x;
        var recorder = new Recorder<TestFixture, TestFixture>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, TestFixture>(
                expr.Body,
                false,
                false,
                false)
            .Subscribe(recorder);

        await Assert.That(recorder.Values.Count).IsEqualTo(0);
        await Assert.That(recorder.Error).IsNull();
    }

    /// <summary>Verifies that a leaf value of an unrelated type is reported rather than silently dropped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LeafValueOfAnotherType_SignalsInvalidCast()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = StartValue };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var recorder = new Recorder<TestFixture, int>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, int>(
                expr.Body,
                false,
                false,
                false)
            .Subscribe(recorder);

        await Assert.That(recorder.Error).IsTypeOf<InvalidCastException>();
    }

    /// <summary>Verifies that a null root leaves the whole chain unparented rather than throwing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullRoot_ProducesNoValues()
    {
        EnsureInitialized();

        HostTestFixture? fixture = null;
        Expression<Func<HostTestFixture, string>> expr = x => x.Child!.IsNotNullString;
        var recorder = new Recorder<HostTestFixture, string>();

        using var sub = fixture.SubscribeToExpressionChain<HostTestFixture, string>(
                expr.Body,
                false,
                false,
                false)
            .Subscribe(recorder);

        await Assert.That(recorder.Values.Count).IsEqualTo(0);
        await Assert.That(recorder.Error).IsNull();
    }

    /// <summary>Resets and initializes the ReactiveUI binding infrastructure for testing.</summary>
    internal static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }

    /// <summary>Records what a chain subscription produced, including the terminal signal.</summary>
    /// <typeparam name="TSender">The root sender type.</typeparam>
    /// <typeparam name="TValue">The leaf value type.</typeparam>
    private sealed class Recorder<TSender, TValue> : IObserver<IObservedChange<TSender, TValue>>
    {
        /// <summary>Gets the values the chain emitted, in order.</summary>
        public List<IObservedChange<TSender, TValue>> Values { get; } = [];

        /// <summary>Gets the error the chain signalled, if any.</summary>
        public Exception? Error { get; private set; }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(IObservedChange<TSender, TValue> value) => Values.Add(value);

        /// <inheritdoc/>
        public void OnError(Exception error) => Error = error;

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }
    }
}
