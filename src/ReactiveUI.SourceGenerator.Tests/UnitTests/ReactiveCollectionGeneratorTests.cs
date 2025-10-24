// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerator.Tests;

namespace ReactiveUI.SourceGenerators.Tests;
/// <summary>
/// ReactiveCollectionGeneratorTests.
/// </summary>
[TestFixture]
public class ReactiveCollectionGeneratorTests : TestBase<ReactiveCollectionGenerator>
{
    /// <summary>
    /// Tests that the source generator correctly generates reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCollectionField()
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
