// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;
using Xunit.Abstractions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// BindableDerivedListGeneratorTests.
/// </summary>
public class BindableDerivedListGeneratorTests(ITestOutputHelper output) : TestBase<BindableDerivedListGenerator>(output)
{
    /// <summary>
    /// Tests that the source generator correctly generates reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromReactiveProperties()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System.Collections.ObjectModel;
                using DynamicData;

                namespace TestNs;

                public partial class TestVM
                {
                    [BindableDerivedList]
                    private ReadOnlyObservableCollection<int> _test1;
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }
}
