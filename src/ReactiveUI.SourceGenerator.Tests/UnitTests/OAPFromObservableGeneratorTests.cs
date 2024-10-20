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
public class OAPFromObservableGeneratorTests(ITestOutputHelper output) : TestBase(output)
{
    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromObservableProp()
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
                    public IObservable<int> Test1 => Observable.Return(42);
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
    public Task FromObservableMethods()
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
                    public IObservable<int> Test2() => Observable.Return(42);
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
    public Task FromObservableMethodsWithName()
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
                    public IObservable<int> Test3() => Observable.Return(42);
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
    public Task FromObservablePropertiesWithName()
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
                    public IObservable<int> Test4 => Observable.Return(42);
                }
            """;

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass<ObservableAsPropertyGenerator>(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath(nameof(ObservableAsPropertyGenerator)));
    }
}
