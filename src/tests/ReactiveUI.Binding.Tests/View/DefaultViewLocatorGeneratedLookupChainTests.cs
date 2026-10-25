// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Tests.TestExecutors;
using TUnit.Core.Executors;

namespace ReactiveUI.Binding.Tests.View;

/// <summary>Tests that <see cref="DefaultViewLocator"/> consults every registered generated lookup.</summary>
[NotInParallel]
[TestExecutor<BindingBuilderTestExecutor>]
public class DefaultViewLocatorGeneratedLookupChainTests
{
    /// <summary>The number of lookups registered concurrently in each round.</summary>
    private const int ConcurrentLookupCount = 64;

    /// <summary>The fewest threads a round races, so registrations collide even on a runner with few cores.</summary>
    private const int MinimumRacingThreads = 8;

    /// <summary>The number of racing rounds.</summary>
    private const int RegistrationRounds = 8;

    /// <summary>Verifies each registered lookup resolves the view models it knows about.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoLookups_EachResolvesItsOwnViewModels()
    {
        DefaultViewLocator.SetGeneratedViewDispatch(static (vm, _) => vm is FirstViewModel ? new FirstView() : null);
        DefaultViewLocator.SetGeneratedViewDispatch(static (vm, _) => vm is SecondViewModel ? new SecondView() : null);
        var locator = new DefaultViewLocator();

        var first = locator.ResolveView(new FirstViewModel());
        var second = locator.ResolveView(new SecondViewModel());

        await Assert.That(first).IsTypeOf<FirstView>();
        await Assert.That(second).IsTypeOf<SecondView>();
    }

    /// <summary>Verifies the non-generic overload consults every registered lookup.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoLookups_NonGenericResolve_EachResolvesItsOwnViewModels()
    {
        DefaultViewLocator.SetGeneratedViewDispatch(static (vm, _) => vm is FirstViewModel ? new FirstView() : null);
        DefaultViewLocator.SetGeneratedViewDispatch(static (vm, _) => vm is SecondViewModel ? new SecondView() : null);
        var locator = new DefaultViewLocator();

        var first = locator.ResolveView((object)new FirstViewModel());
        var second = locator.ResolveView((object)new SecondViewModel());

        await Assert.That(first).IsTypeOf<FirstView>();
        await Assert.That(second).IsTypeOf<SecondView>();
    }

    /// <summary>Verifies the most recently registered lookup wins for a view model both lookups resolve.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DuplicateViewModel_MostRecentlyRegisteredLookupWins()
    {
        var earlier = new FirstView();
        var later = new FirstView();
        DefaultViewLocator.SetGeneratedViewDispatch((_, _) => earlier);
        DefaultViewLocator.SetGeneratedViewDispatch((_, _) => later);

        var result = new DefaultViewLocator().ResolveView(new FirstViewModel());

        await Assert.That(result).IsSameReferenceAs(later);
    }

    /// <summary>Verifies an earlier lookup serves a view model the most recent lookup cannot resolve.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DuplicateViewModel_LatestLookupResolvesNull_EarlierLookupServes()
    {
        var earlier = new FirstView();
        DefaultViewLocator.SetGeneratedViewDispatch((_, _) => earlier);
        DefaultViewLocator.SetGeneratedViewDispatch(static (_, _) => null);

        var result = new DefaultViewLocator().ResolveView(new FirstViewModel());

        await Assert.That(result).IsSameReferenceAs(earlier);
    }

    /// <summary>Verifies each lookup receives the requested contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Lookups_ReceiveTheNormalizedContract()
    {
        var contracts = new List<string>();
        DefaultViewLocator.SetGeneratedViewDispatch((_, contract) =>
        {
            contracts.Add(contract);
            return null;
        });
        DefaultViewLocator.SetGeneratedViewDispatch((_, contract) =>
        {
            contracts.Add(contract);
            return null;
        });

        _ = new DefaultViewLocator().ResolveView(new FirstViewModel(), null);

        await Assert.That(string.Join("|", contracts)).IsEqualTo("|");
    }

