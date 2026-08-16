// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for the ObservableAsProperty generator.</summary>
public class OAPFromObservableGeneratorTests : TestBase<ObservableAsPropertyGenerator>
{
    /// <summary>Tests that the source generator correctly generates observable properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
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

    /// <summary>Tests that the source generator correctly generates observable properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Nested()
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

    /// <summary>Tests that the source generator correctly generates observable properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
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

    /// <summary>Tests that the source generator correctly generates observable properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
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

    /// <summary>Tests that the source generator correctly generates observable properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithName()
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

    /// <summary>Tests that the source generator correctly generates observable properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithAttr()
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

    /// <summary>Tests that the source generator correctly generates observable properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task AttrRef()
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

    /// <summary>Tests that the source generator correctly generates observable properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task AttrNullRef()
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

    /// <summary>Tests that the source generator correctly generates observable properties.</summary>
    /// <returns>
    /// A task to monitor the async.
    /// </returns>
    [Test]
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

    /// <summary>Tests that the source generator correctly generates observable properties.</summary>
    /// <returns>
    /// A task to monitor the async.
    /// </returns>
    [Test]
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

    /// <summary>Tests that an empty string initial value initialises the generated backing field.</summary>
    /// <returns>
    /// A task to monitor the async.
    /// </returns>
    [Test]
    public Task FromPartialPropertyWithEmptyStringInitialValue()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;
                using System.Reactive.Subjects;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    private readonly Subject<string> _testSubject = new();

                    public TestVM()
                    {
                        _pLCActiveHelper = _testSubject.ToProperty(this, nameof(PLCActive));
                    }

                    [ObservableAsProperty(InitialValue = "")]
                    public partial string PLCActive { get; }
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>Tests that a non nullable string without an initial value defaults to an empty string.</summary>
    /// <returns>
    /// A task to monitor the async.
    /// </returns>
    [Test]
    public Task FromPartialPropertyWithoutInitialValue()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;
                using System.Reactive.Linq;
                using System.Reactive.Subjects;

                namespace TestNs;

                public partial class TestVM : ReactiveObject
                {
                    private readonly Subject<string> _testSubject = new();
                    private readonly Subject<string?> _testNullableSubject = new();

                    public TestVM()
                    {
                        _pLCActiveHelper = _testSubject.ToProperty(this, nameof(PLCActive));
                        _pLCNameHelper = _testNullableSubject.ToProperty(this, nameof(PLCName));
                    }

                    [ObservableAsProperty]
                    public partial string PLCActive { get; }

                    [ObservableAsProperty]
                    public partial string? PLCName { get; }
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }
}
