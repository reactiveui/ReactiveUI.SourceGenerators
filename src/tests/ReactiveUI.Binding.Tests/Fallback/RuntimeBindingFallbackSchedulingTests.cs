// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Tests.Mixins;
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Tests the runtime two-way bindings on an explicit sequencer, and re-rooting a path at the view.</summary>
[NotInParallel]
public class RuntimeBindingFallbackSchedulingTests
{
    /// <summary>The value the source starts with.</summary>
    private const string InitialValue = "Initial";

    /// <summary>The value assigned on the source after binding.</summary>
    private const string SourceEditedValue = "FromSource";

    /// <summary>The value assigned on the target after binding.</summary>
    private const string TargetEditedValue = "FromTarget";

    /// <summary>The expression text reported when a write faults.</summary>
    private const string BindingExpression = "x => x.Name";

    /// <summary>The caption the view model carries for the nested-lambda path.</summary>
    private const string Caption = "banana";

    /// <summary>The number of times <c>a</c> appears in <see cref="Caption"/>.</summary>
    private const int CountOfA = 3;

    /// <summary>A two-way binding on an explicit sequencer carries values in both directions.</summary>
    /// <param name="immediate">Whether to use the immediate sequencer or queue on the current thread.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task BindTwoWay_WithSequencer_CarriesBothDirections(bool immediate)
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var source = new TestViewModel { Name = InitialValue };
        var target = new TestViewModel();

        using var binding = RuntimeBindingFallback.BindTwoWay(
            source,
            target,
            static x => x.Name,
            static x => x.Name,
            immediate ? Sequencer.Immediate : Sequencer.CurrentThread,
            BindingExpression);

        source.Name = SourceEditedValue;
        var afterSourceEdit = target.Name;
        target.Name = TargetEditedValue;

        using (Assert.Multiple())
        {
            await Assert.That(afterSourceEdit).IsEqualTo(SourceEditedValue);
            await Assert.That(source.Name).IsEqualTo(TargetEditedValue);
        }
    }

    /// <summary>Re-rooting a path at the view leaves a nested lambda's own parameter in place.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RootAtViewModel_NestedLambda_ReplacesOnlyTheViewModelParameter()
    {
        var view = new ViewModelRootedUnsafeBindingTests.TypedStubView { ViewModel = new() { Caption = Caption } };

        var rooted = RuntimeBindingFallback.RootAtViewModel<DispatchStubViewModel, int>(
            static vm => vm.Caption.Count(static c => c == 'a'));

        await Assert.That(rooted.Compile()(view)).IsEqualTo(CountOfA);
    }
}
