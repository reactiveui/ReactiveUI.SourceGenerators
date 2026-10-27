// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers how the generator reads the consumer's language and framework capabilities, which decide the shape of
/// every emitted overload. Both inputs come from the compilation rather than from anything under our control, so
/// each is read defensively and answered with the conservative value when it is absent.
/// </summary>
public class LanguageFeatureDetectionTests
{
    /// <summary>The name of the probe consumer, and the root namespace its build exposes.</summary>
    private const string ProbeName = "Probe";

    /// <summary>The hint name of the file that holds the generated WhenAnyValue dispatch.</summary>
    private const string WhenAnyValueDispatchHint = "WhenAnyValueDispatch.g.cs";

    /// <summary>The runtime's internal copy of the attribute, for a framework that does not declare one.</summary>
    private const string InternalAttributeSource = """
        namespace System.Runtime.CompilerServices
        {
            [AttributeUsage(AttributeTargets.Parameter)]
            internal sealed class CallerArgumentExpressionAttribute : Attribute
            {
                public CallerArgumentExpressionAttribute(string parameterName) => ParameterName = parameterName;

                public string ParameterName { get; }
            }
        }
        """;

    /// <summary>A runtime stub that declares its expression parameter with the attribute on every framework.</summary>
    private const string ReferencedStubSource = """
        using System;
        using System.Linq.Expressions;
        using System.Runtime.CompilerServices;

        namespace ReactiveUI.Binding
        {
            public static class ReactiveUIBindingExtensions
            {
                public static IObservable<TValue> WhenAnyValue<TSender, TValue>(
                    this TSender sender,
                    Expression<Func<TSender, TValue>> property1,
                    [CallerArgumentExpression("property1")] string property1Expression = "",
                    [CallerFilePath] string callerFilePath = "",
                    [CallerLineNumber] int callerLineNumber = 0)
                    where TSender : class => throw new NotImplementedException();
            }
        }
        """;

    /// <summary>A consumer that observes one property through the referenced stub.</summary>
    private const string ReferencedStubConsumerSource = """
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;

        namespace Probe
        {
            public sealed class Model : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;

                public string Name { get; set; }
            }

            public static class Usage
            {
                public static IObservable<string> Observe(Model model) => model.WhenAnyValue(x => x.Name);
            }
        }
        """;

    /// <summary>A minimal consumer, enough to build a compilation from.</summary>
    private const string MinimalSource = """
                                         namespace Probe
                                         {
                                             public class Marker
                                             {
                                             }
                                         }
                                         """;

    /// <summary>C# parse options carry the version the consumer asked for.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadLanguageVersion_CSharpParseOptions_ReturnsTheRequestedVersion()
    {
        var version = BindingGenerator.ReadLanguageVersion(new CSharpParseOptions(LanguageVersion.CSharp10));

        await Assert.That(version).IsEqualTo(LanguageVersion.CSharp10);
    }

    /// <summary>
    /// Options that are not C#'s carry no version to read, and the compiler default is the answer that keeps the
    /// pass running rather than failing every file in the compilation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadLanguageVersion_NonCSharpParseOptions_ReturnsTheCompilerDefault() =>
        await Assert.That(BindingGenerator.ReadLanguageVersion(null)).IsEqualTo(LanguageVersion.Default);

    /// <summary>A consumer on a framework that declares the attribute can have generated code apply it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasAccessibleExpressionAttribute_FrameworkDeclaresIt_ReturnsTrue()
    {
        var compilation = TestHelper.CreateCompilation(MinimalSource, LanguageVersion.CSharp10);

        await Assert.That(BindingGenerator.HasAccessibleExpressionAttribute(compilation)).IsTrue();
    }

    /// <summary>
    /// A compilation whose references do not declare the attribute cannot apply it, so the expression parameters
    /// are left off the generated overload and dispatch falls back to the file and line.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasAccessibleExpressionAttribute_NothingDeclaresIt_ReturnsFalse()
    {
        var compilation = CSharpCompilation.Create(ProbeName);

        await Assert.That(BindingGenerator.HasAccessibleExpressionAttribute(compilation)).IsFalse();
    }

    /// <summary>A runtime stub without expression-text parameters keeps the shorter interceptor signature.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StubHasExpressionParameters_AttributeAvailableButStubOmittedThem_ReturnsFalse()
    {
        const string source = """
            namespace Probe
            {
                public static class Stub
                {
                    public static void WhenAnyValue<TSender, TValue>(
                        TSender sender,
                        System.Linq.Expressions.Expression<System.Func<TSender, TValue>> property1,
                        string callerFilePath = "",
                        int callerLineNumber = 0) { }
                }
            }
            """;
        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var stub = compilation.GetTypeByMetadataName("Probe.Stub");

        await Assert.That(BindingGenerator.HasAccessibleExpressionAttribute(compilation)).IsTrue();
        await Assert.That(BindingGenerator.StubHasExpressionParameters(stub)).IsFalse();
    }

