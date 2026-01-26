// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Extended unit tests for the ObservableAsProperty generator covering edge cases.
/// </summary>
[TestFixture]
public class OapExtTests : TestBase<ObservableAsPropertyGenerator>
{
    /// <summary>
    /// Tests ObservableAsProperty with initial value.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithInitialValue()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;
            using System.Reactive.Linq;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty(InitialValue = "\"Loading...\"")]
                private string _status = string.Empty;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with initial value for numeric type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithNumericInitialValue()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty(InitialValue = "-1")]
                private int _count;

                [ObservableAsProperty(InitialValue = "0.0")]
                private double _percentage;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with combined options.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithCombinedOptions()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty(ReadOnly = false, UseProtected = true, PropertyName = "CustomStatus")]
                private string _statusField = string.Empty;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with generic types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithGenericTypes()
    {
        const string sourceCode = """
            using System;
            using System.Collections.Generic;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                private List<string>? _items;

                [ObservableAsProperty]
                private Dictionary<int, string>? _mappings;

                [ObservableAsProperty]
                private IReadOnlyList<int>? _numbers;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty from observable property with generic type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromObservablePropertyWithGenericType()
    {
        const string sourceCode = """
            using System;
            using System.Collections.Generic;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                public IObservable<List<string>> ItemsObservable => Observable.Return(new List<string>());

                [ObservableAsProperty]
                public IObservable<IReadOnlyDictionary<string, int>> MappingsObservable => 
                    Observable.Return<IReadOnlyDictionary<string, int>>(new Dictionary<string, int>());
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with nullable types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithNullableTypes()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                private int? _nullableInt;

                [ObservableAsProperty]
                private DateTime? _nullableDateTime;

                [ObservableAsProperty]
                private Guid? _nullableGuid;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty from observable method with parameters converted.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromObservableMethodWithCustomName()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty(PropertyName = "IsLoading")]
                public IObservable<bool> GetLoadingState() => Observable.Return(false);

                [ObservableAsProperty(PropertyName = "ErrorMessage")]
                public IObservable<string?> GetError() => Observable.Return<string?>(null);
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with tuple types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithTupleTypes()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                private (int Id, string Name)? _namedTuple;

                [ObservableAsProperty]
                private ValueTuple<string, int, bool> _valueTuple;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty from observable property with tuple.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromObservablePropertyWithTuple()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                public IObservable<(bool Success, string Message)> ResultObservable =>
                    Observable.Return((true, "Done"));
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with array types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithArrayTypes()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                private byte[]? _bytes;

                [ObservableAsProperty]
                private string[]? _strings;

                [ObservableAsProperty]
                private int[][]? _jaggedArray;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty in generic class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldInGenericClass()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class GenericVM<T> : ReactiveObject where T : class
            {
                [ObservableAsProperty]
                private T? _item;

                [ObservableAsProperty]
                public IObservable<T?> ItemObservable => Observable.Return<T?>(default);
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with enum types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithEnumTypes()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public enum Status { Pending, Active, Completed }

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                private Status _status;

                [ObservableAsProperty]
                private Status? _nullableStatus;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty from observable property with enum.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromObservablePropertyWithEnum()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public enum ConnectionState { Disconnected, Connecting, Connected }

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                public IObservable<ConnectionState> StateObservable => 
                    Observable.Return(ConnectionState.Disconnected);
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests multiple ObservableAsProperty in same class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromMultipleObservableAsProperties()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                private bool _isLoading;

                [ObservableAsProperty]
                private string? _errorMessage;

                [ObservableAsProperty]
                private int _itemCount;

                [ObservableAsProperty(UseProtected = true)]
                private double _progress;

                [ObservableAsProperty(ReadOnly = false)]
                private string _status = string.Empty;

                [ObservableAsProperty]
                public IObservable<bool> HasItems => Observable.Return(true);

                [ObservableAsProperty]
                public IObservable<string> DisplayName() => Observable.Return("Test");
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty in record class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldInRecordClass()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial record TestVMRecord : ReactiveObject
            {
                [ObservableAsProperty]
                private string? _status;

                [ObservableAsProperty]
                public IObservable<int> CountObservable => Observable.Return(0);
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with interface types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithInterfaceTypes()
    {
        const string sourceCode = """
            using System;
            using System.Collections.Generic;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public interface IItem
            {
                string Name { get; }
            }

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                private IItem? _currentItem;

                [ObservableAsProperty]
                private IDisposable? _subscription;

                [ObservableAsProperty]
                private IReadOnlyCollection<IItem>? _allItems;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty from observable property with interface.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromObservablePropertyWithInterface()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public interface IResult
            {
                bool Success { get; }
            }

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                public IObservable<IResult?> ResultObservable => Observable.Return<IResult?>(null);
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with TimeSpan and DateTimeOffset.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithTimeTypes()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                private TimeSpan _elapsed;

                [ObservableAsProperty]
                private DateTimeOffset _lastUpdated;

                [ObservableAsProperty]
                private TimeSpan? _remainingTime;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty in deeply nested class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldInDeeplyNestedClass()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class Level1 : ReactiveObject
            {
                [ObservableAsProperty]
                private int _level1Value;

                public partial class Level2 : ReactiveObject
                {
                    [ObservableAsProperty]
                    private int _level2Value;

                    [ObservableAsProperty]
                    public IObservable<string> Level2Observable => Observable.Return("L2");

                    public partial class Level3 : ReactiveObject
                    {
                        [ObservableAsProperty]
                        private int _level3Value;

                        public partial class Level4 : ReactiveObject
                        {
                            [ObservableAsProperty]
                            private int _level4Value;

                            [ObservableAsProperty]
                            public IObservable<bool> DeepObservable => Observable.Return(true);
                        }
                    }
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with delegate types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithDelegateTypes()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                private Func<int, string>? _formatter;

                [ObservableAsProperty]
                private Action? _callback;

                [ObservableAsProperty]
                private EventHandler? _handler;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with attributes.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromFieldWithAttributes()
    {
        const string sourceCode = """
            using System;
            using System.Runtime.Serialization;
            using System.Text.Json.Serialization;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [property: JsonPropertyName("computed_value")]
                [property: JsonInclude]
                [DataMember(Name = "computedValue")]
                [ObservableAsProperty]
                private int _computedValue;

                [property: JsonIgnore]
                [IgnoreDataMember]
                [ObservableAsProperty]
                public IObservable<string> InternalState => Observable.Return("state");
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ObservableAsProperty with complex nested generics from observable.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromObservablePropertyWithComplexGenerics()
    {
        const string sourceCode = """
            using System;
            using System.Collections.Generic;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                public IObservable<Dictionary<string, List<int>>> ComplexDataObservable =>
                    Observable.Return(new Dictionary<string, List<int>>());

                [ObservableAsProperty]
                public IObservable<IReadOnlyDictionary<int, IReadOnlyList<string>>> NestedReadOnlyObservable =>
                    Observable.Return<IReadOnlyDictionary<int, IReadOnlyList<string>>>(
                        new Dictionary<int, IReadOnlyList<string>>());
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }
}
