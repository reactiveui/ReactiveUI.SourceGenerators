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
public class OAPFromObservableGeneratorTests(ITestOutputHelper output) : TestBase<ObservableAsPropertyGenerator>(output)
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

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromObservablePropNestedClasses()
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
            
                    public partial class TestVMInner1 : ReactiveObject
                    {
                        [ObservableAsProperty]
                        public IObservable<int> TestIn1 => Observable.Return(42);
                    }
            
                    public partial class TestVMInner2 : ReactiveObject
                    {
                        [ObservableAsProperty]
                        public IObservable<int> TestIn2 => Observable.Return(42);
            
                        public partial class TestVMInner3 : ReactiveObject
                        {
                            [ObservableAsProperty]
                            public IObservable<int> TestIn3 => Observable.Return(42);
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

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
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

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
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

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromObservablePropertiesWithAttribute()
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
                    [ObservableAsProperty(PropertyName = "MyNamedProperty")]
                    [property: JsonInclude]
                    [DataMember]
                    public IObservable<int> Test5 => Observable.Return(42);
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromObservablePropertiesWithAttributeRef()
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
                    [ObservableAsProperty(PropertyName = "MyNamedProperty")]
                    [property: JsonInclude]
                    [DataMember]
                    public IObservable<object> Test6 => Observable.Return(new object());
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Fact]
    public Task FromObservablePropertiesWithAttributeNullableRef()
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
                    [ObservableAsProperty(PropertyName = "MyNamedProperty")]
                    [property: JsonInclude]
                    [DataMember]
                    public IObservable<object?> Test7 => Observable.Return(new object());
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>
    /// A task to monitor the async.
    /// </returns>
    [Fact]
    public Task FromField()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using System.Runtime.Serialization;
                using System.Text.Json.Serialization;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;
                using System.Reactive.Subjects;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    private readonly Subject<double?> _testSubject = new();

                    [property: JsonInclude]
                    [DataMember]
                    [ObservableAsProperty]
                    private double? _testProperty = 1.1d;

                    public TestVM()
                    {
                        _testPropertyHelper = _testSubject.ToProperty(this, nameof(TestProperty));
                    }
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests that the source generator correctly generates observable properties.
    /// </summary>
    /// <returns>
    /// A task to monitor the async.
    /// </returns>
    [Fact]
    public Task FromPartialProperty()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using System.Runtime.Serialization;
                using System.Text.Json.Serialization;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;
                using System.Reactive.Subjects;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    private readonly Subject<double?> _testSubject = new();

                    public TestVM()
                    {
                        _testPropertyHelper = _testSubject.ToProperty(this, nameof(TestProperty));
                    }
            
                    [JsonInclude]
                    [DataMember]
                    [ObservableAsProperty(InitialValue = "1.1d")]
                    public partial double? TestProperty { get; }
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }
}