    /// <summary>Verifies the lookups are consulted from the most recently registered to the first.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LookupsAreConsultedMostRecentFirst()
    {
        var order = new List<string>();
        DefaultViewLocator.SetGeneratedViewDispatch((_, _) =>
        {
            order.Add("first");
            return null;
        });
        DefaultViewLocator.SetGeneratedViewDispatch((_, _) =>
        {
            order.Add("second");
            return null;
        });
        DefaultViewLocator.SetGeneratedViewDispatch((_, _) =>
        {
            order.Add("third");
            return null;
        });

        _ = new DefaultViewLocator().ResolveView(new FirstViewModel());

        await Assert.That(string.Join(",", order)).IsEqualTo("third,second,first");
    }

    /// <summary>Verifies registering the same lookup again consults it once.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisteringTheSameLookupAgain_ConsultsItOnce()
    {
        var calls = 0;
        Func<object, string, IViewFor?> lookup = (_, _) =>
        {
            calls++;
            return null;
        };
        DefaultViewLocator.SetGeneratedViewDispatch(lookup);
        DefaultViewLocator.SetGeneratedViewDispatch(lookup);

        _ = new DefaultViewLocator().ResolveView(new FirstViewModel());

        await Assert.That(calls).IsEqualTo(1);
    }

    /// <summary>Verifies concurrent registrations all take effect, including those that lose a race and retry.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    /// <remarks>
    /// Each round starts its threads together at a barrier, so registrations collide and the losers retry against the
    /// array the winner published. Several rounds keep that collision from depending on the runner's core count.
    /// </remarks>
    [Test]
    public async Task ConcurrentRegistrations_AllTakeEffect()
    {
        var threads = Math.Max(Environment.ProcessorCount, MinimumRacingThreads);
        var perThread = (ConcurrentLookupCount / threads) + 1;
        var total = threads * perThread * RegistrationRounds;
        for (var round = 0; round < RegistrationRounds; round++)
        {
            using var barrier = new Barrier(threads);
            var offset = round * threads * perThread;
            var workers = new Thread[threads];
            for (var t = 0; t < threads; t++)
            {
                var first = offset + (t * perThread);
                workers[t] = new(() =>
                {
                    barrier.SignalAndWait();
                    for (var i = first; i < first + perThread; i++)
                    {
                        var number = i;
                        DefaultViewLocator.SetGeneratedViewDispatch((vm, _) => vm is NumberedViewModel { Number: var n } && n == number ? new FirstView() : null);
                    }
                });
                workers[t].Start();
            }

            foreach (var worker in workers)
            {
                worker.Join();
            }
        }

        var locator = new DefaultViewLocator();

        var unresolved = 0;
        for (var i = 0; i < total; i++)
        {
            if (locator.ResolveView(new NumberedViewModel(i)) is null)
            {
                unresolved++;
            }
        }

        await Assert.That(unresolved).IsEqualTo(0);
    }

    /// <summary>Verifies resetting removes every registered lookup.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Reset_RemovesEveryLookup()
    {
        DefaultViewLocator.SetGeneratedViewDispatch(static (vm, _) => vm is FirstViewModel ? new FirstView() : null);
        DefaultViewLocator.SetGeneratedViewDispatch(static (vm, _) => vm is SecondViewModel ? new SecondView() : null);

        DefaultViewLocator.ResetGeneratedViewDispatchForTesting();
        var locator = new DefaultViewLocator();

        await Assert.That(locator.ResolveView(new FirstViewModel())).IsNull();
        await Assert.That(locator.ResolveView(new SecondViewModel())).IsNull();
    }

    /// <summary>A view model the first lookup resolves.</summary>
    private sealed class FirstViewModel
    {
        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>A view model the second lookup resolves.</summary>
    private sealed class SecondViewModel
    {
        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>The view of <see cref="FirstViewModel"/>.</summary>
    private sealed class FirstView : IViewFor
    {
        /// <inheritdoc/>
        public object? ViewModel { get; set; }
    }

    /// <summary>The view of <see cref="SecondViewModel"/>.</summary>
    private sealed class SecondView : IViewFor
    {
        /// <inheritdoc/>
        public object? ViewModel { get; set; }
    }

    /// <summary>A view model identified by a number.</summary>
    /// <param name="Number">The number that identifies the view model.</param>
    private sealed record NumberedViewModel(int Number);
}
