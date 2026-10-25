// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Reflection;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Tests for <see cref="BindingErrors"/>, which carries the fault contract a binding's write follows.</summary>
public class BindingErrorsTests
{
    /// <summary>The expression name the tests report faults against.</summary>
    private const string BoundExpression = "x => x.Caption";

    /// <summary>A value written through the binding.</summary>
    private const int WrittenValue = 42;

    /// <summary>The values a single successful write is expected to produce.</summary>
    private static readonly int[] ExpectedWrite = [WrittenValue];

    /// <summary>Values reaching the write are forwarded to it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_WhenSourceEmits_AppliesTheWrite()
    {
        var subject = new Subject<int>();
        var written = new List<int>();

        using var subscription = BindingErrors.Subscribe(subject, written.Add, BoundExpression);
        subject.OnNext(WrittenValue);

        await Assert.That(written).IsEquivalentTo(ExpectedWrite);
    }

    /// <summary>Completion is not an error and reaches nobody.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_WhenSourceCompletes_DoesNotThrow()
    {
        var subject = new Subject<int>();
        var written = new List<int>();

        using var subscription = BindingErrors.Subscribe(subject, written.Add, BoundExpression);
        subject.OnCompleted();

        await Assert.That(written).IsEmpty();
    }

    /// <summary>
    /// A fault with no inner exception is the sequence itself ending in error, which the subscriber can
    /// already observe, so it is recorded and not rethrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_WhenSourceFaultsWithoutInnerException_DoesNotRethrow()
    {
        var subject = new Subject<int>();

        using var subscription = BindingErrors.Subscribe(subject, static _ => { }, BoundExpression);

        await Assert.That(() => subject.OnError(new InvalidOperationException("no inner"))).ThrowsNothing();
    }

    /// <summary>A fault carrying an inner exception is a setter that threw, and is rethrown so it is not lost.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_WhenSourceFaultsWithInnerException_RethrowsAsTargetInvocation()
    {
        var subject = new Subject<int>();
        var inner = new InvalidOperationException("the setter threw");

        using var subscription = BindingErrors.Subscribe(subject, static _ => { }, BoundExpression);

        var thrown = await Assert.That(() => subject.OnError(new InvalidOperationException("outer", inner)))
            .Throws<TargetInvocationException>();

        await Assert.That(thrown!.InnerException).IsSameReferenceAs(inner);
        await Assert.That(thrown.Message).Contains(BoundExpression);
    }

    /// <summary>A null source is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_NullSource_Throws() =>
        await Assert.That(static () => BindingErrors.Subscribe<int>(null!, static _ => { }, BoundExpression))
            .Throws<ArgumentNullException>();

    /// <summary>A null write is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_NullOnNext_Throws() =>
        await Assert.That(static () => BindingErrors.Subscribe(new Subject<int>(), null!, BoundExpression))
            .Throws<ArgumentNullException>();
}
