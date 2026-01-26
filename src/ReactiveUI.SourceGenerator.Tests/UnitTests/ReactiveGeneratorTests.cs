// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for the Reactive generator.
/// </summary>
[TestFixture]
public class ReactiveGeneratorTests : TestBase<ReactiveGenerator>
{
    /// <summary>
    /// Tests that the source generator correctly generates reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Basic()
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

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task CalledValue()
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
                    private string value = string.Empty;
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithAccess()
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

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Froms the reactive properies with attributes.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithAttributes()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
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
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Froms the reactive properties with attributes and access and inheritance.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithAttrAccessInherit()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
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
                    [Reactive(Inheritance = InheritanceModifier.Virtual, SetModifier = AccessModifier.Protected)]
                    private string? _name;
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Froms the reactive properties with attributes and access and inheritance.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithIdenticalClass()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using System.Runtime.Serialization;
                using System.Text.Json.Serialization;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;

                namespace TestNs1
                {
                    public partial class TestVM : ReactiveObject
                    {
                        [property: JsonInclude]
                        [DataMember]
                        [Reactive(Inheritance = InheritanceModifier.Virtual, SetModifier = AccessModifier.Protected)]
                        private string? _name;
                    }
                }

                namespace TestNs2
                {
                    public partial class TestVM : ReactiveObject
                    {
                        [property: JsonInclude]
                        [DataMember]
                        [Reactive(Inheritance = InheritanceModifier.Virtual, SetModifier = AccessModifier.Protected)]
                        private string? _name;
                    }
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Froms the reactive properties with nested class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithNestedClass()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using System.Runtime.Serialization;
                using System.Text.Json.Serialization;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;

                namespace TestNs1
                {
                    /// <summary>
                    /// TestViewModel3.
                    /// </summary>
                    public partial class TestViewModel3 : ReactiveObject
                    {
                        [Reactive]
                        private float _testVM3Property;

                        [Reactive]
                        private float _testVM3Property2;

                        /// <summary>
                        /// TestInnerClass.
                        /// </summary>
                        public partial class TestInnerClass1 : ReactiveObject
                        {
                            [Reactive]
                            private int _testInner1;

                            [Reactive]
                            private int _testInner11;
                        }

                        /// <summary>
                        /// TestInnerClass.
                        /// </summary>
                        public partial class TestInnerClass2 : ReactiveObject
                        {
                            [Reactive]
                            private int _testInner2;

                            [Reactive]
                            private int _testInner22;

                            /// <summary>
                            /// TestInnerClass4.
                            /// </summary>
                            /// <seealso cref="ReactiveUI.ReactiveObject" />
                            public partial class TestInnerClass3 : ReactiveObject
                            {
                                [Reactive]
                                private int _testInner3;

                                [Reactive]
                                private int _testInner33;
                            }
                        }
                    }
                }
                """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithInit()
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
                    [Reactive(SetModifier = AccessModifier.Init, UseRequired = true)]
                    private string _mustBeSet;
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Partial()
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
                    public int Test1 { get; set; }
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property generation from a partial class with AlsoNotify support.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task PartialAlsoNotify()
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
                    [Reactive(nameof(OtherNotifyProperty))]
                    private int _test4;

                    // This property is notified when Test4 changes
                    public int OtherNotifyProperty { get; set; }
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }
}
