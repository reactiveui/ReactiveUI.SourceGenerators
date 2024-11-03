// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;
using Xunit.Abstractions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for the Reactive generator.
/// </summary>
/// <param name="output">The output helper.</param>
public class ReactiveGeneratorTests(ITestOutputHelper output) : TestBase<ReactiveGenerator>(output)
{
    /// <summary>
    /// Tests that the source generator correctly generates reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromReactiveProperties()
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
                    [Reactive]
                    private int _test1 = 10;
                }
            """;

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath());
    }

    /// <summary>
    /// Tests that the source generator correctly generates reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromReactivePropertiesWithAccess()
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
                    [Reactive(SetModifier = AccessModifier.Protected)]
                    private int _test2 = 10;
                }
            """;

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath());
    }

    /// <summary>
    /// Froms the reactive properies with attributes.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromReactiveProperiesWithAttributes()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = @"
                using System;
                using System.Runtime.Serialization;
                using System.Text.Json.Serialization;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    [property: JsonInclude]
                    [DataMember]
                    [Reactive]
                    private int _test3 = 10;
                }
            ";

        // Act: Initialize the helper and run the generator.
        var driver = TestHelper.TestPass(sourceCode);

        // Assert: Verify the generated code.
        return Verify(driver).UseDirectory(TestHelper.VerifiedFilePath());
    }
}
