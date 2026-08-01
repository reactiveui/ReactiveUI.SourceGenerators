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

    /// <summary>The fully qualified System.Reactive unit type name.</summary>
    private const string SystemReactiveUnitTypeName = "global::System.Reactive.Unit";

    /// <summary>The ReactiveUI assembly and declaration namespace.</summary>
    private const string ReactiveUiNamespace = "ReactiveUI";

    /// <summary>The minimum real ReactiveUI package major version covered by this project.</summary>
    private const int MinimumCurrentReactiveUiMajorVersion = 24;

    /// <summary>Consumer source covering the supported and rejected command option branches.</summary>
    private const string CommandOptionsSource = """
        using System;
        using System.Diagnostics.CodeAnalysis;
        using System.Threading;
        using System.Threading.Tasks;
        using ReactiveUI;
        using ReactiveUI.Primitives.Concurrency;
        using ReactiveUI.SourceGenerators;

        namespace Compatibility;

        public partial class CommandOptionsViewModel : ReactiveObject
        {
            [Reactive]
            private IObservable<bool> _generatedCanRun = null!;
            private readonly IObservable<bool> _fieldCanRun = null!;
            private readonly ISequencer _fieldSequencer = null!;

            private IObservable<bool> PropertyCanRun => null!;
            private IObservable<bool> MethodCanRun() => null!;
            private bool InvalidCanRun => true;
            private ISequencer PropertySequencer => null!;
            private object InvalidScheduler => null!;
            private IObservable<bool> DuplicateCanRun() => null!;
            private IObservable<bool> DuplicateCanRun(int value) => null!;
            private ISequencer DuplicateScheduler() => null!;
            private ISequencer DuplicateScheduler(int value) => null!;
            private ISequencer MethodScheduler() => null!;

            [ReactiveCommand(CanExecute = nameof(GeneratedCanRun))]
            private void GeneratedCanExecute() { }

            [ReactiveCommand(CanExecute = nameof(_fieldCanRun))]
            private void FieldCanExecute() { }

            [ReactiveCommand(CanExecute = nameof(PropertyCanRun))]
            private void PropertyCanExecute() { }

            [ReactiveCommand(CanExecute = nameof(MethodCanRun))]
            private void MethodCanExecute() { }

            [ReactiveCommand(CanExecute = "MissingCanRun")]
            private void MissingCanExecute() { }

            [ReactiveCommand(CanExecute = nameof(InvalidCanRun))]
            private void InvalidCanExecute() { }

            [ReactiveCommand(CanExecute = nameof(DuplicateCanRun))]
            private void AmbiguousCanExecute() { }

            [ReactiveCommand(CanExecute = null)]
            private void NullCanExecute() { }

            [ReactiveCommand(OutputScheduler = nameof(_fieldSequencer))]
            private void FieldScheduler() { }

            [ReactiveCommand(OutputScheduler = nameof(PropertySequencer))]
            private void PropertyScheduler() { }

            [ReactiveCommand(OutputScheduler = "global::ReactiveUI.RxSchedulers.MainThreadScheduler")]
            private void BuiltInScheduler() { }

            [ReactiveCommand(OutputScheduler = nameof(InvalidScheduler))]
            private void InvalidSchedulerCommand() { }

            [ReactiveCommand(OutputScheduler = nameof(DuplicateScheduler))]
            private void AmbiguousSchedulerCommand() { }

            [ReactiveCommand(OutputScheduler = nameof(MethodScheduler))]
            private void MethodSchedulerCommand() { }

            [ReactiveCommand(OutputScheduler = null)]
            private void NullScheduler() { }

            [ReactiveCommand(AccessModifier = PropertyAccessModifier.InternalProtected)]
            private void ProtectedInternal() { }

            [ReactiveCommand(AccessModifier = PropertyAccessModifier.PrivateProtected)]
            private void PrivateProtected() { }

            /// <summary>Saves the current value.</summary>
            [ReactiveCommand]
            [property: SuppressMessage("Coverage", "Generated")]
            private async Task m_saveAsync(string value, CancellationToken cancellationToken) => await Task.CompletedTask;

            [ReactiveCommand]
            private void __delete() { }

            [ReactiveCommand]
            private void Unsupported(int first, int second) { }
        }
        """;

    /// <summary>Consumer source covering reactive-field generation options.</summary>
    private const string ReactiveFieldOptionsSource = """
        using System;
        using ReactiveUI;
        using ReactiveUI.SourceGenerators;

        namespace Compatibility;

        public partial class ReactiveFieldOptionsViewModel : ReactiveObject
        {
            public string Other { get; set; } = string.Empty;

            [Reactive("", nameof(Other), nameof(Value))]
            [property: Obsolete("forwarded")]
            private string? _value;

            [Reactive(SetModifier = AccessModifier.Protected, Inheritance = InheritanceModifier.Virtual)]
            private string? _protectedVirtual;

            [Reactive(SetModifier = AccessModifier.Internal, Inheritance = InheritanceModifier.New)]
            private string? _internalNew;

            [Reactive(SetModifier = AccessModifier.Private)]
            private string? _private;

            [Reactive(SetModifier = AccessModifier.InternalProtected)]
            private string? _internalProtected;

            [Reactive(SetModifier = AccessModifier.PrivateProtected)]
            private string? _privateProtected;

            [Reactive(SetModifier = AccessModifier.Init, UseRequired = true)]
            private string _required = string.Empty;
        }
        """;

    /// <summary>Consumer source covering partial-property and invalid-target generation paths.</summary>
    private const string ReactivePartialPropertySource = """
        using ReactiveUI;
        using ReactiveUI.SourceGenerators;

        namespace Compatibility;

        public class PartialPropertyBase : ReactiveObject
        {
            public virtual string Overridden { get; set; } = string.Empty;
        }

        public partial class PartialPropertyViewModel : PartialPropertyBase
        {
            /// <summary>Documented reactive property.</summary>
            [Reactive(nameof(Other))]
            public partial string? Documented { get; set; }

            [Reactive]
            protected internal partial string Restricted { get; private protected set; }

            [Reactive]
            public virtual partial string Virtual { get; set; }

            [Reactive]
            public override partial string Overridden { get; set; }

            public string Other { get; set; } = string.Empty;

            [Reactive]
            public string Ordinary { get; set; } = string.Empty;
        }

        public partial class InvalidReactiveTarget
        {
            [Reactive]
            private int _invalid;

            [Reactive]
            public partial int InvalidProperty { get; set; }
        }

        public partial class ReactiveNameCollision : ReactiveObject
        {
            [Reactive]
            private int Name;
        }
        """;

    /// <summary>Consumer source covering observable-as-property generation and rejection paths.</summary>
    private const string ObservableAsPropertyOptionsSource = """
        using System;
        using ReactiveUI;
        using ReactiveUI.SourceGenerators;

        namespace Compatibility;

        public partial class ObservableOptionsViewModel : ReactiveObject
        {
            [ObservableAsProperty(UseProtected = true, InitialValue = "42")]
            public IObservable<int> NumberStream => null!;

            [ObservableAsProperty(PropertyName = "Status", InitialValue = "ready")]
            public IObservable<string?> StatusStream => null!;

            [ObservableAsProperty(PropertyName = "Computed", UseProtected = true)]
            public IObservable<int> Compute() => null!;

            [ObservableAsProperty]
            public IObservable<string?> Maybe() => null!;

            [ObservableAsProperty]
            public IObservable<int> HasParameter(int value) => null!;

            [ObservableAsProperty]
            public int NotObservable() => 0;

            [ObservableAsProperty]
            public int OrdinaryProperty { get; set; }

            [ObservableAsProperty(ReadOnly = false, UseProtected = true, InitialValue = "7")]
            public partial int PartialValue { get; }

            [ObservableAsProperty]
            private int _defaultField;

            [ObservableAsProperty(UseProtected = true)]
            private string? _nullableField;

            [ObservableAsProperty(ReadOnly = false)]
            private int _mutableField = 1;

            [ObservableAsProperty]
            private int Collision;
        }

        public partial class InvalidObservableTarget
        {
            [ObservableAsProperty]
            public IObservable<int> InvalidProperty => null!;

            [ObservableAsProperty]
            public IObservable<int> InvalidMethod() => null!;

            [ObservableAsProperty]
            private int _invalidField;
        }
        """;

    /// <summary>Verifies this test project is executing against a real ReactiveUI 24-or-newer package.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task CurrentTestsUseRealReactiveUiV24OrNewerPackage()
    {
        var version = typeof(ReactiveObject).Assembly.GetName().Version
            ?? throw new InvalidOperationException("ReactiveUI did not expose an assembly version.");

        await Assert.That(version.Major).IsGreaterThanOrEqualTo(MinimumCurrentReactiveUiMajorVersion);
    }

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
        await Assert.That(integration.Namespace).IsEqualTo("global::ReactiveUI");
        await Assert.That(integration.DeclarationNamespace).IsEqualTo(ReactiveUiNamespace);
        await Assert.That(integration.VoidTypeName).IsEqualTo("global::ReactiveUI.Primitives.RxVoid");
        await Assert.That(integration.UsingDirectives).IsEqualTo("using ReactiveUI;");
        await Assert.That(generatedSource.Contains("global::ReactiveUI.Primitives.RxVoid", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("global::System.Reactive", StringComparison.Ordinal)).IsFalse();
        await Assert.That(GetErrors(compilation)).IsEmpty();
    }

    /// <summary>Verifies a command can consume the observable property generated from a reactive field.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ReactiveCommandUsesCanExecutePropertyGeneratedFromReactiveField()
    {
        var (compilation, generatedSource) = RunCommandGenerator(
            """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace Compatibility;

            public partial class ViewModel : ReactiveObject
            {
                [Reactive]
                private IObservable<bool> _canRun = null!;

                [ReactiveCommand(CanExecute = nameof(CanRun))]
                private void Run()
                {
                }
            }
            """,
            TestCompilationReferences.CreateDefault());

        await Assert.That(generatedSource.Contains("ReactiveCommand.Create(Run, CanRun)", StringComparison.Ordinal)).IsTrue();
        await Assert.That(GetErrors(compilation)).IsEmpty();
    }

    /// <summary>Exercises supported and rejected command options through the generated output.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ReactiveCommandOptionsGenerateOnlyValidArgumentsAndModifiers()
    {
        var (compilation, generatedSource) = RunCommandGenerator(CommandOptionsSource, TestCompilationReferences.CreateDefault());

        await Assert.That(generatedSource.Contains("ReactiveCommand.Create(GeneratedCanExecute, GeneratedCanRun)", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("ReactiveCommand.Create(FieldCanExecute, _fieldCanRun)", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("ReactiveCommand.Create(PropertyCanExecute, PropertyCanRun)", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("ReactiveCommand.Create(MethodCanExecute, MethodCanRun())", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("outputScheduler: _fieldSequencer", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("outputScheduler: PropertySequencer", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("outputScheduler: MethodScheduler()", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("outputScheduler: global::ReactiveUI.RxSchedulers.MainThreadScheduler", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("protected internal global::ReactiveUI.ReactiveCommand", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("private protected global::ReactiveUI.ReactiveCommand", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("SaveCommand", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("DeleteCommand", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("UnsupportedCommand", StringComparison.Ordinal)).IsFalse();
        await Assert.That(generatedSource.Contains("/// <summary>Saves the current value.</summary>", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("SuppressMessage", StringComparison.Ordinal)).IsTrue();
        await Assert.That(GetErrors(compilation)).IsEmpty();
    }

    /// <summary>Verifies all reactive-field modifiers and notification filters through generated source.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ReactiveFieldOptionsGenerateExpectedPropertyShapes()
    {
        var (compilation, generatedSource, diagnostics) = RunReactiveGenerator(ReactiveFieldOptionsSource, LanguageVersion.Preview);

        await Assert.That(generatedSource.Contains("protected set", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("internal set", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("private set", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("protected internal set", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("private protected set", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("required string Required", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains(" virtual string? ProtectedVirtual", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains(" new string? InternalNew", StringComparison.Ordinal)).IsTrue();
        await Assert.That(CountOccurrences(generatedSource, "RaisePropertyChanged(nameof(Other))")).IsEqualTo(1);
        await Assert.That(generatedSource.Contains("RaisePropertyChanged(nameof(Value))", StringComparison.Ordinal)).IsFalse();
        await Assert.That(generatedSource.Contains("Obsolete", StringComparison.Ordinal)).IsTrue();
        await Assert.That(diagnostics).IsEmpty();
        await Assert.That(GetErrors(compilation)).IsEmpty();
    }

    /// <summary>Verifies partial-property generation, invalid targets, and name-collision diagnostics.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ReactivePartialPropertiesCoverLanguageAndDiagnosticPaths()
    {
        var (_, previewSource, previewDiagnostics) = RunReactiveGenerator(ReactivePartialPropertySource, LanguageVersion.Preview);
        var (_, csharp13Source, csharp13Diagnostics) = RunReactiveGenerator(ReactivePartialPropertySource, LanguageVersion.CSharp13);
        var previewIds = GetDiagnosticIds(previewDiagnostics);

        await Assert.That(previewSource.Contains("get => field;", StringComparison.Ordinal)).IsTrue();
        await Assert.That(csharp13Source.Contains("private string? _documented;", StringComparison.Ordinal)).IsTrue();
        await Assert.That(previewSource.Contains("private protected set", StringComparison.Ordinal)).IsTrue();
        await Assert.That(previewSource.Contains(" virtual partial string Virtual", StringComparison.Ordinal)).IsTrue();
        await Assert.That(previewSource.Contains(" override partial string Overridden", StringComparison.Ordinal)).IsTrue();
        await Assert.That(previewSource.Contains("/// <summary>Documented reactive property.</summary>", StringComparison.Ordinal)).IsTrue();
        await Assert.That(previewIds).Contains("RXUISG0009");
        await Assert.That(previewIds).Contains("RXUISG0018");
        await Assert.That(GetDiagnosticIds(csharp13Diagnostics)).IsEquivalentTo(previewIds);
    }

    /// <summary>Verifies observable-as-property options and rejected member shapes end to end.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ObservableAsPropertyOptionsCoverGeneratedAndDiagnosticPaths()
    {
        var (_, generatedSource, diagnostics) = RunObservableAsPropertyGenerator(ObservableAsPropertyOptionsSource);
        var diagnosticIds = GetDiagnosticIds(diagnostics);

        await Assert.That(generatedSource.Contains("protected ReactiveUI.ObservableAsPropertyHelper<int>?", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("private string? _status = \"ready\";", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("public string? Status", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("Compute()!.ToProperty", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("NumberStream!.ToProperty", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("public partial int PartialValue", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("protected ReactiveUI.ObservableAsPropertyHelper<int>? _partialValueHelper", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("private readonly ReactiveUI.ObservableAsPropertyHelper<int> _defaultFieldHelper", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("private ReactiveUI.ObservableAsPropertyHelper<int>? _mutableFieldHelper", StringComparison.Ordinal)).IsTrue();
        await Assert.That(diagnosticIds).Contains("RXUISG0009");
        await Assert.That(diagnosticIds).Contains("RXUISG0017");
        await Assert.That(diagnosticIds).Contains("RXUISG0018");
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
        await Assert.That(integration.Namespace).IsEqualTo("global::ReactiveUI.Reactive");
        await Assert.That(integration.DeclarationNamespace).IsEqualTo("ReactiveUI.Reactive");
        await Assert.That(integration.VoidTypeName).IsEqualTo(SystemReactiveUnitTypeName);
        await Assert.That(integration.UsingDirectives).IsEqualTo("using ReactiveUI;\nusing ReactiveUI.Reactive;");
        await Assert.That(generatedSource.Contains("global::ReactiveUI.Reactive.ReactiveCommand", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains(SystemReactiveUnitTypeName, StringComparison.Ordinal)).IsTrue();
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
        await Assert.That(generatedSource.Contains(SystemReactiveUnitTypeName, StringComparison.Ordinal)).IsTrue();
        await Assert.That(GetErrors(compilation)).IsEmpty();
    }

    /// <summary>Exercises package detection when the command API is absent, referenced, or exposes primitive void.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task IntegrationDetectionCoversReferencedAndCommandMemberShapes()
    {
        var baseReferences = TestCompilationReferences.CreateForAssemblies(typeof(object).Assembly);
        var absent = CreateIntegrationCompilation(string.Empty, baseReferences).GetReactiveUiIntegration();
        var referencedV22 = CreateIntegrationCompilation(
            string.Empty,
            baseReferences.Add(CreateReactiveUiMarkerReference("22.0.0.0"))).GetReactiveUiIntegration();
        var referencedV23 = CreateIntegrationCompilation(
            string.Empty,
            baseReferences.Add(CreateReactiveUiMarkerReference("23.0.0.0"))).GetReactiveUiIntegration();
        var nonMethodCreate = CreateIntegrationCompilation(
            """
            namespace ReactiveUI;
            public static class ReactiveCommand
            {
                public static int Create;
            }
            """,
            baseReferences).GetReactiveUiIntegration();
        var primitive = CreateIntegrationCompilation(
            """
            namespace ReactiveUI.Primitives
            {
                public readonly struct RxVoid
                {
                }
            }

            namespace ReactiveUI
            {
                public sealed class ReactiveCommand<T>
                {
                }

                public static class ReactiveCommand
                {
                    public static ReactiveCommand<Primitives.RxVoid> Create() => new();
                }
            }
            """,
            baseReferences).GetReactiveUiIntegration();

        await Assert.That(absent.IsNewerThan22).IsFalse();
        await Assert.That(referencedV22.Api).IsEqualTo(ReactiveUiApi.Legacy);
        await Assert.That(referencedV22.IsNewerThan22).IsFalse();
        await Assert.That(referencedV23.Api).IsEqualTo(ReactiveUiApi.Legacy);
        await Assert.That(referencedV23.IsNewerThan22).IsTrue();
        await Assert.That(nonMethodCreate.Api).IsEqualTo(ReactiveUiApi.Legacy);
        await Assert.That(nonMethodCreate.IsNewerThan22).IsFalse();
        await Assert.That(primitive.Api).IsEqualTo(ReactiveUiApi.Primitives);
        await Assert.That(primitive.IsNewerThan22).IsTrue();
    }

    /// <summary>Verifies automatic forwarding for every supported validation and serialization family.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ReactiveFieldsForwardAllSupportedAttributeFamilies()
    {
        const string source = """
            using System.ComponentModel.DataAnnotations;
            using System.Runtime.Serialization;
            using System.Text.Json.Serialization;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace Compatibility;

            public partial class ForwardingViewModel : ReactiveObject
            {
                [Required, Reactive]
                private string? _required;

                [JsonInclude, Reactive]
                private string? _json;

                [UIHint("Text"), Reactive]
                private string? _hint;

                [ScaffoldColumn(true), Reactive]
                private string? _scaffold;

                [Display(Name = "Shown"), Reactive]
                private string? _display;

                [Editable(true), Reactive]
                private string? _editable;

                [Key, Reactive]
                private int _key;

                [DataMember, Reactive]
                private string? _member;

                [IgnoreDataMember, Reactive]
                private string? _ignoredMember;
            }
            """;

        var (compilation, generatedSource, diagnostics) = RunReactiveGenerator(source, LanguageVersion.Preview);

        await Assert.That(generatedSource.Contains("RequiredAttribute", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("JsonIncludeAttribute", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("UIHintAttribute", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("ScaffoldColumnAttribute", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("DisplayAttribute", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("EditableAttribute", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("KeyAttribute", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("DataMemberAttribute", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("IgnoreDataMemberAttribute", StringComparison.Ordinal)).IsTrue();
        await Assert.That(diagnostics).IsEmpty();
        await Assert.That(GetErrors(compilation)).IsEmpty();
    }

    /// <summary>Verifies unresolved and nonconstant forwarded attribute expressions produce generator diagnostics.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ObservableAsPropertyReportsInvalidForwardedAttributeForms()
    {
        const string source = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace Compatibility;

            public partial class InvalidForwardingViewModel : ReactiveObject
            {
                [ObservableAsProperty]
                [property: Missing]
                private int _missing;

                [ObservableAsProperty]
                [property: Obsolete(GetMessage())]
                private int _invalidExpression;

                private static string GetMessage() => "message";
            }
            """;

        var (_, _, diagnostics) = RunObservableAsPropertyGenerator(source);
        var diagnosticIds = GetDiagnosticIds(diagnostics);

        await Assert.That(diagnosticIds).Contains("RXUISG0012");
        await Assert.That(diagnosticIds).Contains("RXUISG0013");
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
                [System.Obsolete]
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
        await Assert.That(generatedSource.Contains("ObsoleteAttribute", StringComparison.Ordinal)).IsFalse();
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
                [System.Obsolete]
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
        await Assert.That(generatedSource.Contains("ObsoleteAttribute", StringComparison.Ordinal)).IsFalse();
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

    /// <summary>Runs the reactive-property generator and collects all generated property sources.</summary>
    /// <param name="source">The consumer source text.</param>
    /// <param name="languageVersion">The language version used for the consumer compilation.</param>
    /// <returns>The output compilation, generated property source, and generator diagnostics.</returns>
    private static (Compilation Compilation, string GeneratedSource, ImmutableArray<Diagnostic> Diagnostics) RunReactiveGenerator(
        string source,
        LanguageVersion languageVersion) =>
        RunGenerator(
            source,
            languageVersion,
            new ReactiveGenerator(),
            static hintName => hintName.EndsWith("Properties.g.cs", StringComparison.Ordinal));

    /// <summary>Runs the observable-as-property generator and collects its product sources.</summary>
    /// <param name="source">The consumer source text.</param>
    /// <returns>The output compilation, generated source, and generator diagnostics.</returns>
    private static (Compilation Compilation, string GeneratedSource, ImmutableArray<Diagnostic> Diagnostics) RunObservableAsPropertyGenerator(
        string source) =>
        RunGenerator(
            source,
            LanguageVersion.Preview,
            new ObservableAsPropertyGenerator(),
            static hintName => hintName.EndsWith(".ObservableAsProperties.g.cs", StringComparison.Ordinal)
                || hintName.EndsWith(".ObservableAsPropertyFromObservable.g.cs", StringComparison.Ordinal));

    /// <summary>Runs one incremental generator and collects selected generated sources.</summary>
    /// <param name="source">The consumer source text.</param>
    /// <param name="languageVersion">The consumer language version.</param>
    /// <param name="generator">The generator to execute.</param>
    /// <param name="includeHint">Selects product output hint names.</param>
    /// <returns>The output compilation, selected source, and generator diagnostics.</returns>
    private static (Compilation Compilation, string GeneratedSource, ImmutableArray<Diagnostic> Diagnostics) RunGenerator(
        string source,
        LanguageVersion languageVersion,
        IIncrementalGenerator generator,
        Func<string, bool> includeHint)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);
        var compilation = CSharpCompilation.Create(
            "ReactiveConsumer",
            [CSharpSyntaxTree.ParseText(SourceText.From(source, Encoding.UTF8), parseOptions)],
            TestCompilationReferences.CreateDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create([generator]).WithUpdatedParseOptions(parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        var generatedSource = new StringBuilder();
        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var sourceResult in result.GeneratedSources)
            {
                if (includeHint(sourceResult.HintName))
                {
                    _ = generatedSource.AppendLine(sourceResult.SourceText.ToString());
                }
            }
        }

        return (outputCompilation, generatedSource.ToString(), diagnostics);
    }

    /// <summary>Copies diagnostic identifiers without allocating a LINQ iterator chain.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <returns>The diagnostic identifiers.</returns>
    private static string[] GetDiagnosticIds(ImmutableArray<Diagnostic> diagnostics)
    {
        var ids = new string[diagnostics.Length];
        for (var index = 0; index < diagnostics.Length; index++)
        {
            ids[index] = diagnostics[index].Id;
        }

        return ids;
    }

    /// <summary>Counts non-overlapping occurrences of a value in generated source.</summary>
    /// <param name="source">The source to search.</param>
    /// <param name="value">The value to count.</param>
    /// <returns>The number of occurrences.</returns>
    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var startIndex = 0;
        while ((startIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal)) >= 0)
        {
            count++;
            startIndex += value.Length;
        }

        return count;
    }

    /// <summary>Creates a compilation used to exercise ReactiveUI package-profile detection.</summary>
    /// <param name="source">The source declaring any command API surface.</param>
    /// <param name="references">The references visible to the compilation.</param>
    /// <returns>The created compilation.</returns>
    private static CSharpCompilation CreateIntegrationCompilation(
        string source,
        ImmutableArray<MetadataReference> references) =>
        CSharpCompilation.Create(
            "IntegrationDetectionConsumer",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new(OutputKind.DynamicallyLinkedLibrary));

    /// <summary>Creates a ReactiveUI-named reference with no command type and a specified assembly version.</summary>
    /// <param name="version">The assembly version to emit.</param>
    /// <returns>The portable executable metadata reference.</returns>
    private static PortableExecutableReference CreateReactiveUiMarkerReference(string version)
    {
        var source = $$"""
            using System.Reflection;
            [assembly: AssemblyVersion("{{version}}")]
            namespace ReactiveUI;
            public sealed class PackageMarker
            {
            }
            """;
        var compilation = CSharpCompilation.Create(
            ReactiveUiNamespace,
            [CSharpSyntaxTree.ParseText(source)],
            TestCompilationReferences.CreateForAssemblies(typeof(object).Assembly),
            new(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        if (!result.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
        }

        return MetadataReference.CreateFromImage(stream.ToArray());
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
            ReactiveUiNamespace,
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
