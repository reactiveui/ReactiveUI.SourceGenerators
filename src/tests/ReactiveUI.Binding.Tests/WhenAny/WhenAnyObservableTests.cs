// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.WhenAny;

/// <summary>Tests for WhenAnyObservable extension methods.</summary>
public class WhenAnyObservableTests
{
    /// <summary>A sample value emitted by a single observable.</summary>
    private const int SampleValue = 42;

    /// <summary>The second value emitted when merging two observables.</summary>
    private const int SecondValue = 2;

    /// <summary>The expected number of merged emissions from two observables.</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>The first value emitted in the three-observable combining test.</summary>
    private const int FirstCombineValue = 10;

    /// <summary>The second value emitted in the three-observable combining test.</summary>
    private const int SecondCombineValue = 20;

    /// <summary>The third value emitted in the three-observable combining test.</summary>
    private const int ThirdCombineValue = 30;

    /// <summary>The expected number of emissions when combining three observables.</summary>
    private const int ExpectedThreeEmissions = 3;

    /// <summary>A value emitted by a late-assigned observable.</summary>
    private const int LateAssignedValue = 99;

    /// <summary>Verifies that null observables do not cause exceptions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullObservablesDoNotCauseExceptions()
    {
        EnsureInitialized();

        var vm = new TestWhenAnyObsViewModel();
        var values = new List<int>();

        // Command1 is null, should not throw
        using var sub = vm.WhenAnyObservableUnsafe(x => x.Command1)
            .Subscribe(values.Add);

        // Set it to a real observable
        var subject = new Subject<int>();
        vm.Command1 = subject;

        subject.OnNext(SampleValue);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(SampleValue);
    }

    /// <summary>Verifies that merging two observable properties works.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SmokeTestMerging()
    {
        EnsureInitialized();

        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var vm = new TestWhenAnyObsViewModel { Command1 = subject1, Command2 = subject2 };
        var values = new List<int>();

        using var sub = vm.WhenAnyObservableUnsafe(x => x.Command1, x => x.Command2)
            .Subscribe(values.Add);

        subject1.OnNext(1);
        subject2.OnNext(SecondValue);

        await Assert.That(values.Count).IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(values).Contains(1);
        await Assert.That(values).Contains(SecondValue);
    }

    /// <summary>Verifies that combining three observable properties works.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SmokeTestCombining()
    {
        EnsureInitialized();

        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var subject3 = new Subject<int>();
        var vm = new TestWhenAnyObsViewModel { Command1 = subject1, Command2 = subject2, Command3 = subject3 };
        var values = new List<int>();

        using var sub = vm.WhenAnyObservableUnsafe(x => x.Command1, x => x.Command2, x => x.Command3)
            .Subscribe(values.Add);

        subject1.OnNext(FirstCombineValue);
        subject2.OnNext(SecondCombineValue);
        subject3.OnNext(ThirdCombineValue);

        await Assert.That(values.Count).IsEqualTo(ExpectedThreeEmissions);
    }

    /// <summary>Verifies that a null object updates when a non-null observable is assigned.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullObjectUpdatesWhenNotNullAnymore()
    {
        EnsureInitialized();

        var vm = new TestWhenAnyObsViewModel();
        var values = new List<int>();

        using var sub = vm.WhenAnyObservableUnsafe(x => x.Changes)
            .Subscribe(values.Add);

        // Initially null, should not emit
        await Assert.That(values.Count).IsEqualTo(0);

        // Now set a real observable
        var subject = new Subject<int>();
        vm.Changes = subject;

        subject.OnNext(LateAssignedValue);

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(LateAssignedValue);
    }

    /// <summary>Resets and initializes the ReactiveUI binding infrastructure for testing.</summary>
    internal static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }
}
