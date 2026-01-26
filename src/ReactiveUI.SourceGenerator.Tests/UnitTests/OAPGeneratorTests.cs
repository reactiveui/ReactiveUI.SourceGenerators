// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for the ObservableAsProperty generator.
/// </summary>
[TestFixture]
public class OAPGeneratorTests : TestBase<ObservableAsPropertyGenerator>
{
    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
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

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldNestedClass()
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
                    private int _test2 = 10;

                    public partial class TestVMInner1 : ReactiveObject
                    {
                        [ObservableAsProperty]
                        private int _testIn1 = 10;
                    }
            
                    public partial class TestVMInner2 : ReactiveObject
                    {
                        [ObservableAsProperty]
                        private int _testIn2 = 10;
            
                        public partial class TestVMInner3 : ReactiveObject
                        {
                            [ObservableAsProperty]
                            private int _testIn3 = 10;
                        }
                    }
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
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
                    private int _test3 = 10;
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
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
                private int _test4 = 10;
            }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task NonReadOnlyFromFieldProtected()
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
                    [ObservableAsProperty(UseProtected = true)]
                    private int _test5 = 10;
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }
}
