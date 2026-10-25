// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Binding.Tests.WhenAny;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Reaches every arity of the runtime WhenAnyObservable overloads, which follow the observable a property holds.</summary>
/// <remarks>
/// Each overload is called on the declaring class rather than as an extension method. Written as an
/// extension call the generated dispatch wins overload resolution, and the runtime overload these
/// assertions are about would never run.
/// </remarks>
public class WhenAnyObservableWideArityTests
{
    /// <summary>The value each stream is driven with.</summary>
    private const string EmittedValue = "a";

    /// <summary>The first and last stream's values read together, which is what the selectors project.</summary>
    private const string BothEnds = EmittedValue + EmittedValue;

    /// <summary>Following one stream reports what it emits.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingOneStream_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following two streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingTwoStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following three streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingThreeStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following four streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingFourStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following five streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingFiveStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following six streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingSixStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following seven streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingSevenStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following eight streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingEightStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                x => x.Stream8)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following nine streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingNineStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                x => x.Stream8,
                x => x.Stream9)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following ten streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingTenStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                x => x.Stream8,
                x => x.Stream9,
                x => x.Stream10)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following eleven streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingElevenStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                x => x.Stream8,
                x => x.Stream9,
                x => x.Stream10,
                x => x.Stream11)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Following twelve streams reports what they emit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_FollowingTwelveStreams_ReportsTheEmittedValue()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                x => x.Stream8,
                x => x.Stream9,
                x => x.Stream10,
                x => x.Stream11,
                x => x.Stream12)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(EmittedValue);
    }

    /// <summary>Combining two streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningTwoStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                static (v1, v2) => v1 + v2)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }

    /// <summary>Combining three streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningThreeStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                static (v1, v2, v3) => v1 + v3)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }

    /// <summary>Combining four streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningFourStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                static (v1, v2, v3, v4) => v1 + v4)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }

    /// <summary>Combining five streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningFiveStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                static (v1, v2, v3, v4, v5) => v1 + v5)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }

    /// <summary>Combining six streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningSixStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                static (v1, v2, v3, v4, v5, v6) => v1 + v6)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }

    /// <summary>Combining seven streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningSevenStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                static (v1, v2, v3, v4, v5, v6, v7) => v1 + v7)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }

    /// <summary>Combining eight streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningEightStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                x => x.Stream8,
                static (v1, v2, v3, v4, v5, v6, v7, v8) => v1 + v8)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }

    /// <summary>Combining nine streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningNineStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                x => x.Stream8,
                x => x.Stream9,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9) => v1 + v9)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }

    /// <summary>Combining ten streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningTenStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                x => x.Stream8,
                x => x.Stream9,
                x => x.Stream10,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10) => v1 + v10)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }

    /// <summary>Combining eleven streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningElevenStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                x => x.Stream8,
                x => x.Stream9,
                x => x.Stream10,
                x => x.Stream11,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11) => v1 + v11)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }

    /// <summary>Combining twelve streams reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CombiningTwelveStreams_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityObservableFixture();
        var streams = fixture.FillStreams();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyObservableUnsafe(
                fixture,
                x => x.Stream1,
                x => x.Stream2,
                x => x.Stream3,
                x => x.Stream4,
                x => x.Stream5,
                x => x.Stream6,
                x => x.Stream7,
                x => x.Stream8,
                x => x.Stream9,
                x => x.Stream10,
                x => x.Stream11,
                x => x.Stream12,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12) => v1 + v12)
            .Subscribe(seen.Add);

        foreach (var stream in streams)
        {
            // Only the streams this arity observes were subscribed to, so the rest have no observer.
            stream.Observer?.OnNext(EmittedValue);
        }

        await Assert.That(seen[^1]).IsEqualTo(BothEnds);
    }
}
