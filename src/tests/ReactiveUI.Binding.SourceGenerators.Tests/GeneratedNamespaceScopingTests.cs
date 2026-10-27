// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers which namespace the generated dispatch overloads are emitted into, and that a call site reaches them
/// from there. Extension-method lookup walks the enclosing namespaces of the call site from the inside out and
/// stops at the first that offers any candidate, so the namespace is what decides whether a call gets the
/// compile-time overload or falls through to the runtime stub.
/// </summary>
public class GeneratedNamespaceScopingTests
{
    /// <summary>The hint name of the file carrying the namespace declaration and its import.</summary>
    private const string AttributesFileName = "GeneratedBindingsAttributes.g.cs";

    /// <summary>The token in <see cref="SourceTemplate"/> standing in for the consumer's own namespace.</summary>
    private const string NamespaceToken = "__CONSUMER_NAMESPACE__";

    /// <summary>The namespace declaration a consumer sharing the runtime library's namespace gets.</summary>
    private const string SharedNamespaceDeclaration = "namespace ReactiveUI.Binding\n";

    /// <summary>The namespace a consumer with no namespace of its own gets.</summary>
    private const string AssemblyNamespace = "ReactiveUI.Binding.Generated.TestAssembly";

    /// <summary>The namespace declaration a consumer with no namespace of its own gets.</summary>
    private const string AssemblyNamespaceDeclaration = $"namespace {AssemblyNamespace}\n";

    /// <summary>The generated class name wherever another assembly could also emit into the same namespace.</summary>
    private const string AssemblyQualifiedClassName = "__ReactiveUIGeneratedBindings_TestAssembly";

    /// <summary>An assembly-level grant of internals to another assembly.</summary>
    private const string InternalsVisibleTo =
        "[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"Contoso.App.Tests\")]\n";

    /// <summary>The last import in <see cref="SourceTemplate"/>, after which assembly attributes may appear.</summary>
    private const string LastImport = "using ReactiveUI.Binding;\n";

    /// <summary>A consumer root namespace with no relationship to this library's own.</summary>
    private const string UnrelatedRootNamespace = "Contoso.App";

    /// <summary>A minimal consumer: one observable type and one call site, in a namespace of our choosing.</summary>
    private const string SourceTemplate = """
                                          using System;
                                          using System.ComponentModel;
                                          using ReactiveUI.Binding;

                                          namespace __CONSUMER_NAMESPACE__
                                          {
                                              public class MyViewModel : INotifyPropertyChanged
                                              {
                                                  public event PropertyChangedEventHandler PropertyChanged;

                                                  public string Name { get; set; }
                                              }

                                              public static class Usage
                                              {
                                                  public static IObservable<string> Observe(MyViewModel viewModel)
                                                  {
                                                      return viewModel.WhenChanged(x => x.Name);
                                                  }
                                              }
                                          }
                                          """;

    /// <summary>
    /// The dispatch overloads land in the consumer's own root namespace, which is the one namespace their code
    /// is reliably nested under.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RootNamespace_PutsTheDispatchOverloadsInTheConsumersNamespace()
    {
        var result = TestHelper.RunGenerator(SourceIn(UnrelatedRootNamespace), LanguageVersion.CSharp10, UnrelatedRootNamespace);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(AttributesFileName, $"namespace {UnrelatedRootNamespace}\n");
        await result.GeneratedSourceContains(AttributesFileName, $"global using global::{UnrelatedRootNamespace};");
    }

    /// <summary>
    /// A build that exposes no root namespace still gets a namespace of its own, derived from the assembly name,
    /// so two assemblies that both run the generator cannot see each other's overloads.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoRootNamespace_PutsTheDispatchOverloadsUnderTheAssemblyNamespace()
    {
        var result = TestHelper.RunGenerator(SourceIn(UnrelatedRootNamespace), LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(AttributesFileName, AssemblyNamespaceDeclaration);
    }

    /// <summary>
    /// A root namespace is a project property, not an identifier, so it can carry characters no namespace may.
    /// Each segment is rendered as a legal identifier, and the segments stay separate.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RootNamespaceWithCharactersIllegalInAnIdentifier_IsRenderedAsIdentifierSegments()
    {
        var result = TestHelper.RunGenerator(SourceIn(UnrelatedRootNamespace), LanguageVersion.CSharp10, "My-App.2Fast");

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(AttributesFileName, "namespace My_App._2Fast\n");
    }

    /// <summary>
    /// The case that makes the root namespace the right home: a consumer whose own code sits under
    /// <c>ReactiveUI.Binding</c> reaches the runtime stub at an enclosing namespace, and lookup stops there. Only
    /// an overload nested at least as deep as their own namespace is reached first.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallSiteBeneathTheRuntimeStubNamespace_BindsToTheGeneratedOverload()
    {
        var result = TestHelper.RunGenerator(
            SourceIn("ReactiveUI.Binding.Consumer.Views"),
            LanguageVersion.CSharp10,
            "ReactiveUI.Binding.Consumer");

        await result.CompilationSucceeds();
        await AssertCallSiteResolvesTo(result, "ReactiveUI.Binding.Consumer");
    }

    /// <summary>
    /// A file whose namespace sits outside the root namespace has no enclosing level offering the overload, and
    /// reaches it through the generated import instead.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallSiteOutsideTheRootNamespace_BindsToTheGeneratedOverload()
    {
        var result = TestHelper.RunGenerator(SourceIn("Fabrikam.Ui"), LanguageVersion.CSharp10, UnrelatedRootNamespace);

        await result.CompilationSucceeds();
        await AssertCallSiteResolvesTo(result, UnrelatedRootNamespace);
    }

    /// <summary>
    /// Before global usings there is no way to scope a namespace to one compilation, so the overloads stay in the
    /// shared namespace and no import is emitted, whatever the root namespace is. The call still reaches them,
    /// through the import of that namespace the consumer already has.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BeforeGlobalUsings_TheDispatchOverloadsStayInTheSharedNamespace()
    {
        var result = TestHelper.RunGenerator(SourceIn(UnrelatedRootNamespace), LanguageVersion.CSharp7_3, UnrelatedRootNamespace);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(AttributesFileName, SharedNamespaceDeclaration);
        await Assert.That(result.GeneratedSources[AttributesFileName]).DoesNotContain("global using");
        await AssertCallSiteResolvesTo(result, Constants.SharedGeneratedNamespace);
    }

    /// <summary>
    /// An assembly that exposes its internals can have its generated overloads seen by the assembly it exposes
    /// them to, and below C# 10 there is no global using to scope them with. Those overloads move to the
    /// consumer's own root namespace, which no other assembly's code sits under.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BeforeGlobalUsings_GrantingInternalsVisibleTo_MovesTheOverloadsOutOfTheSharedNamespace()
    {
        var source = SourceIn(UnrelatedRootNamespace)
            .Replace(LastImport, LastImport + InternalsVisibleTo, StringComparison.Ordinal);
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp7_3, UnrelatedRootNamespace);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(AttributesFileName, $"namespace {UnrelatedRootNamespace}\n");
        await AssertCallSiteResolvesTo(result, UnrelatedRootNamespace);
    }

