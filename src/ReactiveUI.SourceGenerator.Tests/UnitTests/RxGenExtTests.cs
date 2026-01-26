// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Extended unit tests for the Reactive generator covering edge cases.
/// </summary>
[TestFixture]
public class RxGenExtTests : TestBase<ReactiveGenerator>
{
    /// <summary>
    /// Tests reactive property with generic type parameter.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Generic()
    {
        const string sourceCode = """
            using System;
            using System.Collections.Generic;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private List<string>? _items;

                [Reactive]
                private Dictionary<int, string>? _mappings;

                [Reactive]
                private IEnumerable<int>? _numbers;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property in a record class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Record()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial record TestVMRecord : ReactiveObject
            {
                [Reactive]
                private string? _name;

                [Reactive]
                private int _age;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with nullable value types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Nullable()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
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
    /// Tests reactive property with override inheritance modifier.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Override()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class BaseVM : ReactiveObject
            {
                [Reactive(Inheritance = InheritanceModifier.Virtual)]
                private string? _baseName;
            }

            public partial class DerivedVM : BaseVM
            {
                [Reactive(Inheritance = InheritanceModifier.Override)]
                private string? _baseName;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with new inheritance modifier.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task NewMod()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class BaseVM : ReactiveObject
            {
                [Reactive]
                private string? _shadowedProp;
            }

            public partial class DerivedVM : BaseVM
            {
                [Reactive(Inheritance = InheritanceModifier.New)]
                private string? _shadowedProp;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests multiple reactive properties with different access modifiers.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task MixedAccess()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive(SetModifier = AccessModifier.Public)]
                private string? _publicSet;

                [Reactive(SetModifier = AccessModifier.Protected)]
                private string? _protectedSet;

                [Reactive(SetModifier = AccessModifier.Internal)]
                private string? _internalSet;

                [Reactive(SetModifier = AccessModifier.Private)]
                private string? _privateSet;

                [Reactive(SetModifier = AccessModifier.InternalProtected)]
                private string? _internalProtectedSet;

                [Reactive(SetModifier = AccessModifier.PrivateProtected)]
                private string? _privateProtectedSet;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with array types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Arrays()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private byte[]? _bytes;

                [Reactive]
                private string[]? _strings;

                [Reactive]
                private int[][]? _jaggedArray;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with tuple types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Tuples()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private (int Id, string Name)? _namedTuple;

                [Reactive]
                private Tuple<int, string>? _tuple;

                [Reactive]
                private ValueTuple<string, int, bool> _valueTuple;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with custom struct type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Struct()
    {
        const string sourceCode = """
            using System;
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
                [Reactive]
                private Point _location;

                [Reactive]
                private Point? _optionalLocation;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with multiple AlsoNotify properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task MultiAlso()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive(nameof(FullName), nameof(DisplayName))]
                private string? _firstName;

                [Reactive(nameof(FullName), nameof(DisplayName))]
                private string? _lastName;

                public string FullName => $"{FirstName} {LastName}";
                public string DisplayName => $"{LastName}, {FirstName}";
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property in deeply nested classes.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task DeepNested()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class Level1 : ReactiveObject
            {
                [Reactive]
                private int _level1Prop;

                public partial class Level2 : ReactiveObject
                {
                    [Reactive]
                    private int _level2Prop;

                    public partial class Level3 : ReactiveObject
                    {
                        [Reactive]
                        private int _level3Prop;

                        public partial class Level4 : ReactiveObject
                        {
                            [Reactive]
                            private int _level4Prop;

                            public partial class Level5 : ReactiveObject
                            {
                                [Reactive]
                                private int _level5Prop;
                            }
                        }
                    }
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with enum types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Enums()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public enum Status { Pending, Active, Completed }

            [Flags]
            public enum Permissions { None = 0, Read = 1, Write = 2, Execute = 4 }

            public partial class TestVM : ReactiveObject
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
    /// Tests reactive property with interface types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Interfaces()
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
    /// Tests reactive property with delegate types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Delegates()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private Action? _action;

                [Reactive]
                private Func<int, string>? _func;

                [Reactive]
                private EventHandler? _handler;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with complex generic constraints.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ComplexGen()
    {
        const string sourceCode = """
            using System;
            using System.Collections.Generic;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private Dictionary<string, List<int>>? _nestedGenerics;

                [Reactive]
                private IReadOnlyDictionary<string, IReadOnlyList<string>>? _readOnlyNestedGenerics;

                [Reactive]
                private Func<Dictionary<int, List<string>>, bool>? _complexFunc;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property in generic class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task GenClass()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class GenericVM<T> : ReactiveObject where T : class
            {
                [Reactive]
                private T? _item;

                [Reactive]
                private int _count;
            }

            public partial class GenericVM<TKey, TValue> : ReactiveObject
                where TKey : notnull
                where TValue : class
            {
                [Reactive]
                private TKey? _key;

                [Reactive]
                private TValue? _value;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with multiple attributes on same field.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task MultiAttr()
    {
        const string sourceCode = """
            using System;
            using System.ComponentModel;
            using System.Runtime.Serialization;
            using System.Text.Json.Serialization;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [property: JsonInclude]
                [property: JsonPropertyName("user_name")]
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
    /// Tests reactive properties in multiple partial class definitions.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task MultiPartial()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private string? _firstName;
            }

            public partial class TestVM
            {
                [Reactive]
                private string? _lastName;
            }

            public partial class TestVM
            {
                [Reactive]
                private int _age;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with readonly struct types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ReadOnly()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public readonly struct ImmutablePoint
            {
                public int X { get; }
                public int Y { get; }
                public ImmutablePoint(int x, int y) { X = x; Y = y; }
            }

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private ImmutablePoint _point;

                [Reactive]
                private ImmutablePoint? _optionalPoint;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with Lazy types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Lazy()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private Lazy<string>? _lazyString;

                [Reactive]
                private Lazy<int>? _lazyInt;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests reactive property with TimeSpan and DateTimeOffset.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Time()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private TimeSpan _duration;

                [Reactive]
                private TimeSpan? _optionalDuration;

                [Reactive]
                private DateTimeOffset _timestamp;

                [Reactive]
                private DateTimeOffset? _optionalTimestamp;

                [Reactive]
                private TimeOnly _timeOnly;

                [Reactive]
                private DateOnly _dateOnly;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }
}
