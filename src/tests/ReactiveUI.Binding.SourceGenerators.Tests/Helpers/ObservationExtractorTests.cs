// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Tests for <see cref="ObservationExtractor"/>, which reads the observation APIs' call sites.</summary>
public class ObservationExtractorTests
{
    /// <summary>The name <c>WhenChanged</c> and <c>WhenChanging</c> give the projection.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsSelectorParameterName_ConversionFunc_ReturnsTrue() =>
        await Assert.That(ObservationExtractor.IsSelectorParameterName("conversionFunc")).IsTrue();

    /// <summary>The name <c>WhenAny</c> and <c>WhenAnyValue</c> give the projection.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsSelectorParameterName_Selector_ReturnsTrue() =>
        await Assert.That(ObservationExtractor.IsSelectorParameterName("selector")).IsTrue();

    /// <summary>Another parameter the same overloads carry is not the projection.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsSelectorParameterName_Scheduler_ReturnsFalse() =>
        await Assert.That(ObservationExtractor.IsSelectorParameterName("scheduler")).IsFalse();

    /// <summary>A name as long as <c>conversionFunc</c> is still a different parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsSelectorParameterName_SameLengthAsConversionFunc_ReturnsFalse() =>
        await Assert.That(ObservationExtractor.IsSelectorParameterName("callerFilePath")).IsFalse();

    /// <summary>A name as long as <c>selector</c> is still a different parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsSelectorParameterName_SameLengthAsSelector_ReturnsFalse() =>
        await Assert.That(ObservationExtractor.IsSelectorParameterName("property")).IsFalse();
}
