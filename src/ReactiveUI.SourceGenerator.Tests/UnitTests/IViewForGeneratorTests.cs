// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;
using Xunit.Abstractions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// IViewForGeneratorTests.
/// </summary>
public class IViewForGeneratorTests(ITestOutputHelper output) : TestBase<IViewForGenerator>(output)
{
    /// <summary>
    /// Tests that the source generator correctly generates reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromIViewFor()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System.Collections.ObjectModel;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;

                namespace TestNs;

                [IViewFor<TestViewModel>]
                public partial class TestViewWpf : Window
                {
                    /// <summary>
                    /// Initializes a new instance of the <see cref="TestViewWpf"/> class.
                    /// </summary>
                    public TestViewWpf() => ViewModel = TestViewModel.Instance;
                }

                public partial class TestViewModel : ReactiveObject
                {
                    /// <summary>
                    /// Gets the instance of the test view model.
                    /// </summary>
                    public static TestViewModel Instance { get; } = new();

                    /// <summary>
                    /// Gets or sets the test property.
                    /// </summary>
                    public int TestProperty { get; set; }
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }
}
