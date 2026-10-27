// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;
using LinqExpression = System.Linq.Expressions.Expression;

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>Tests for reading a single link of an expression member chain.</summary>
public class ChainLinkReaderTests
{
    /// <summary>The value the fixture holds.</summary>
    private const string HeldValue = "held";

    /// <summary>The array an index link is built over.</summary>
    private static readonly string[] IndexedValues = [HeldValue];

    /// <summary>Verifies that a property link compiles to a fetcher.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateGetter_ForPropertyLink_ReturnsAFetcher()
    {
        Expression<Func<ObservedValueFixture, string>> expr = x => x.Value;

        var getter = ChainLinkReader.CreateGetter(expr.Body);

        await Assert.That(getter).IsNotNull();
    }

    /// <summary>
    /// Verifies that an array index link compiles to nothing. Its expression carries no indexer, which is
    /// the one shape in a chain that names no member.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateGetter_ForArrayIndexLink_ReturnsNull()
    {
        var getter = ChainLinkReader.CreateGetter(ArrayIndexLink());

        await Assert.That(getter).IsNull();
    }

    /// <summary>Verifies that an absent parent reads as nothing rather than throwing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadValue_WithNoParent_ReturnsNull()
    {
        Expression<Func<ObservedValueFixture, string>> expr = x => x.Value;
        var getter = ChainLinkReader.CreateGetter(expr.Body);

        var value = ChainLinkReader.ReadValue(null, getter, null, expr.Body);

        await Assert.That(value).IsNull();
    }

    /// <summary>Verifies that a link with a fetcher reads through it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadValue_WithFetcher_ReadsThroughIt()
    {
        var fixture = new ObservedValueFixture { Value = HeldValue };
        Expression<Func<ObservedValueFixture, string>> expr = x => x.Value;
        var getter = ChainLinkReader.CreateGetter(expr.Body);

        var value = ChainLinkReader.ReadValue(fixture, getter, null, expr.Body);

        await Assert.That(value).IsEqualTo(HeldValue);
    }

    /// <summary>
    /// Verifies that a link with no fetcher falls back to reading the change itself, and that the fallback
    /// reports the link as unreadable rather than quietly producing a wrong value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadValue_WithoutFetcher_SurfacesTheUnreadableLink()
    {
        var fixture = new ObservedValueFixture { Value = HeldValue };
        var link = ArrayIndexLink();

        await Assert.That(() => ChainLinkReader.ReadValue(fixture, null, null, link))
            .ThrowsExactly<NotSupportedException>();
    }

    /// <summary>Builds an array index link, whose expression carries no indexer.</summary>
    /// <returns>The link expression.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IndexExpression ArrayIndexLink() =>
        LinqExpression.ArrayAccess(LinqExpression.Constant(IndexedValues), LinqExpression.Constant(0));
}
