// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;
using Xunit.Abstractions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for the ObservableAsProperty generator.
/// </summary>
/// <param name="output">The output helper.</param>
public class OAPGeneratorTests(ITestOutputHelper output) : TestBase(output)
{
    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromField()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    [ObservableAsProperty]
                    private int _test1 = 10;
                }
            """;

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass<ObservableAsPropertyGenerator>(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath(nameof(ObservableAsPropertyGenerator)));
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task NonReadOnlyFromField()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    [ObservableAsProperty(ReadOnly = false)]
                    private int _test2 = 10;
                }
            """;

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass<ObservableAsPropertyGenerator>(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath(nameof(ObservableAsPropertyGenerator)));
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task NamedFromField()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;
            using System.Reactive.Linq;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty(PropertyName = "MyNamedProperty")]
                private int _test2 = 10;
            }
            """;

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass<ObservableAsPropertyGenerator>(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath(nameof(ObservableAsPropertyGenerator)));
    }
}
