// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Extended unit tests for the BindableDerivedList generator covering edge cases.
/// </summary>
[TestFixture]
public class DerivedListExtTests : TestBase<BindableDerivedListGenerator>
{
    /// <summary>
    /// Tests BindableDerivedList with multiple lists.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task MultipleLists()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public partial class TestVM
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<string>? _names;

                [BindableDerivedList]
                private ReadOnlyObservableCollection<int>? _numbers;

                [BindableDerivedList]
                private ReadOnlyObservableCollection<double>? _values;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList with complex generic type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ComplexGeneric()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public class ItemModel
            {
                public int Id { get; set; }
                public string? Name { get; set; }
            }

            public partial class TestVM
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<ItemModel>? _items;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList in nested class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task NestedClass()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public partial class OuterVM
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<int>? _outerList;

                public partial class InnerVM
                {
                    [BindableDerivedList]
                    private ReadOnlyObservableCollection<string>? _innerList;

                    public partial class DeepInnerVM
                    {
                        [BindableDerivedList]
                        private ReadOnlyObservableCollection<double>? _deepList;
                    }
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList with generic class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task GenericClass()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public partial class GenericVM<T> where T : class
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<T>? _items;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList with record type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task RecordClass()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public partial record TestVMRecord
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<int>? _numbers;

                [BindableDerivedList]
                private ReadOnlyObservableCollection<string>? _names;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList with nullable reference type elements.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task NullableElements()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public partial class TestVM
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<string?>? _nullableStrings;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList with interface element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task InterfaceType()
    {
        const string sourceCode = """
            using System;
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public interface IItem
            {
                int Id { get; }
                string Name { get; }
            }

            public partial class TestVM
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<IItem>? _items;

                [BindableDerivedList]
                private ReadOnlyObservableCollection<IDisposable>? _disposables;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList with tuple type elements.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task TupleType()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public partial class TestVM
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<(int Id, string Name)>? _tuples;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList in different namespaces.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task DiffNamespaces()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace Namespace1
            {
                public partial class TestVM
                {
                    [BindableDerivedList]
                    private ReadOnlyObservableCollection<int>? _numbers;
                }
            }

            namespace Namespace2
            {
                public partial class TestVM
                {
                    [BindableDerivedList]
                    private ReadOnlyObservableCollection<string>? _names;
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList with enum element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task EnumType()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public enum Status { Pending, Active, Completed }

            public partial class TestVM
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<Status>? _statuses;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList with struct element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task StructType()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public struct Point
            {
                public int X { get; set; }
                public int Y { get; set; }
            }

            public partial class TestVM
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<Point>? _points;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList with Guid element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task GuidType()
    {
        const string sourceCode = """
            using System;
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public partial class TestVM
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<Guid>? _ids;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList with DateTime element type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task DateTimeType()
    {
        const string sourceCode = """
            using System;
            using System.Collections.ObjectModel;
            using DynamicData;

            namespace TestNs;

            public partial class TestVM
            {
                [BindableDerivedList]
                private ReadOnlyObservableCollection<DateTime>? _dates;

                [BindableDerivedList]
                private ReadOnlyObservableCollection<DateTimeOffset>? _timestamps;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests BindableDerivedList combined with reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithReactive()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;
            using DynamicData;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private string? _searchText;

                [Reactive]
                private bool _isLoading;

                [BindableDerivedList]
                private ReadOnlyObservableCollection<string>? _filteredItems;

                [BindableDerivedList]
                private ReadOnlyObservableCollection<int>? _sortedNumbers;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }
}
