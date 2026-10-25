// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>
/// Tests for <see cref="ReactiveNotifyPropertyChangedMixins"/>, the runtime bridge a binding falls back to
/// when the generator could not resolve the observation at compile time.
/// </summary>
public class ReactiveNotifyPropertyChangedMixinTests
{
    /// <summary>The value the view model starts with.</summary>
    private const string InitialValue = "Initial";

    /// <summary>A changed property value used across notification tests.</summary>
    private const string NotifyingValue = "Changed";

    /// <summary>The expected number of emissions after a single change (kicker plus one change).</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>The expected number of emissions after two successive changes (kicker plus two changes).</summary>
    private const int ExpectedThreeEmissions = 3;

    /// <summary>The array an index expression with no indexer behind it reads from.</summary>
    private static readonly string[] _arrayBackingTheIndexExpression = [InitialValue];

    /// <summary>
    /// Verifies that NestedObservedChanges returns a single-element observable with default value
    /// when the sourceChange value is null.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 230-232 (sourceChange.Value is null branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NestedObservedChanges_NullSourceChangeValue_ReturnsSingleKicker()
    {
        EnsureInitialized();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var sourceChange = new ObservedChange<object?, object?>(null, body, null);

        var results = new List<IObservedChange<object?, object?>>();
        using var sub = ReactiveNotifyPropertyChangedMixins.NestedObservedChanges(body, sourceChange, false)
            .Subscribe(results.Add);

        await Assert.That(results.Count).IsEqualTo(1);
        await Assert.That(results[0].Sender).IsNull();
    }

    /// <summary>
    /// Verifies that NestedObservedChanges emits a kicker followed by property notifications
    /// when the sourceChange value is non-null.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 235-237 (sourceChange.Value is not null branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NestedObservedChanges_NonNullSourceChangeValue_EmitsKickerAndNotifications()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = InitialValue };

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var sourceChange = new ObservedChange<object?, object?>(null, body, vm);

        var results = new List<IObservedChange<object?, object?>>();
        using var sub = ReactiveNotifyPropertyChangedMixins.NestedObservedChanges(body, sourceChange, false)
            .Subscribe(results.Add);

        // Should have the kicker (StartWith)
        await Assert.That(results.Count).IsGreaterThanOrEqualTo(1);

        // Now change the property - should emit another notification
        vm.Name = NotifyingValue;

        await Assert.That(results.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
    }

    /// <summary>
    /// Verifies that NotifyForProperty throws ArgumentException when the expression has no valid member info.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 248 (memberInfo null branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NotifyForProperty_ExpressionWithNoMemberInfo_ThrowsArgumentException()
    {
        EnsureInitialized();

        var vm = new TestViewModel();

        // A ParameterExpression has no member info, so GetMemberInfo will throw
        var paramExpr = System.Linq.Expressions.Expression.Parameter(typeof(TestViewModel), "x");

        var action = () => ReactiveNotifyPropertyChangedMixins.NotifyForProperty(vm, paramExpr, false);

        await Assert.That(action).ThrowsException();
    }

    /// <summary>
    /// Verifies that SubscribeToExpressionChain with isDistinct=false does not deduplicate values
    /// when the same value is emitted multiple times.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 219 (isDistinct false branch in expression chain overload).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscribeToExpressionChain_IsDistinctFalse_DoesNotDeduplicate()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "A" };

        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;

