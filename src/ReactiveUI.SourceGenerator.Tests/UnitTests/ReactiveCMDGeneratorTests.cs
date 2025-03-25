// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;
using Xunit.Abstractions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for the ReactiveCommand generator.
/// </summary>
/// <param name="output">The output helper.</param>
public class ReactiveCMDGeneratorTests(ITestOutputHelper output) : TestBase<ReactiveCommandGenerator>(output)
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
                    private int Test1() => 10;
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
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
                    [property: JsonInclude]
                    private int Test3(string baseString) => int.Parse(baseString);
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
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

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
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

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Froms the reactive command with output scheduler.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromReactiveCommandWithOutputScheduler()
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
                    [ReactiveCommand(OutputScheduler = "RxApp.MainThreadScheduler")]
                    private int Test1() => 10;
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Froms the reactive command with nested classes.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromReactiveCommandWithNestedClasses()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;
                using System.Threading.Tasks;

                namespace TestNs;

                /// <summary>
                /// TestViewModel3.
                /// </summary>
                public partial class TestViewModel3 : ReactiveObject
                {
                    [ReactiveCommand]
                    private int Test1() => 10;

                    /// <summary>
                    /// TestInnerClass.
                    /// </summary>
                    public partial class TestInnerClass1 : ReactiveObject
                    {
                        [ReactiveCommand]
                        private int TestI1() => 10;
                    }

                    /// <summary>
                    /// TestInnerClass.
                    /// </summary>
                    public partial class TestInnerClass2 : ReactiveObject
                    {
                        [ReactiveCommand]
                        private int TestI2() => 10;

                        /// <summary>
                        /// TestInnerClass4.
                        /// </summary>
                        /// <seealso cref="ReactiveUI.ReactiveObject" />
                        public partial class TestInnerClass3 : ReactiveObject
                        {
                            [ReactiveCommand]
                            private int TestI3() => 10;
                        }
                    }
                }            
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }
}
