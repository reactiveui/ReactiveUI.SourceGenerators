// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for the Reactive generator.
/// </summary>
[TestFixture]
public class ReactiveObjectGeneratorTests : TestBase<ReactiveObjectGenerator>
{
    /// <summary>
    /// Tests the ReactiveObject generator with IReactiveObjectAttribute.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveObject()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;
                namespace TestNs;

                [IReactiveObject]
                public partial class TestVM
                {
                    [Reactive]
                    private int _test1 = 10;
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }
}