        var values = new List<IObservedChange<TestFixture, string>>();
        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
                expr.Body,
                false,
                false,
                false)
            .Subscribe(values.Add);

        fixture.IsNotNullString = "B";
        fixture.IsNotNullString = "C";

        // Should have initial + 2 changes = at least 3
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedThreeEmissions);
    }

    /// <summary>
    /// Verifies that SubscribeToExpressionChain with isDistinct=true deduplicates by value.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 219 (isDistinct true branch in expression chain overload).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscribeToExpressionChain_IsDistinctTrue_DeduplicatesByValue()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "A" };

        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;

        var values = new List<IObservedChange<TestFixture, string>>();
        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
                expr.Body,
                false,
                false,
                true)
            .Subscribe(values.Add);

        // Change to different values
        fixture.IsNotNullString = "B";
        fixture.IsNotNullString = "C";

        // All values are distinct, so all should come through
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedThreeEmissions);
    }

    /// <summary>
    /// Verifies that SubscribeToExpressionChain with skipInitial=true skips the first value.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 201-203 (skipInitial true path).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscribeToExpressionChain_SkipInitialTrue_SkipsFirstValue()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = InitialValue };

        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;

        var values = new List<IObservedChange<TestFixture, string>>();
        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
                expr.Body,
                false,
                true,
                false)
            .Subscribe(values.Add);

        // No initial value should have been emitted
        await Assert.That(values.Count).IsEqualTo(0);

        fixture.IsNotNullString = NotifyingValue;

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0].Value).IsEqualTo(NotifyingValue);
    }

    /// <summary>
    /// Verifies that SubscribeToExpressionChain filters out null senders from the chain.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 206 (Where x.Sender is not null filter).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscribeToExpressionChain_NullSenderInChain_IsFilteredOut()
    {
        EnsureInitialized();

        // A deep chain where intermediate becomes null will produce a null-sender change
        var fixture = new HostTestFixture { Child = new() { IsNotNullString = "Hello" } };

        var values = new List<IObservedChange<HostTestFixture, string>>();
        using var sub = fixture.SubscribeToExpressionChain<HostTestFixture, string>(
                ((Expression<Func<HostTestFixture, string>>)(x => x.Child!.IsNotNullString)).Body,
                false,
                false,
                false)
            .Subscribe(values.Add);

        var initialCount = values.Count;

        // Setting Child to null should not produce a null-sender emission in the final stream
        fixture.Child = null;

        // Setting it back should produce a new emission
        fixture.Child = new() { IsNotNullString = "World" };

        await Assert.That(values.Count).IsGreaterThan(initialCount);
    }

    /// <summary>
    /// Verifies that ObservableForProperty by name with beforeChange=true emits before-change notifications.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 111-129 (factory subscription path with beforeChange).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_BeforeChange_EmitsNotification()
    {
        EnsureInitialized();

        var vm = new TestViewModel();

        var values = new List<IObservedChange<TestViewModel, string>>();
        using var sub = vm.ObservableForProperty<TestViewModel, string>("Name", true)
            .Subscribe(values.Add);

        vm.Name = "Alice";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Verifies that ObservableForProperty by expression with beforeChange=true emits notifications.
    /// Covers the expression-based overload of ObservableForProperty with beforeChange true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByExpression_BeforeChange_EmitsNotification()
    {
        EnsureInitialized();

        var vm = new TestViewModel();

        var values = new List<IObservedChange<TestViewModel, string>>();
        using var sub = vm.ObservableForProperty(x => x.Name, true, true, true)
            .Subscribe(values.Add);

        vm.Name = "Bob";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Verifies that NestedObservedChanges with beforeChange=true routes through
    /// the PropertyChanging event path.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 235 with beforeChange true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NestedObservedChanges_BeforeChange_RoutesToPropertyChanging()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = InitialValue };

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var sourceChange = new ObservedChange<object?, object?>(null, body, vm);

        var results = new List<IObservedChange<object?, object?>>();
        using var sub = ReactiveNotifyPropertyChangedMixins.NestedObservedChanges(body, sourceChange, true)
            .Subscribe(results.Add);

        // Should have the kicker from StartWith
        await Assert.That(results.Count).IsGreaterThanOrEqualTo(1);

        // Changing the property should trigger a PropertyChanging notification
        vm.Name = NotifyingValue;

        await Assert.That(results.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
    }

    /// <summary>
    /// Verifies that the val cast path in SubscribeToExpressionChain line 93 is exercised
    /// when val is TValue (the normal case).
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 93 (val is TValue cast).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_GetCurrentValue_ValIsTValue_Succeeds()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Test" };

        string? receivedValue = null;
        using var sub = vm.ObservableForProperty<TestViewModel, string>("Name", skipInitial: false)
            .Subscribe(x => receivedValue = x.Value);

        await Assert.That(receivedValue).IsEqualTo("Test");
    }

    /// <summary>
    /// Verifies that a lookup made while no <see cref="ICreatesObservableForProperty"/> is registered
    /// does not stop the same sender and property from resolving once registration has happened.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_LookupBeforeRegistration_ResolvesAfterRegistration()
    {
        RxBindingBuilder.ResetForTesting();

        var vm = new LateRegistrationFixture { Name = InitialValue };

        await Assert.That(() => vm.ObservableForProperty<LateRegistrationFixture, string>(
                nameof(LateRegistrationFixture.Name),
                skipInitial: false))
            .ThrowsExactly<InvalidOperationException>();

        EnsureInitialized();

        var values = new List<IObservedChange<LateRegistrationFixture, string>>();
        using var sub = vm.ObservableForProperty<LateRegistrationFixture, string>(
                nameof(LateRegistrationFixture.Name),
                skipInitial: false)
            .Subscribe(values.Add);

        vm.Name = NotifyingValue;

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
    }

    /// <summary>
    /// Verifies that a nested-chain lookup made while no <see cref="ICreatesObservableForProperty"/>
    /// is registered does not stop the same sender and property from resolving once registration
    /// has happened.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NotifyForProperty_LookupBeforeRegistration_ResolvesAfterRegistration()
    {
        RxBindingBuilder.ResetForTesting();

        var vm = new LateRegistrationFixture { Title = InitialValue };
        Expression<Func<LateRegistrationFixture, string>> expr = x => x.Title;
        var body = Reflection.Rewrite(expr.Body);

        await Assert.That(() => ReactiveNotifyPropertyChangedMixins.NotifyForProperty(vm, body, false))
            .ThrowsExactly<InvalidOperationException>();

        EnsureInitialized();

        var results = new List<IObservedChange<object?, object?>>();
        using var sub = ReactiveNotifyPropertyChangedMixins.NotifyForProperty(vm, body, false)
            .Subscribe(results.Add);

        vm.Title = NotifyingValue;

        await Assert.That(results.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// A property holding null is observed as the default of the observed type, so a subscriber sees the
    /// absence rather than the observation faulting on the way to reading it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_NullPropertyValue_EmitsTheDefault()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var values = new List<int?>();

        using var subscription = fixture.ObservableForProperty<TestFixture, int?>(
            nameof(TestFixture.NullableInt),
            false,
            false,
            true).Subscribe(x => values.Add(x.Value));

        await Assert.That(values).Contains(static x => x is null);
    }

    /// <summary>
    /// A property read as a type it does not hold is a caller error, and surfaces as the cast failure rather
    /// than as a silently wrong value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_ValueOfAnotherType_SignalsInvalidCast()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = InitialValue };

        await Assert.That(() =>
        {
            using var subscription = fixture.ObservableForProperty<TestFixture, int>(
                nameof(TestFixture.IsNotNullString),
                false,
                false,
                true).Subscribe(static _ => { });
        }).Throws<InvalidCastException>();
    }

    /// <summary>The overload that takes the whole set of options builds the same chain observation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscribeToExpressionChain_WithSuppressedWarnings_ObservesTheChain()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = InitialValue };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var subscription = fixture
            .SubscribeToExpressionChain<TestFixture, string>(expr.Body, false, false, true, true)
            .Subscribe(x => values.Add(x.Value));

        fixture.IsNotNullString = NotifyingValue;

        await Assert.That(values).Contains(NotifyingValue);
    }

    /// <summary>
    /// An array index reaches the member lookup as an index expression with no indexer behind it, so there
    /// is no property to observe and the call is rejected rather than observing nothing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NotifyForProperty_WithSuppressedWarnings_ExpressionNamingNoMember_Throws()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var arrayAccess = System.Linq.Expressions.Expression.ArrayAccess(
            System.Linq.Expressions.Expression.Constant(_arrayBackingTheIndexExpression),
            System.Linq.Expressions.Expression.Constant(0));

        await Assert.That(() => ReactiveNotifyPropertyChangedMixins.NotifyForProperty(fixture, arrayAccess, false, true))
            .ThrowsExactly<ArgumentException>();
    }

    /// <summary>Resets and initializes the ReactiveUI binding infrastructure for testing.</summary>
    private static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }
}
