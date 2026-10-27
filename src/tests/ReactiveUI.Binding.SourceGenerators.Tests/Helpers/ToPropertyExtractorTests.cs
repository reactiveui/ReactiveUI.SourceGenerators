// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Unit tests for <see cref="ToPropertyExtractor"/>.</summary>
public class ToPropertyExtractorTests
{
    /// <summary>The property every selector in these tests names.</summary>
    private const string PropertyName = "Name";

    /// <summary>The view model in <see cref="TargetSource"/> the generator can raise.</summary>
    private const string RaisableTypeName = "RaisableViewModel";

    /// <summary>The class in <see cref="TargetSource"/> declaring the generic methods.</summary>
    private const string StubsTypeName = "Stubs";

    /// <summary>The stub-shaped method in <see cref="TargetSource"/>, generic over a source and a value type.</summary>
    private const string StubMethodName = "Shaped";

    /// <summary>The <c>ToPropertyDispatch.g.cs</c> name the generator tests look for.</summary>
    private const string ToPropertyDispatchName = "ToPropertyDispatch.g.cs";

    /// <summary>A view model the generator can raise, and a stub-shaped generic method to close over test types.</summary>
    private const string TargetSource = """
                                        #nullable enable
                                        using System.ComponentModel;

                                        namespace TestApp
                                        {
                                            public partial class RaisableViewModel : INotifyPropertyChanged
                                            {
                                                public event PropertyChangedEventHandler? PropertyChanged;
                                            }

                                            public static class StaticHolder
                                            {
                                            }

                                            public static class Stubs
                                            {
                                                public static void Shaped<TObj, TRet>()
                                                {
                                                }

                                                public static void Open<T>()
                                                {
                                                }
                                            }
                                        }
                                        """;

    /// <summary>A parenthesized lambda with one parameter names its property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadSelectorName_ParenthesizedSingleParameter_ReturnsName() =>
        await Assert.That(ToPropertyExtractor.ReadSelectorName(SyntaxFactory.ParseExpression("(x) => x.Name")))
            .IsEqualTo(PropertyName);

    /// <summary>A lambda with more than one parameter names no property of the source.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadSelectorName_TwoParameters_ReturnsNull() =>
        await Assert.That(ToPropertyExtractor.ReadSelectorName(SyntaxFactory.ParseExpression("(x, y) => x.Name")))
            .IsNull();

    /// <summary>An argument that is not a lambda is not a selector.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadSelectorName_NotALambda_ReturnsNull() =>
        await Assert.That(ToPropertyExtractor.ReadSelectorName(SyntaxFactory.ParseExpression("\"Name\""))).IsNull();

    /// <summary>A lambda with a block body is not a selector.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadSelectorName_BlockBody_ReturnsNull() =>
        await Assert.That(ToPropertyExtractor.ReadSelectorName(SyntaxFactory.ParseExpression("x => { return x.Name; }")))
            .IsNull();

    /// <summary>Parentheses and the null-forgiving operator around the member are read through.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadSelectorName_ParenthesizedAndNullForgivingBody_ReturnsName()
    {
        using (Assert.Multiple())
        {
            await Assert.That(ToPropertyExtractor.ReadSelectorName(SyntaxFactory.ParseExpression("x => (x.Name)")))
                .IsEqualTo(PropertyName);
            await Assert.That(ToPropertyExtractor.ReadSelectorName(SyntaxFactory.ParseExpression("x => x.Name!")))
                .IsEqualTo(PropertyName);
        }
    }

    /// <summary>A member read off anything but the lambda's parameter names no property of the source.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadSelectorName_OtherReceiver_ReturnsNull()
    {
        using (Assert.Multiple())
        {
            await Assert.That(ToPropertyExtractor.ReadSelectorName(SyntaxFactory.ParseExpression("x => y.Name"))).IsNull();
            await Assert.That(ToPropertyExtractor.ReadSelectorName(SyntaxFactory.ParseExpression("x => x.Child.Name"))).IsNull();
            await Assert.That(ToPropertyExtractor.ReadSelectorName(SyntaxFactory.ParseExpression("x => x"))).IsNull();
        }
    }

    /// <summary>A string literal is read from its token.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadConstantName_StringLiteral_ReturnsValue()
    {
        var (argument, model) = Argument("\"Name\"");

        await Assert.That(ToPropertyExtractor.ReadConstantName(argument, model, default)).IsEqualTo(PropertyName);
    }

    /// <summary>A constant that is not a string names no property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadConstantName_NonStringConstant_ReturnsNull()
    {
        var (argument, model) = Argument("42");

        await Assert.That(ToPropertyExtractor.ReadConstantName(argument, model, default)).IsNull();
    }

    /// <summary>An expression with no constant value names no property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadConstantName_NotAConstant_ReturnsNull()
    {
        var (argument, model) = Argument("Probe.Holder.Compute()");

        await Assert.That(ToPropertyExtractor.ReadConstantName(argument, model, default)).IsNull();
    }

