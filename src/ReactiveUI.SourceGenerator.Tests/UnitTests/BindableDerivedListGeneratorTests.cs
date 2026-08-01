// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests bindable derived-list source generation.</summary>
public class BindableDerivedListGeneratorTests : TestBase<BindableDerivedListGenerator>
{
    /// <summary>Tests that the source generator correctly generates reactive properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
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