    /// <summary>
    /// A build that exposes no root namespace leaves nowhere else the call sites could reach, so the shared
    /// namespace and its collision risk still beats emitting somewhere nothing can see.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BeforeGlobalUsings_GrantingInternalsVisibleToWithNoRootNamespace_KeepsTheSharedNamespace()
    {
        var source = SourceIn(UnrelatedRootNamespace)
            .Replace(LastImport, LastImport + InternalsVisibleTo, StringComparison.Ordinal);
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp7_3);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(AttributesFileName, SharedNamespaceDeclaration);
        await AssertCallSiteResolvesTo(result, Constants.SharedGeneratedNamespace);
    }

    /// <summary>
    /// Another assembly sharing this root namespace has already put its dispatch class where this one would go,
    /// and both would answer the same call. Seeing it there, the overloads move to the per-assembly namespace.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RootNamespaceAlreadyCarriesADispatchClass_FallsBackToTheAssemblyNamespace()
    {
        var source = SourceIn(UnrelatedRootNamespace) + $$"""


                                                          namespace {{UnrelatedRootNamespace}}
                                                          {
                                                              internal static class __ReactiveUIGeneratedBindings
                                                              {
                                                              }
                                                          }
                                                          """;
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, UnrelatedRootNamespace);

        await result.GeneratedSourceContains(AttributesFileName, AssemblyNamespaceDeclaration);
    }

    /// <summary>
    /// Without that exposure nothing can see the overloads, so they stay where every file reaches them without
    /// an import of its own.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BeforeGlobalUsings_WithoutInternalsVisibleTo_KeepsTheSharedNamespace()
    {
        var result = TestHelper.RunGenerator(
            SourceIn("Fabrikam.Ui"),
            LanguageVersion.CSharp7_3,
            UnrelatedRootNamespace);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(AttributesFileName, SharedNamespaceDeclaration);
        await AssertCallSiteResolvesTo(result, Constants.SharedGeneratedNamespace);
    }

    /// <summary>Renders the consumer source in the given namespace.</summary>
    /// <param name="consumerNamespace">The namespace the consumer's own code is declared in.</param>
    /// <returns>The source code.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string SourceIn(string consumerNamespace) =>
        SourceTemplate.Replace(NamespaceToken, consumerNamespace, StringComparison.Ordinal);

    /// <summary>
    /// Asserts that the <c>WhenChanged</c> call in the consumer source binds to the generated overload in the
    /// expected namespace, rather than to the runtime stub.
    /// </summary>
    /// <param name="result">The generator run to inspect.</param>
    /// <param name="expectedNamespace">The namespace the generated overload is expected to live in.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    /// <remarks>
    /// Only the assembly's own namespace carries the plain class name; anywhere another assembly could also emit,
    /// the class name carries the assembly so no two assemblies declare a type by the same name.
    /// </remarks>
    private static async Task AssertCallSiteResolvesTo(GeneratorTestResult result, string expectedNamespace)
    {
        var symbol = await CallSiteResolution.ResolveAsync(result, Constants.WhenChangedMethodName);
        var expectedClass = string.Equals(expectedNamespace, AssemblyNamespace, StringComparison.Ordinal)
            ? Constants.GeneratedExtensionClassName
            : AssemblyQualifiedClassName;

        await Assert.That(symbol).IsNotNull();
        await Assert.That(symbol!.ContainingType.Name).IsEqualTo(expectedClass);
        await Assert.That(symbol.ContainingType.ContainingNamespace.ToDisplayString()).IsEqualTo(expectedNamespace);
    }
}