    /// <summary>
    /// The .NET Framework runtime declares the expression parameters through its own internal copy of the attribute,
    /// which the consumer cannot apply. The compiler never fills those parameters on the generated overload, so it
    /// declares them without the attribute, to match the stub's parameter list, and dispatches on the file and line.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NetFrameworkStub_AttributeOnlyInternalToRuntime_DispatchesOnFileAndLine()
    {
        var result = RunAgainstReferencedStub(Basic.Reference.Assemblies.Net472.References.All, true);

        await Assert.That(BindingGenerator.HasAccessibleExpressionAttribute(result.OutputCompilation)).IsFalse();
        await result.GeneratedSourceContains(WhenAnyValueDispatchHint, "string property1Expression = \"\",");
        await result.GeneratedSourceContains(WhenAnyValueDispatchHint, "if (callerLineNumber == ");
        await result.GeneratedSourceDoesNotContain(WhenAnyValueDispatchHint, "CallerArgumentExpression");
    }

    /// <summary>
    /// A framework that declares the attribute lets the generated overload apply it from C# 10, so the compiler fills
    /// the expression parameter and the overload dispatches on the lambda's text.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReferencedStub_FrameworkDeclaresAttribute_DispatchesOnExpressionText()
    {
#if NET11_0_OR_GREATER
        var frameworkReferences = Basic.Reference.Assemblies.Net110.References.All;
#elif NET10_0_OR_GREATER
        var frameworkReferences = Basic.Reference.Assemblies.Net100.References.All;
#else
        var frameworkReferences = Basic.Reference.Assemblies.Net80.References.All;
#endif
        var result = RunAgainstReferencedStub(frameworkReferences, false);

        await Assert.That(BindingGenerator.HasAccessibleExpressionAttribute(result.OutputCompilation)).IsTrue();
        await result.GeneratedSourceContains(
            WhenAnyValueDispatchHint,
            "[global::System.Runtime.CompilerServices.CallerArgumentExpression(\"property1\")]");
        await result.GeneratedSourceContains(WhenAnyValueDispatchHint, "if (property1Expression == \"x => x.Name\")");
    }

    /// <summary>A runtime stub with expression-text parameters exposes the longer interceptor signature.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StubHasExpressionParameters_RuntimeStubDeclaresThem_ReturnsTrue()
    {
        const string source = """
            namespace Probe
            {
                public static class Stub
                {
                    public static void WhenAnyValue<TSender, TValue>(
                        TSender sender,
                        System.Linq.Expressions.Expression<System.Func<TSender, TValue>> property1,
                        string property1Expression = "",
                        string callerFilePath = "",
                        int callerLineNumber = 0) { }
                }
            }
            """;
        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var stub = compilation.GetTypeByMetadataName("Probe.Stub");

        await Assert.That(BindingGenerator.StubHasExpressionParameters(stub)).IsTrue();
    }

    /// <summary>An interceptor matches a runtime stub built without caller-expression parameters.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NetFrameworkStyleStub_InterceptorMatchesShortSignature()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using System.Linq.Expressions;
            using System.Runtime.CompilerServices;
            using ReactiveUI.Binding;

            namespace ReactiveUI.Binding
            {
                public static class ReactiveUIBindingExtensions
                {
                    public static IObservable<TValue> WhenAnyValue<TSender, TValue>(
                        this TSender sender,
                        Expression<Func<TSender, TValue>> property1,
                        [CallerFilePath] string callerFilePath = "",
                        [CallerLineNumber] int callerLineNumber = 0)
                        where TSender : class => throw new NotImplementedException();
                }
            }

            public sealed class Model : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;
                public string Name { get; set; } = "initial";
            }

            public static class Usage
            {
                public static IObservable<string> Observe(Model model) => model.WhenAnyValue(x => x.Name, "", 0);
            }
            """;
        var parseOptions = TestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp11);
        var compilation = TestHelper.CreateCompilation(source, parseOptions, false, "TestAssembly", []);
        var result = TestHelper.RunGenerator(compilation, parseOptions, ProbeName, true);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(
            WhenAnyValueDispatchHint,
            InterceptableLocationReader.IsSupported ? "__Intercept_WhenAnyValue_" : "Concrete typed overload for WhenAnyValue");
    }

    /// <summary>
    /// Runs the generator over a C# 10 consumer whose runtime stub lives in a separately compiled assembly, as the
    /// runtime package's does, declaring its expression parameter with the attribute.
    /// </summary>
    /// <param name="frameworkReferences">The framework the stub and the consumer both compile against.</param>
    /// <param name="stubDeclaresAttribute">Whether the stub assembly declares its own internal copy of the attribute, as the runtime does where the framework has none.</param>
    /// <returns>The generator result.</returns>
    private static GeneratorTestResult RunAgainstReferencedStub(
        IEnumerable<MetadataReference> frameworkReferences,
        bool stubDeclaresAttribute)
    {
        var parseOptions = TestHelper.ParseOptionsFor(LanguageVersion.CSharp10);
        List<MetadataReference> references = [.. frameworkReferences];
        List<SyntaxTree> stubTrees = [CSharpSyntaxTree.ParseText(ReferencedStubSource, parseOptions)];
        if (stubDeclaresAttribute)
        {
            stubTrees.Add(CSharpSyntaxTree.ParseText(InternalAttributeSource, parseOptions));
        }

        var stub = CSharpCompilation.Create(
            "StubRuntime",
            stubTrees,
            references,
            new(OutputKind.DynamicallyLinkedLibrary));
        references.Add(stub.ToMetadataReference());
        var consumer = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(ReferencedStubConsumerSource, parseOptions)],
            references,
            new(OutputKind.DynamicallyLinkedLibrary));

        return TestHelper.RunGenerator(consumer, parseOptions, ProbeName, true);
    }
}
