// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Generators;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers the declarations written for the members ReactiveUI.SourceGenerators adds: each name, type and accessibility
/// has to match what that generator writes, or a call site binding the member is read against the wrong shape.
/// </summary>
public class SourceGeneratorsDeclarationTests
{
    /// <summary>What the probe writes when the compilation uses none of the attributes.</summary>
    private const string None = "// none";

    /// <summary>The header of the second type the grouping test declares.</summary>
    private const string SecondHeader = "partial class Second";

    /// <summary>A ReactiveUI built on ReactiveUI.Primitives, whose commands carry <c>RxVoid</c>.</summary>
    /// <remarks>The field and the generic overload are other members named <c>Create</c>, which the lookup steps over.</remarks>
    private const string PrimitivesReactiveUI = """
        namespace ReactiveUI.Primitives { public struct RxVoid { } }
        namespace ReactiveUI
        {
            public class ReactiveCommand<TInput, TOutput> { }
            public static class ReactiveCommand
            {
                public static int Create;
                public static T[] Create<T>() => null;
                public static ReactiveCommand<ReactiveUI.Primitives.RxVoid, ReactiveUI.Primitives.RxVoid> Create(System.Action execute) => null;
            }
        }
        """;

    /// <summary>A ReactiveUI whose commands carry <c>Unit</c>.</summary>
    private const string UnitReactiveUI = """
        namespace System.Reactive { public struct Unit { } }
        namespace ReactiveUI
        {
            public class ReactiveCommand<TInput, TOutput> { }
            public static class ReactiveCommand
            {
                public static ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> Create(System.Action execute) => null;
            }
        }
        """;

    /// <summary>ReactiveUI's System.Reactive flavour, which declares its commands in <c>ReactiveUI.Reactive</c>.</summary>
    private const string SystemReactiveUI = """
        namespace System.Reactive { public struct Unit { } }
        namespace ReactiveUI.Reactive
        {
            public class ReactiveCommand<TInput, TOutput> { }
            public static class ReactiveCommand { }
        }
        """;

    /// <summary>ReactiveUI.SourceGenerators' attributes, as its 4.0.0 library declares them.</summary>
    private static readonly MetadataReference Attributes =
        MetadataReference.CreateFromFile(typeof(ReactiveUI.SourceGenerators.ReactiveAttribute).Assembly.Location);

    /// <summary>
    /// The framework the attributes library was built for. Its enums are read through the framework it references, so the
    /// probe has to reference the same one, and no ReactiveUI: the stand-ins below play that part.
    /// </summary>
#if NET11_0_OR_GREATER
    private static readonly IEnumerable<MetadataReference> FrameworkReferences = Basic.Reference.Assemblies.Net110.References.All;
#else
    private static readonly IEnumerable<MetadataReference> FrameworkReferences = Basic.Reference.Assemblies.Net100.References.All;
#endif

    /// <summary>A compilation that uses none of the attributes gets no declarations, and is read as it is.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoAttributes_DeclaresNothing() =>
        await Assert.That(Declarations("public partial class Model { }")).IsEqualTo(None);

    /// <summary>A <c>[Reactive]</c> field declares a public property whose setter follows <c>SetModifier</c>.</summary>
    /// <param name="modifier">The <c>AccessModifier</c> member named on the attribute.</param>
    /// <param name="setter">The setter the property is declared with.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("", "set { }")]
    [Arguments("(SetModifier = AccessModifier.Protected)", "protected set { }")]
    [Arguments("(SetModifier = AccessModifier.Internal)", "internal set { }")]
    [Arguments("(SetModifier = AccessModifier.Private)", "private set { }")]
    [Arguments("(SetModifier = AccessModifier.InternalProtected)", "protected internal set { }")]
    [Arguments("(SetModifier = AccessModifier.PrivateProtected)", "private protected set { }")]
    [Arguments("(SetModifier = AccessModifier.Init)", "init { }")]
    public async Task ReactiveField_DeclaresPropertyWithSetter(string modifier, string setter) =>
        await Assert.That(Declarations($"public partial class Model {{ [Reactive{modifier}] private string _name; }}"))
            .Contains($"public string Name {{ get {{ throw null; }} {setter} }}");