    /// <summary>A source type that is not a named type leaves nothing to generate.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadTarget_ArraySource_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(TargetSource);
        var source = compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_Int32));

        await Assert.That(ReadTarget(compilation, source, compilation.GetSpecialType(SpecialType.System_String))).IsNull();
    }

    /// <summary>A static source type leaves nothing to generate.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadTarget_StaticSource_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(TargetSource);

        await Assert.That(ReadTarget(compilation, Type(compilation, "StaticHolder"), compilation.GetSpecialType(SpecialType.System_String)))
            .IsNull();
    }

    /// <summary>A value type that is a type parameter leaves nothing to generate.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadTarget_TypeParameterValue_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(TargetSource);
        var typeParameter = Type(compilation, StubsTypeName).GetMembers("Open").OfType<IMethodSymbol>().Single().TypeParameters[0];

        await Assert.That(ReadTarget(compilation, Type(compilation, RaisableTypeName), typeParameter)).IsNull();
    }

    /// <summary>A value type that did not resolve leaves nothing to generate.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadTarget_ErrorValue_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(TargetSource);
        var missing = compilation.CreateErrorTypeSymbol(null, "Missing", 0);

        await Assert.That(ReadTarget(compilation, Type(compilation, RaisableTypeName), missing)).IsNull();
    }

    /// <summary>A nullable reference value type keeps its annotation in the display name only.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadTarget_NullableReferenceValue_AnnotatesTheDisplayName()
    {
        var compilation = TestHelper.CreateCompilation(TargetSource);
        var method = StubMethod(compilation).Construct(
            [Type(compilation, RaisableTypeName), compilation.GetSpecialType(SpecialType.System_String)],
            [NullableAnnotation.NotAnnotated, NullableAnnotation.Annotated]);

        var target = ToPropertyExtractor.ReadTarget(method, compilation);

        await Assert.That(target).IsNotNull();
        using (Assert.Multiple())
        {
            await Assert.That(target!.Value.ValueTypeFullName).IsEqualTo("string");
            await Assert.That(target.Value.ValueTypeDisplay).IsEqualTo("string?");
        }
    }

    /// <summary>A call with fewer than two arguments is not a ToProperty call the generator can read.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractToPropertyInvocation_SingleArgument_GeneratesNoDispatch()
    {
        const string source = """
                              using System;

                              namespace TestApp
                              {
                                  public static class CustomExtensions
                                  {
                                      public static object ToProperty<T>(this IObservable<T> target) => target;
                                  }

                                  public static class Scenario
                                  {
                                      public static object Execute(IObservable<string> names) => names.ToProperty();
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.DoesNotHaveGeneratedSource(ToPropertyDispatchName);
    }

    /// <summary>A property passed by name, after another named argument, is still found.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractToPropertyInvocation_NamedPropertyArgument_GeneratesDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class MyViewModel : INotifyPropertyChanged
                                  {
                                      private readonly ObservableAsPropertyHelper<string> _name;

                                      public MyViewModel(IObservable<string> names) =>
                                          _name = names.ToProperty(source: this, property: x => x.Name);

                                      public event PropertyChangedEventHandler PropertyChanged;

                                      public string Name => _name.Value;
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasGeneratedSource(ToPropertyDispatchName);
    }

    /// <summary>Compiles an argument inside a call and returns it with its semantic model.</summary>
    /// <param name="argument">The argument text.</param>
    /// <returns>The parsed argument and the model that bound it.</returns>
    private static (ExpressionSyntax Argument, SemanticModel Model) Argument(string argument)
    {
        var compilation = TestHelper.CreateCompilation($$"""
                                                         namespace Probe
                                                         {
                                                             public static class Holder
                                                             {
                                                                 public static string Compute() => "Name";

                                                                 public static void Take(object value)
                                                                 {
                                                                 }

                                                                 public static void Use() => Take({{argument}});
                                                             }
                                                         }
                                                         """);
        var tree = compilation.SyntaxTrees.First();
        var expression = tree.GetRoot().DescendantNodes().OfType<ArgumentSyntax>().Last().Expression;
        return (expression, compilation.GetSemanticModel(tree));
    }

    /// <summary>Closes the stub-shaped method over a source and value type and reads its target.</summary>
    /// <param name="compilation">The compilation declaring the stub-shaped method.</param>
    /// <param name="source">The source type argument.</param>
    /// <param name="value">The value type argument.</param>
    /// <returns>The target, or null when nothing can be generated.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ToPropertyExtractor.Target? ReadTarget(Compilation compilation, ITypeSymbol source, ITypeSymbol value) =>
        ToPropertyExtractor.ReadTarget(StubMethod(compilation).Construct(source, value), compilation);

    /// <summary>Gets the stub-shaped generic method.</summary>
    /// <param name="compilation">The compilation declaring it.</param>
    /// <returns>The open method.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IMethodSymbol StubMethod(Compilation compilation) =>
        Type(compilation, StubsTypeName).GetMembers(StubMethodName).OfType<IMethodSymbol>().Single();

    /// <summary>Gets a type declared in the test namespace.</summary>
    /// <param name="compilation">The compilation declaring it.</param>
    /// <param name="name">The type's simple name.</param>
    /// <returns>The type.</returns>
    /// <exception cref="InvalidOperationException">The compilation declares no such type.</exception>
    private static INamedTypeSymbol Type(Compilation compilation, string name) =>
        compilation.GetTypeByMetadataName($"TestApp.{name}")
        ?? throw new InvalidOperationException($"'{name}' was not declared.");
}
