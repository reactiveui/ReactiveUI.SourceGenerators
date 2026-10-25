// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="SignatureGrouping"/>.</summary>
public class SignatureGroupingTests
{
    /// <summary>
    /// Call sites whose keys match share a group, and the groups come out in the order their keys were first seen,
    /// which keeps the generated file stable from one build to the next.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Group_SharedKeys_GroupsInFirstSeenOrderAndKeepsCallSiteOrder()
    {
        ImmutableArray<CallSite> callSites =
        [
            new("b", "first"),
            new("a", "second"),
            new("b", "third"),
            new("c", "fourth"),
            new("a", "fifth"),
        ];

        var groups = SignatureGrouping.Group(
            callSites,
            static (key, site) => _ = key.Append(site.Key),
            static (first, members) => new Group(first.Key, members));

        var described = string.Join(
            "; ",
            groups.Select(static g => $"{g.Key}: {string.Join(",", g.Members.Select(static m => m.Name))}"));
        await Assert.That(described).IsEqualTo("b: first,third; a: second,fifth; c: fourth");
    }

    /// <summary>The key writer gets a cleared builder for every call site, so one key never leaks into the next.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Group_KeyWriterAppends_StartsEachKeyEmpty()
    {
        ImmutableArray<CallSite> callSites = [new("x", "first"), new("x", "second")];

        var groups = SignatureGrouping.Group(
            callSites,
            static (key, site) => _ = key.Append(site.Key).Append('|'),
            static (first, members) => new Group(first.Key, members));

        await Assert.That(groups).HasSingleItem();
        await Assert.That(groups[0].Members.Length).IsEqualTo(callSites.Length);
    }

    /// <summary>No call sites produce no groups.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Group_NoCallSites_ReturnsNoGroups()
    {
        var groups = SignatureGrouping.Group(
            ImmutableArray<CallSite>.Empty,
            static (key, site) => _ = key.Append(site.Key),
            static (first, members) => new Group(first.Key, members));

        await Assert.That(groups).IsEmpty();
    }

    /// <summary>A call site the tests group.</summary>
    /// <param name="Key">The signature key the call site writes.</param>
    /// <param name="Name">Identifies the call site.</param>
    private sealed record CallSite(string Key, string Name);

    /// <summary>A group the tests build.</summary>
    /// <param name="Key">The key of the group's first call site.</param>
    /// <param name="Members">Every call site in the group.</param>
    private sealed record Group(string Key, CallSite[] Members);
}
