// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Extended unit tests for the ReactiveObject generator covering edge cases.
/// </summary>
[TestFixture]
public class RxObjExtTests : TestBase<ReactiveObjectGenerator>
{
    /// <summary>
    /// Tests ReactiveObject with multiple reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveObjectWithMultipleProperties()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class TestVM
            {
                [Reactive]
                private string? _firstName;

                [Reactive]
                private string? _lastName;

                [Reactive]
                private int _age;

                [Reactive]
                private bool _isActive;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with nested class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveObjectWithNestedClass()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class ParentVM
            {
                [Reactive]
                private string? _parentProperty;

                [IReactiveObject]
                public partial class ChildVM
                {
                    [Reactive]
                    private string? _childProperty;
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with ObservableAsProperty.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task WithOap()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class TestVM
            {
                [Reactive]
                private string? _input;

                [ObservableAsProperty]
                private string? _output;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with generic type parameters.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveObjectWithGenericClass()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class GenericVM<T> where T : class
            {
                [Reactive]
                private T? _item;

                [Reactive]
                private int _count;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with multiple type constraints.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveObjectWithMultipleTypeConstraints()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public interface IEntity
            {
                int Id { get; }
            }

            [IReactiveObject]
            public partial class EntityVM<TEntity> where TEntity : class, IEntity, new()
            {
                [Reactive]
                private TEntity? _entity;

                [Reactive]
                private bool _isModified;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with record.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveObjectRecord()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial record TestVMRecord
            {
                [Reactive]
                private string? _name;

                [Reactive]
                private int _value;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with access modifiers on reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveObjectWithAccessModifiers()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class TestVM
            {
                [Reactive(SetModifier = AccessModifier.Protected)]
                private string? _protectedSet;

                [Reactive(SetModifier = AccessModifier.Internal)]
                private string? _internalSet;

                [Reactive(SetModifier = AccessModifier.Private)]
                private string? _privateSet;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with inheritance modifiers.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveObjectWithInheritanceModifiers()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class BaseVM
            {
                [Reactive(Inheritance = InheritanceModifier.Virtual)]
                private string? _virtualProperty;
            }

            [IReactiveObject]
            public partial class DerivedVM : BaseVM
            {
                [Reactive(Inheritance = InheritanceModifier.Override)]
                private string? _virtualProperty;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with complex types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ComplexType()
    {
        const string sourceCode = """
            using System;
            using System.Collections.Generic;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class TestVM
            {
                [Reactive]
                private List<string>? _items;

                [Reactive]
                private Dictionary<int, string>? _mappings;

                [Reactive]
                private (int Id, string Name)? _tuple;

                [Reactive]
                private int[]? _numbers;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with nullable value types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Nullable()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class TestVM
            {
                [Reactive]
                private int? _nullableInt;

                [Reactive]
                private DateTime? _nullableDateTime;

                [Reactive]
                private Guid? _nullableGuid;

                [Reactive]
                private decimal? _nullableDecimal;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with attributes on properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveObjectWithAttributes()
    {
        const string sourceCode = """
            using System;
            using System.Runtime.Serialization;
            using System.Text.Json.Serialization;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class TestVM
            {
                [property: JsonPropertyName("user_name")]
                [property: JsonInclude]
                [DataMember(Name = "userName")]
                [Reactive]
                private string? _userName;

                [property: JsonIgnore]
                [IgnoreDataMember]
                [Reactive]
                private string? _password;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with deeply nested classes.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveObjectWithDeeplyNestedClasses()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class Level1
            {
                [Reactive]
                private int _level1Prop;

                [IReactiveObject]
                public partial class Level2
                {
                    [Reactive]
                    private int _level2Prop;

                    [IReactiveObject]
                    public partial class Level3
                    {
                        [Reactive]
                        private int _level3Prop;

                        [IReactiveObject]
                        public partial class Level4
                        {
                            [Reactive]
                            private int _level4Prop;
                        }
                    }
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with enum types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Enums()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public enum Status { Pending, Active, Completed }

            [Flags]
            public enum Permissions { None = 0, Read = 1, Write = 2, Execute = 4 }

            [IReactiveObject]
            public partial class TestVM
            {
                [Reactive]
                private Status _status;

                [Reactive]
                private Status? _nullableStatus;

                [Reactive]
                private Permissions _permissions;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with interface types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Interface()
    {
        const string sourceCode = """
            using System;
            using System.Collections.Generic;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public interface IItem
            {
                string Name { get; }
            }

            [IReactiveObject]
            public partial class TestVM
            {
                [Reactive]
                private IItem? _item;

                [Reactive]
                private IDisposable? _disposable;

                [Reactive]
                private IList<int>? _list;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject with basic properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Basic()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IReactiveObject]
            public partial class TestVM
            {
                [Reactive]
                private string? _firstName;

                [Reactive]
                private string? _lastName;

                public string FullName => $"{FirstName} {LastName}";
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveObject in multiple namespaces.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task MultiNs()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI.SourceGenerators;

            namespace Namespace1
            {
                [IReactiveObject]
                public partial class TestVM
                {
                    [Reactive]
                    private string? _name;
                }
            }

            namespace Namespace2
            {
                [IReactiveObject]
                public partial class TestVM
                {
                    [Reactive]
                    private string? _name;
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }
}
