// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;
using Xunit.Abstractions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for the ReactiveCommand generator.
/// </summary>
/// <param name="output">The output helper.</param>
public class ReactiveCMDGeneratorTests(ITestOutputHelper output) : TestBase(output)
{
    /// <summary>
    /// Tests that the source generator correctly generates ReactiveCommands.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromReactiveCommand()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;
                using System.Threading.Tasks;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    [ReactiveCommand]
                    private void Test1()
                    {
                        var a = 10;
                    }
                }
            """;

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass<ReactiveCommandGenerator>(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath(nameof(ReactiveCommandGenerator)));
    }

    /// <summary>
    /// Tests that the source generator correctly generates ReactiveCommands.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromReactiveCommandWithParameter()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;
                using System.Threading.Tasks;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    [ReactiveCommand]
                    private void Test3(string baseString)
                    {
                        var a = baseString;
                    }
                }
            """;

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass<ReactiveCommandGenerator>(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath(nameof(ReactiveCommandGenerator)));
    }

    /// <summary>
    /// Tests that the source generator correctly generates ReactiveCommands.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromReactiveAsyncCommand()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;
                using System.Threading.Tasks;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    [ReactiveCommand]
                    private async Task Test1()
                    {
                        await Task.Delay(1000);
                    }
                }
            """;

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass<ReactiveCommandGenerator>(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath(nameof(ReactiveCommandGenerator)));
    }

    /// <summary>
    /// Tests that the source generator correctly generates ReactiveCommands.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromReactiveAsyncCommandWithParameter()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;
                using System.Threading.Tasks;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    [ReactiveCommand]
                    private async Task Test3(string baseString)
                    {
                        var a = baseString;
                        await Task.Delay(1000);
                    }
                }
            """;

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass<ReactiveCommandGenerator>(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath(nameof(ReactiveCommandGenerator)));
    }
}
