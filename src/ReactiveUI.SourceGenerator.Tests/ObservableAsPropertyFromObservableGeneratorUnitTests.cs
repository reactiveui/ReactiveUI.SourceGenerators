// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReactiveUI.SourceGenerators;

using Xunit.Abstractions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for the ObservableAsProperty generator.
/// </summary>
/// <param name="output">The output helper.</param>
public class ObservableAsPropertyFromObservableGeneratorUnitTests(ITestOutputHelper output) : TestBase(output)
{
    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task Generator_ShouldGenerate_ObservablePropertiesCorrectly()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        var sourceCode = @"
                using ReactiveUI;
                using System.Reactive.Linq;

                namespace TestNamespace
                {
                    public partial class TestViewModel : ReactiveObject
                    {
                        [ObservableAsProperty]
                        public IObservable<int> MyProperty => Observable.Return(42);
                    }
                }
            ";

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass<ObservableAsPropertyFromObservableGenerator>(
            sourceCode,
            "MyProperty",
            typeof(ObservableAsPropertyFromObservableGenerator));

        // Assert: Verify the generated code.
        return Verify(driver);
    }
}