    /// <summary>A field drops an <c>m_</c> prefix or any leading underscores, and its first letter becomes upper case.</summary>
    /// <param name="field">The field's name.</param>
    /// <param name="property">The property's name.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("m_count", "Count")]
    [Arguments("__count", "Count")]
    [Arguments("count", "Count")]
    public async Task ReactiveField_NamesPropertyFromField(string field, string property) =>
        await Assert.That(Declarations($"public partial class Model {{ [Reactive] private int {field}; }}"))
            .Contains($"public int {property} {{");

    /// <summary>A field whose property name would repeat its own name, or that has no name left, declares nothing.</summary>
    /// <param name="field">The field's name.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("Count")]
    [Arguments("_")]
    public async Task ReactiveField_WithoutADistinctName_DeclaresNothing(string field) =>
        await Assert.That(Declarations($"public partial class Model {{ [Reactive] private int {field}; }}")).IsEqualTo(None);

    /// <summary>A partial property is declared by the consumer already, so nothing is added for it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactivePartialProperty_DeclaresNothing() =>
        await Assert.That(Declarations("public partial class Model { [Reactive] public partial string Name { get; set; } }"))
            .IsEqualTo(None);

    /// <summary>An attribute placed on an event field, which it does not allow, declares nothing.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("Reactive")]
    [Arguments("ReactiveCollection")]
    public async Task AttributeOnEventField_DeclaresNothing(string attribute) =>
        await Assert.That(Declarations($"public partial class Model {{ [{attribute}] public event System.EventHandler _changed; }}"))
            .IsEqualTo(None);

    /// <summary>A type that is not partial cannot gain members, so nothing is declared for it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonPartialType_DeclaresNothing() =>
        await Assert.That(Declarations("public class Model { [Reactive] private int _count; [ReactiveCommand] private void Save() { } }"))
            .IsEqualTo(None);

    /// <summary>A member of a nested generic type is declared inside every enclosing partial type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NestedType_DeclaresInsideEachEnclosingType()
    {
        var declarations = Declarations(
            "namespace App { public partial class Outer<T> { public partial class Inner { [Reactive] private T _value; } } }");

        await Assert.That(declarations).Contains("namespace App");
        await Assert.That(declarations).Contains("partial class Outer<T>");
        await Assert.That(declarations).Contains("partial class Inner");
        await Assert.That(declarations).Contains("public T Value {");
    }

    /// <summary>A <c>[ReactiveCollection]</c> field declares a public read-write property; a partial property adds nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveCollection_DeclaresReadWriteProperty()
    {
        var declarations = Declarations(
            """
            public partial class Model
            {
                [ReactiveCollection]
                private System.Collections.ObjectModel.ObservableCollection<string> _tags;

                [ReactiveCollection]
                public partial System.Collections.ObjectModel.ObservableCollection<string> Items { get; set; }
            }
            """);

        await Assert.That(declarations)
            .Contains("public global::System.Collections.ObjectModel.ObservableCollection<string> Tags { get { throw null; } set { } }");
        await Assert.That(declarations).DoesNotContain("Items");
    }

    /// <summary>A <c>[BindableDerivedList]</c> field declares a read-only property whose accessibility follows <c>AccessModifier</c>.</summary>
    /// <param name="modifier">The <c>PropertyAccessModifier</c> member named on the attribute.</param>
    /// <param name="accessibility">The accessibility the property is declared with.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("", "public")]
    [Arguments("(AccessModifier = PropertyAccessModifier.Protected)", "protected")]
    [Arguments("(AccessModifier = PropertyAccessModifier.Internal)", "internal")]
    [Arguments("(AccessModifier = PropertyAccessModifier.Private)", "private")]
    [Arguments("(AccessModifier = PropertyAccessModifier.InternalProtected)", "protected internal")]
    [Arguments("(AccessModifier = PropertyAccessModifier.PrivateProtected)", "private protected")]
    public async Task BindableDerivedList_DeclaresReadOnlyProperty(string modifier, string accessibility) =>
        await Assert.That(Declarations(
                $"public partial class Model {{ [BindableDerivedList{modifier}] private System.Collections.ObjectModel.ReadOnlyObservableCollection<int> _items; }}"))
            .Contains($"{accessibility} global::System.Collections.ObjectModel.ReadOnlyObservableCollection<int> Items {{ get {{ throw null; }} }}");

