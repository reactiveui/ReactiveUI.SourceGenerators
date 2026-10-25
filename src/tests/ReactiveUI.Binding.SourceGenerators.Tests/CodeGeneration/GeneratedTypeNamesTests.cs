// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="GeneratedTypeNames"/>, which renders the type and member names generated code refers to.</summary>
public class GeneratedTypeNamesTests
{
    /// <summary>The fully qualified name of the view model these tests observe.</summary>
    private const string ViewModelTypeName = "global::TestApp.MyViewModel";

    /// <summary>The observation callback's argument, which the read is cast from.</summary>
    private const string ObserverArgument = "__o";

    /// <summary>A property declared as the type the observation is cast to is read straight off it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadProperty_SegmentNeedsNoNarrowing_ReadsThePropertyDirectly()
    {
        var segment = ModelFactory.CreatePropertyPathSegment();

        var result = GeneratedTypeNames.ReadProperty(segment, ViewModelTypeName, ObserverArgument);

        await Assert.That(result).IsEqualTo("((global::TestApp.MyViewModel)__o).Name");
    }

    /// <summary>
    /// A view exposing its view model as a base hands back the base type, so the read narrows to the view
    /// model the call site named before the chain continues from it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadProperty_SegmentCarriesAReadCast_NarrowsTheRead()
    {
        var segment = new PropertyPathSegment(
            "ViewModel",
            ViewModelTypeName,
            "global::TestApp.MyView",
            true,
            null,
            ViewModelTypeName);

        var result = GeneratedTypeNames.ReadProperty(segment, "global::TestApp.MyView", ObserverArgument);

        await Assert.That(result)
            .IsEqualTo("((global::TestApp.MyViewModel)(object)((global::TestApp.MyView)__o).ViewModel)");
    }
}
