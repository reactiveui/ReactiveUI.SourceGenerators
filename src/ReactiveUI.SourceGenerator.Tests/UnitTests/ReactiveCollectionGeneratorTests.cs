// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerator.Tests;

namespace ReactiveUI.SourceGenerators.Tests;

/// <summary>Tests reactive collection source generation.</summary>
public class ReactiveCollectionGeneratorTests : TestBase<ReactiveCollectionGenerator>
{
    /// <summary>Tests that the source generator correctly generates reactive properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task BasicField()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System.Collections.ObjectModel;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    [ReactiveCollection]
                    private ObservableCollection<int>? _publicObservableCollectionTest;
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }
}
