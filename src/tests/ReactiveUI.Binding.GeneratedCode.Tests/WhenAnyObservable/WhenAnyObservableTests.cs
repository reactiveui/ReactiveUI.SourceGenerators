// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenAnyObservable;

/// <summary>Tests that the source-generator-generated WhenAnyObservable code (Switch/Merge) works correctly at runtime.</summary>
public class WhenAnyObservableTests
{
    /// <summary>The value emitted from the first observable.</summary>
    private const string From1 = "From1";

    /// <summary>The value emitted from the second observable.</summary>
    private const string From2 = "From2";

    /// <summary>Verifies that single observable Switch pattern emits inner values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleObservable_Switch_EmitsInnerValues()
    {
        var subject = new Subject<string>();
        var vm = new ObservablePropertyViewModel { MyCommand = subject };
        var values = new List<string>();

        using var sub = WhenAnyObservableScenarios.SingleObservable_Switch(vm)
            .Subscribe(values.Add);

        subject.OnNext("Hello");

        await Assert.That(values).Contains("Hello");
    }

    /// <summary>Verifies that single observable Switch pattern resubscribes when the observable property changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleObservable_Switch_ResubscribesOnNewObservable()
    {
        var subject1 = new Subject<string>();
        var vm = new ObservablePropertyViewModel { MyCommand = subject1 };
        var values = new List<string>();

        using var sub = WhenAnyObservableScenarios.SingleObservable_Switch(vm)
            .Subscribe(values.Add);

        subject1.OnNext(From1);

        await Assert.That(values).Contains(From1);

        // Replace with new observable
        var subject2 = new Subject<string>();
        vm.MyCommand = subject2;

        subject2.OnNext(From2);

        await Assert.That(values).Contains(From2);

        // Old observable should no longer propagate
        subject1.OnNext("StaleFrom1");

        await Assert.That(values).DoesNotContain("StaleFrom1");
    }

    /// <summary>Verifies that two observable Merge pattern emits from both streams.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoObservables_Merge_EmitsBothStreams()
    {
        var subject1 = new Subject<string>();
        var subject2 = new Subject<string>();
        var vm = new ObservablePropertyViewModel { MyCommand = subject1, OtherCommand = subject2 };
        var values = new List<string>();

        using var sub = WhenAnyObservableScenarios.TwoObservables_Merge(vm)
            .Subscribe(values.Add);

        subject1.OnNext(From1);
        subject2.OnNext(From2);

        await Assert.That(values).Contains(From1);
        await Assert.That(values).Contains(From2);
    }

    /// <summary>Verifies that disposing the WhenAnyObservable subscription stops listening.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsListening()
    {
        var subject = new Subject<string>();
        var vm = new ObservablePropertyViewModel { MyCommand = subject };
        var values = new List<string>();

        var sub = WhenAnyObservableScenarios.SingleObservable_Switch(vm)
            .Subscribe(values.Add);

        subject.OnNext("Before");

        await Assert.That(values).Contains("Before");

        sub.Dispose();

        subject.OnNext("After");

        await Assert.That(values).DoesNotContain("After");
    }
}
