// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerator.Tests;

namespace ReactiveUI.SourceGenerators.Tests;

/// <summary>
/// Extended unit tests for the ReactiveCollection generator covering edge cases.
/// </summary>
[TestFixture]
public class RxCollExtTests : TestBase<ReactiveCollectionGenerator>
{
    /// <summary>
    /// Tests ReactiveCollection with multiple collections.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Multiple()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<string>? _names;

                [ReactiveCollection]
                private ObservableCollection<int>? _numbers;

                [ReactiveCollection]
                private ObservableCollection<double>? _values;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with complex generic type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ComplexType()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public class ItemModel
            {
                public int Id { get; set; }
                public string? Name { get; set; }
            }

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<ItemModel>? _items;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection in nested class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Nested()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class OuterVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<int>? _outerCollection;

                public partial class InnerVM : ReactiveObject
                {
                    [ReactiveCollection]
                    private ObservableCollection<string>? _innerCollection;

                    public partial class DeepInnerVM : ReactiveObject
                    {
                        [ReactiveCollection]
                        private ObservableCollection<double>? _deepCollection;
                    }
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with generic class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Generic()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class GenericVM<T> : ReactiveObject where T : class
            {
                [ReactiveCollection]
                private ObservableCollection<T>? _items;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with record type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Record()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial record TestVMRecord : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<int>? _numbers;

                [ReactiveCollection]
                private ObservableCollection<string>? _names;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with interface element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Interface()
    {
        const string sourceCode = """
            using System;
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public interface IItem
            {
                int Id { get; }
                string Name { get; }
            }

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<IItem>? _items;

                [ReactiveCollection]
                private ObservableCollection<IDisposable>? _disposables;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with tuple type elements.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Tuple()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<(int Id, string Name)>? _tuples;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection in different namespaces.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task DiffNs()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace Namespace1
            {
                public partial class TestVM : ReactiveObject
                {
                    [ReactiveCollection]
                    private ObservableCollection<int>? _numbers;
                }
            }

            namespace Namespace2
            {
                public partial class TestVM : ReactiveObject
                {
                    [ReactiveCollection]
                    private ObservableCollection<string>? _names;
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with enum element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Enum()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public enum Status { Pending, Active, Completed }

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<Status>? _statuses;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with struct element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Struct()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public struct Point
            {
                public int X { get; set; }
                public int Y { get; set; }
            }

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<Point>? _points;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with Guid element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Guid()
    {
        const string sourceCode = """
            using System;
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<Guid>? _ids;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with DateTime element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task DateTime()
    {
        const string sourceCode = """
            using System;
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<DateTime>? _dates;

                [ReactiveCollection]
                private ObservableCollection<DateTimeOffset>? _timestamps;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection combined with reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithReactive()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private string? _searchText;

                [Reactive]
                private bool _isLoading;

                [ReactiveCollection]
                private ObservableCollection<string>? _items;

                [ReactiveCollection]
                private ObservableCollection<int>? _selectedIds;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with nullable element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Nullable()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<string?>? _nullableStrings;

                [ReactiveCollection]
                private ObservableCollection<int?>? _nullableInts;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCollection with byte array element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ByteArray()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<byte[]>? _binaryData;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }
}
