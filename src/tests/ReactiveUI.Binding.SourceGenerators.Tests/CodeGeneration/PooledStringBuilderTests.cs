// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Covers the pooled fragment builder. It backs every generated fragment, so a defect here is a defect in the
/// generated code, and the buffer reuse means a fault can show up as one fragment leaking into the next rather
/// than as an obvious failure.
/// </summary>
public class PooledStringBuilderTests
{
    /// <summary>Enough segments to force the buffer to grow several times over.</summary>
    private const int GrowthSegmentCount = 2_000;

    /// <summary>An empty builder renders as the empty string.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Empty_RendersAsEmpty()
    {
        var builder = new PooledStringBuilder();

        await Assert.That(builder.Length).IsEqualTo(0);
        await Assert.That(builder.ToString()).IsEqualTo(string.Empty);
    }

    /// <summary>Strings, characters and booleans accumulate in order.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Appends_AccumulateInOrder()
    {
        var builder = new PooledStringBuilder();

        var rendered = builder
            .Append("Type").Append('|').Append(true).Append('|').Append(false)
            .ToStringAndReturn();

        await Assert.That(rendered).IsEqualTo("Type|True|False");
    }

    /// <summary>A null or empty append leaves the builder untouched.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendingNothing_LeavesTheContentUnchanged()
    {
        var builder = new PooledStringBuilder();

        var rendered = builder.Append("a").Append((string?)null).Append(string.Empty).Append("b").ToStringAndReturn();

        await Assert.That(rendered).IsEqualTo("ab");
    }

    /// <summary>Integers render exactly as the framework renders them, including the extremes.</summary>
    /// <param name="value">The value to render.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(9)]
    [Arguments(10)]
    [Arguments(-1)]
    [Arguments(-10)]
    [Arguments(1_234_567_890)]
    [Arguments(int.MaxValue)]
    [Arguments(int.MinValue)]
    public async Task Integers_RenderTheSameAsTheFramework(int value)
    {
        var rendered = new PooledStringBuilder().Append(value).ToStringAndReturn();

        await Assert.That(rendered).IsEqualTo(value.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Content longer than the rented buffer grows it without losing or reordering anything.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContentBeyondTheInitialCapacity_GrowsWithoutLoss()
    {
        var builder = new PooledStringBuilder(1);
        var expected = new System.Text.StringBuilder();

        for (var i = 0; i < GrowthSegmentCount; i++)
        {
            _ = builder.Append("segment").Append(i).Append('|');
            _ = expected.Append("segment").Append(i).Append('|');
        }

        await Assert.That(builder.Length).IsEqualTo(expected.Length);
        await Assert.That(builder.ToStringAndReturn()).IsEqualTo(expected.ToString());
    }

    /// <summary>Clearing keeps the buffer and starts the next fragment from empty, which is the grouping-key pattern.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Clear_StartsTheNextFragmentFromEmpty()
    {
        var builder = new PooledStringBuilder();

        _ = builder.Append("first");
        var first = builder.ToString();
        _ = builder.Clear().Append("second");
        var second = builder.ToString();
        builder.Return();

        await Assert.That(first).IsEqualTo("first");
        await Assert.That(second).IsEqualTo("second");
    }

    /// <summary>Rendering leaves the builder usable, so a fragment can be read more than once.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rendering_DoesNotEmptyTheBuilder()
    {
        var builder = new PooledStringBuilder();

        _ = builder.Append("kept");
        var first = builder.ToString();
        var second = builder.ToString();
        builder.Return();

        await Assert.That(first).IsEqualTo("kept");
        await Assert.That(second).IsEqualTo("kept");
    }

    /// <summary>
    /// A builder taken after another was returned starts empty. The buffer is reused, so a stale write position
    /// would show up here as the previous fragment's tail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ABuilderTakenAfterAReturn_StartsEmpty()
    {
        var first = new PooledStringBuilder();
        _ = first.Append("previous fragment");
        first.Return();

        var second = new PooledStringBuilder();
        var rendered = second.Append("next").ToStringAndReturn();

        await Assert.That(rendered).IsEqualTo("next");
    }

    /// <summary>Returning twice does not hand the same buffer out to two live builders.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReturningTwice_DoesNotShareOneBufferBetweenTwoBuilders()
    {
        var builder = new PooledStringBuilder();
        _ = builder.Append("content");
        builder.Return();
        builder.Return();

        var first = new PooledStringBuilder();
        var second = new PooledStringBuilder();

        _ = first.Append("aaaa");
        _ = second.Append("bbbb");

        await Assert.That(first.ToStringAndReturn()).IsEqualTo("aaaa");
        await Assert.That(second.ToStringAndReturn()).IsEqualTo("bbbb");
    }
}