    /// <summary>A command's input and output follow the method's parameters and return type.</summary>
    /// <param name="method">The command method.</param>
    /// <param name="property">The command property it declares.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("void Save() { }", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, global::ReactiveUI.Primitives.RxVoid> SaveCommand")]
    [Arguments("int Count(string text) => 0;", "ReactiveCommand<string, int> CountCommand")]
    [Arguments("Task RunAsync() => null;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, global::ReactiveUI.Primitives.RxVoid> RunCommand")]
    [Arguments("Task<int> LoadAsync(int v, CancellationToken t) => null;", "ReactiveCommand<int, int> LoadCommand")]
    [Arguments("Task<int> FetchAsync(CancellationToken t) => null;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, int> FetchCommand")]
    [Arguments("int Stop(CancellationToken t) => 0;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, int> StopCommand")]
    [Arguments("IObservable<string> Watch() => null;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, string> WatchCommand")]
    [Arguments("Task<IObservable<int>> Both() => null;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, global::System.IObservable<int>> BothCommand")]
    [Arguments("IObservable<int>[] Many() => null;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, global::ReactiveUI.Primitives.RxVoid> ManyCommand")]
    [Arguments("Holder<IObservable<int>>.Inner Held() => null;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, global::ReactiveUI.Primitives.RxVoid> HeldCommand")]
    [Arguments("Stream Derived() => null;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, global::System.IO.Stream> DerivedCommand")]
    [Arguments("int[] Plain() => null;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, int[]> PlainCommand")]
    [Arguments("T Pick<T>() => default;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, T> PickCommand")]
    [Arguments("void m_Reset() { }", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, global::ReactiveUI.Primitives.RxVoid> ResetCommand")]
    [Arguments("void _clear() { }", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, global::ReactiveUI.Primitives.RxVoid> ClearCommand")]
    [Arguments("int SendAsync() => 0;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, int> SendAsyncCommand")]
    [Arguments("Task Async() => null;", "ReactiveCommand<global::ReactiveUI.Primitives.RxVoid, global::ReactiveUI.Primitives.RxVoid> Command")]
    public async Task ReactiveCommand_DeclaresTypedCommand(string method, string property) =>
        await Assert.That(Declarations(CommandModel(method), PrimitivesReactiveUI)).Contains($"public global::ReactiveUI.{property} {{ get {{ throw null; }} }}");

    /// <summary>A method with more than one parameter the command could carry gets no command.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveCommand_WithTwoParameters_DeclaresNothing() =>
        await Assert.That(Declarations(CommandModel("void Move(int x, int y) { }"), PrimitivesReactiveUI)).IsEqualTo(None);

    /// <summary>A command's accessibility follows <c>AccessModifier</c>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveCommand_FollowsAccessModifier() =>
        await Assert.That(Declarations(
                "public partial class Model { [ReactiveCommand(AccessModifier = PropertyAccessModifier.Internal)] private void Save() { } }",
                PrimitivesReactiveUI))
            .Contains("internal global::ReactiveUI.ReactiveCommand<");

    /// <summary>The command type and the type a command without a value carries follow the ReactiveUI in the compilation.</summary>
    /// <param name="reactiveUI">Stand-in declarations of the ReactiveUI the consumer references, or empty for none.</param>
    /// <param name="command">The command type the method declares.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(SystemReactiveUI, "global::ReactiveUI.Reactive.ReactiveCommand<global::System.Reactive.Unit, global::System.Reactive.Unit>")]
    [Arguments(UnitReactiveUI, "global::ReactiveUI.ReactiveCommand<global::System.Reactive.Unit, global::System.Reactive.Unit>")]
    [Arguments("", "global::ReactiveUI.ReactiveCommand<global::System.Reactive.Unit, global::System.Reactive.Unit>")]
    public async Task ReactiveCommand_FollowsReferencedReactiveUI(string reactiveUI, string command) =>
        await Assert.That(Declarations("public partial class Model { [ReactiveCommand] private void Save() { } }", reactiveUI))
            .Contains($"public {command} SaveCommand");

    /// <summary>An <c>[IReactiveObject]</c> class implements <c>IReactiveObject</c>, alongside the members it gains.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IReactiveObject_AddsInterface()
    {
        var declarations = Declarations(
            "[IReactiveObject] public partial class Model { [Reactive] private string _title; }",
            "namespace ReactiveUI { public interface IReactiveObject { } }");

        await Assert.That(declarations).Contains("partial class Model : global::ReactiveUI.IReactiveObject");
        await Assert.That(declarations).Contains("public string Title {");
    }

    /// <summary>An <c>[IReactiveObject]</c> class gains nothing when there is no <c>IReactiveObject</c> or it is not partial.</summary>
    /// <param name="source">The class.</param>
    /// <param name="reactiveUI">Stand-in declarations of the ReactiveUI the consumer references, or empty for none.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("[IReactiveObject] public partial class Model { }", "")]
    [Arguments("[IReactiveObject] public class Model { }", "namespace ReactiveUI { public interface IReactiveObject { } }")]
    public async Task IReactiveObject_WithoutInterfaceOrPartial_DeclaresNothing(string source, string reactiveUI) =>
        await Assert.That(Declarations(source, reactiveUI)).IsEqualTo(None);

    /// <summary>Members are grouped by the type they belong to, in the order the types are first seen.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WriteDeclarations_GroupsMembersByType()
    {
        var first = new PartialTypeDeclaration(null, new(["partial class First"]));
        var second = new PartialTypeDeclaration("App", new([SecondHeader]));

        var text = SourceGeneratorsCompilation.WriteDeclarations(
            [new SourceGeneratorsMember(first, null, "public int A { get { throw null; } }"), null],
            [new SourceGeneratorsMember(second, null, "public int B { get { throw null; } }"), new SourceGeneratorsMember(first, null, "public int C { get { throw null; } }")]);

        await Assert.That(text.IndexOf("partial class First", StringComparison.Ordinal))
            .IsLessThan(text.IndexOf(SecondHeader, StringComparison.Ordinal));
        await Assert.That(text.IndexOf("public int C", StringComparison.Ordinal))
            .IsLessThan(text.IndexOf(SecondHeader, StringComparison.Ordinal));
    }

    /// <summary>The naming rules for fields and command methods.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NamingRules_MatchSourceGenerators()
    {
        await Assert.That(SourceGeneratorsMemberExtractor.PropertyName("m_value")).IsEqualTo("Value");
        await Assert.That(SourceGeneratorsMemberExtractor.CommandName("SaveAsync", true)).IsEqualTo("SaveCommand");
        await Assert.That(SourceGeneratorsMemberExtractor.CommandName("SaveAsync", false)).IsEqualTo("SaveAsyncCommand");
    }

    /// <summary>Places a command method in a partial view model, beside the types the return types name.</summary>
    /// <param name="method">The command method, without its attribute or accessibility.</param>
    /// <returns>The model source.</returns>
    private static string CommandModel(string method) =>
        $$"""
        using System;
        using System.IO;
        using System.Threading;
        using System.Threading.Tasks;

        public class Holder<T> { public class Inner { } }

        public partial class Model
        {
            [ReactiveCommand]
            private {{method}}
        }
        """;

    /// <summary>Runs the declarations pipeline over a model, and returns the declarations it wrote.</summary>
    /// <param name="source">The model, which may use ReactiveUI.SourceGenerators' attributes unqualified.</param>
    /// <param name="reactiveUI">Stand-in declarations of the ReactiveUI the consumer references.</param>
    /// <returns>The declarations, or <see cref="None"/> when the pipeline wrote none.</returns>
    private static string Declarations(string source, string reactiveUI = "")
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp13);
        var compilation = CSharpCompilation.Create(
            "Probe",
            [
                CSharpSyntaxTree.ParseText($"using ReactiveUI.SourceGenerators;\n{source}", parseOptions),
                CSharpSyntaxTree.ParseText(reactiveUI, parseOptions),
            ],
            [.. FrameworkReferences, Attributes],
            new(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create([new DeclarationsProbe().AsSourceGenerator()], parseOptions: parseOptions)
            .RunGenerators(compilation);

        return driver.GetRunResult().Results[0].GeneratedSources[0].SourceText.ToString();
    }

    /// <summary>Writes the declarations the pipeline adds to the compilation, as a generated file the test can read.</summary>
    private sealed class DeclarationsProbe : IIncrementalGenerator
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            context.RegisterSourceOutput(
                SourceGeneratorsCompilation.Register(in context),
                static (output, compilation) => output.AddSource(
                    "Declarations.g.cs",
                    compilation is null ? None : compilation.SyntaxTrees.Last().ToString()));
    }
}
