// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Models;
using ReactiveUI.SourceGenerators.WinForms;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests ReactiveUI package-profile detection and profile-specific command output.</summary>
public sealed class ReactiveUiIntegrationTests
{
    /// <summary>The number of source callbacks in a generated CombineLatest subscription.</summary>
    private const int CombineLatestCallbackCount = 2;

    /// <summary>
    /// Verifies that ReactiveUI 24 base output uses Primitives even when System.Reactive is
    /// independently available to the consuming compilation.
    /// </summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task V24BaseUsesPrimitivesWithoutGeneratedSystemReactiveReferences()
    {
        var references = TestCompilationReferences.CreateDefault();
        var (compilation, generatedSource) = RunCommandGenerator(
            """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace Compatibility;

            public partial class ViewModel : ReactiveObject
            {
                [ReactiveCommand]
                private void Save()
                {
                }
            }
            """,
            references);

        var integration = compilation.GetReactiveUiIntegration();

        await Assert.That(integration.Api).IsEqualTo(ReactiveUiApi.Primitives);
        await Assert.That(generatedSource.Contains("global::ReactiveUI.Primitives.RxVoid", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("global::System.Reactive", StringComparison.Ordinal)).IsFalse();
        await Assert.That(GetErrors(compilation)).IsEmpty();
    }

    /// <summary>Verifies that the ReactiveUI 24 System.Reactive variant selects its moved namespace.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task V24ReactiveUsesReactiveNamespaceAndSystemReactiveUnit()
    {
        var references = TestCompilationReferences.CreateForAssemblies(
            typeof(object).Assembly,
            typeof(Enumerable).Assembly,
            typeof(System.ComponentModel.INotifyPropertyChanged).Assembly,
            typeof(ReactiveUI.Reactive.ReactiveObject).Assembly,
            typeof(System.Reactive.Unit).Assembly,
            typeof(ReactiveCommandGenerator).Assembly);
        var (compilation, generatedSource) = RunCommandGenerator(
            """
            using ReactiveUI.Reactive;
            using ReactiveUI.SourceGenerators;

            namespace Compatibility;

            public partial class ViewModel : ReactiveObject
            {
                [ReactiveCommand]
                private void Save()
                {
                }
            }
            """,
            references);

        var integration = compilation.GetReactiveUiIntegration();

        await Assert.That(integration.Api).IsEqualTo(ReactiveUiApi.SystemReactive);
        await Assert.That(generatedSource.Contains("global::ReactiveUI.Reactive.ReactiveCommand", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("global::System.Reactive.Unit", StringComparison.Ordinal)).IsTrue();
        await Assert.That(GetErrors(compilation)).IsEmpty();
    }

    /// <summary>Verifies that the pre-v24 API remains on the original namespace and Unit type.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task V23UsesLegacyNamespaceAndSystemReactiveUnit()
    {
        var legacyReference = CreateLegacyReactiveUiReference();
        var references = TestCompilationReferences.CreateForAssemblies(
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(System.ComponentModel.INotifyPropertyChanged).Assembly,
                typeof(System.Reactive.Unit).Assembly,
                typeof(ReactiveCommandGenerator).Assembly)
            .Add(legacyReference);
        var (compilation, generatedSource) = RunCommandGenerator(
            """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace Compatibility;

            public partial class ViewModel : ReactiveObject
            {
                [ReactiveCommand]
                private void Save()
                {
                }
            }
            """,
            references);

        var integration = compilation.GetReactiveUiIntegration();

        await Assert.That(integration.Api).IsEqualTo(ReactiveUiApi.Legacy);
        await Assert.That(integration.IsNewerThan22).IsTrue();
        await Assert.That(generatedSource.Contains("global::ReactiveUI.ReactiveCommand", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("global::System.Reactive.Unit", StringComparison.Ordinal)).IsTrue();
        await Assert.That(GetErrors(compilation)).IsEmpty();
    }

    /// <summary>Verifies routed hosts preserve default content while disposing replaced routed views.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task RoutedControlHostEmitsDisposableCompositionBeforeConnecting()
    {
        const string source = """
            using ReactiveUI.SourceGenerators.WinForms;
            using System.ComponentModel;

            namespace Compatibility
            {
                [RoutedControlHost("System.Windows.Forms.UserControl")]
                public partial class RoutedHost
                {
                    private IContainer? components;
                    private void InitializeComponent()
                    {
                    }
                }
            }
            """;

        var (compilation, generatedSource) =
            RunWinFormsHostGenerator<RoutedControlHostGenerator>(source, ".RoutedControlHost.g.cs");
        var disposableRegistrationIndex = generatedSource.IndexOf("_disposables.Add(routeSubscription);", StringComparison.Ordinal);
        var connectIndex = generatedSource.IndexOf("routeSubscription.Connect(", StringComparison.Ordinal);

        await Assert.That(GetErrors(compilation)).IsEmpty();
        await Assert.That(disposableRegistrationIndex).IsGreaterThanOrEqualTo(0);
        await Assert.That(connectIndex).IsGreaterThan(disposableRegistrationIndex);
        await Assert.That(generatedSource.Contains("_routedView?.Dispose();", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("_defaultContent?.Dispose();", StringComparison.Ordinal)).IsFalse();
        await Assert.That(AreCombineLatestCallbacksSerialized(generatedSource)).IsTrue();
    }

    /// <summary>Verifies view-model hosts register their combined subscription before it connects.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ViewModelControlHostEmitsDisposableCompositionBeforeConnecting()
    {
        const string source = """
            using ReactiveUI.SourceGenerators.WinForms;
            using System.ComponentModel;

            namespace Compatibility
            {
                [ViewModelControlHost("System.Windows.Forms.UserControl")]
                public partial class ViewModelHost
                {
                    private IContainer? components;
                    private void InitializeComponent()
                    {
                    }
                }
            }
            """;

        var (compilation, generatedSource) =
            RunWinFormsHostGenerator<ViewModelControlHostGenerator>(source, ".ViewModelControlHost.g.cs");
        var disposableRegistrationIndex = generatedSource.IndexOf("_disposables.Add(viewModelSubscription);", StringComparison.Ordinal);
        var connectIndex = generatedSource.IndexOf("viewModelSubscription.Connect(", StringComparison.Ordinal);

        await Assert.That(GetErrors(compilation)).IsEmpty();
        await Assert.That(disposableRegistrationIndex).IsGreaterThanOrEqualTo(0);
        await Assert.That(connectIndex).IsGreaterThan(disposableRegistrationIndex);
        await Assert.That(generatedSource.Contains("private readonly DisposableCollection _disposables = new();", StringComparison.Ordinal)).IsTrue();
        await Assert.That(AreCombineLatestCallbacksSerialized(generatedSource)).IsTrue();
    }

    /// <summary>Runs a Windows Forms host generator and returns its generated host source.</summary>
    /// <typeparam name="TGenerator">The Windows Forms host generator type.</typeparam>
    /// <param name="source">The consumer source text.</param>
    /// <param name="generatedHintSuffix">The suffix identifying the generated host source.</param>
    /// <returns>The output compilation and generated host source.</returns>
    private static (Compilation Compilation, string GeneratedSource) RunWinFormsHostGenerator<TGenerator>(
        string source,
        string generatedHintSuffix)
        where TGenerator : IIncrementalGenerator, new()
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "WinFormsHostConsumer",
            [
                CSharpSyntaxTree.ParseText(SourceText.From(source, Encoding.UTF8), parseOptions),
                CSharpSyntaxTree.ParseText(
                    SourceText.From(TestCompilationReferences.WindowsDesktopStubs, Encoding.UTF8),
                    parseOptions,
                    path: "WindowsDesktopStubs.g.cs"),
            ],
            TestCompilationReferences.CreatePortableDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver
            .Create([new TGenerator()])
            .WithUpdatedParseOptions(parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        foreach (var generatorResult in driver.GetRunResult().Results)
        {
            foreach (var generatedSourceResult in generatorResult.GeneratedSources)
            {
                if (generatedSourceResult.HintName.EndsWith(generatedHintSuffix, StringComparison.Ordinal))
                {
                    return (outputCompilation, generatedSourceResult.SourceText.ToString());
                }
            }
        }

        throw new InvalidOperationException("The Windows Forms host generator did not produce source.");
    }

    /// <summary>Checks that generated CombineLatest callbacks execute within the subscription gate.</summary>
    /// <param name="generatedSource">The generated host source to inspect.</param>
    /// <returns><see langword="true"/> when both source callbacks are serialized by a lock.</returns>
    private static bool AreCombineLatestCallbacksSerialized(string generatedSource)
    {
        var callbackCount = 0;
        foreach (var node in CSharpSyntaxTree.ParseText(generatedSource).GetRoot().DescendantNodes())
        {
            if (node is not MethodDeclarationSyntax method
                || method.Identifier.ValueText is not ("SetLeft" or "SetRight"))
            {
                continue;
            }

            callbackCount++;
            if (!ContainsLockedOnNext(method))
            {
                return false;
            }
        }

        return callbackCount == CombineLatestCallbackCount;
    }

    /// <summary>Checks whether a generated callback invokes <c>_onNext</c> inside a lock statement.</summary>
    /// <param name="method">The generated callback method.</param>
    /// <returns><see langword="true"/> when the callback is serialized.</returns>
    private static bool ContainsLockedOnNext(MethodDeclarationSyntax method)
    {
        foreach (var node in method.DescendantNodes())
        {
            if (node is LockStatementSyntax lockStatement
                && lockStatement.Statement.ToString().Contains("_onNext(", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Runs the Reactive and ReactiveCommand generators against a consumer compilation.</summary>
    /// <param name="source">The consumer source text.</param>
    /// <param name="references">The metadata references for the consumer compilation.</param>
    /// <returns>The output compilation and generated command source.</returns>
    private static (Compilation Compilation, string GeneratedSource) RunCommandGenerator(
        string source,
        ImmutableArray<MetadataReference> references)
    {
        var compilation = CSharpCompilation.Create(
            "Consumer",
            [CSharpSyntaxTree.ParseText(SourceText.From(source, Encoding.UTF8), CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview))],
            references,
            new(OutputKind.DynamicallyLinkedLibrary));
        var driver = CSharpGeneratorDriver
            .Create([new ReactiveCommandGenerator(), new ReactiveGenerator()])
            .WithUpdatedParseOptions((CSharpParseOptions)compilation.SyntaxTrees.First().Options);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        string? generatedSource = null;
        foreach (var generatorResult in driver.GetRunResult().Results)
        {
            foreach (var generatedSourceResult in generatorResult.GeneratedSources)
            {
                if (generatedSourceResult.HintName.EndsWith(".ReactiveCommands.g.cs", StringComparison.Ordinal))
                {
                    generatedSource = generatedSourceResult.SourceText.ToString();
                    break;
                }
            }

            if (generatedSource is not null)
            {
                break;
            }
        }

        return (outputCompilation, generatedSource ?? throw new InvalidOperationException("The command generator did not produce source."));
    }

    /// <summary>Creates an in-memory metadata reference implementing the pre-v24 ReactiveUI API.</summary>
    /// <returns>The portable executable metadata reference.</returns>
    private static PortableExecutableReference CreateLegacyReactiveUiReference()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using System.Reactive;
            using System.Reflection;

            [assembly: AssemblyVersion("23.2.28.0")]

            namespace ReactiveUI;

            public interface IReactiveObject : INotifyPropertyChanged, INotifyPropertyChanging
            {
                void RaisePropertyChanged(PropertyChangedEventArgs args);
                void RaisePropertyChanging(PropertyChangingEventArgs args);
            }

            public class ReactiveObject : IReactiveObject
            {
                public event PropertyChangedEventHandler? PropertyChanged;
                public event PropertyChangingEventHandler? PropertyChanging;
                public void RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);
                public void RaisePropertyChanging(PropertyChangingEventArgs args) => PropertyChanging?.Invoke(this, args);
            }

            public sealed class ReactiveCommand<TInput, TOutput>
            {
            }

            public static class ReactiveCommand
            {
                public static ReactiveCommand<Unit, Unit> Create(Action execute) => new();
            }
            """;
        var compilation = CSharpCompilation.Create(
            "ReactiveUI",
            [CSharpSyntaxTree.ParseText(source)],
            TestCompilationReferences.CreateForAssemblies(
                typeof(object).Assembly,
                typeof(System.ComponentModel.INotifyPropertyChanged).Assembly,
                typeof(System.Reactive.Unit).Assembly),
            new(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        if (!result.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
        }

        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    /// <summary>Formats all compilation errors for assertion output.</summary>
    /// <param name="compilation">The compilation to inspect.</param>
    /// <returns>The formatted compilation errors.</returns>
    private static string GetErrors(Compilation compilation)
    {
        var errors = new List<string>();
        foreach (var diagnostic in compilation.GetDiagnostics())
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                errors.Add(diagnostic.ToString());
            }
        }

        return string.Join(Environment.NewLine, errors);
    }
}
