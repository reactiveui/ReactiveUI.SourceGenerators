// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections;
using System.Reflection;
using System.Runtime.Loader;
using ReactiveUI.SourceGenerators.WinForms;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Runs the generated Windows Forms hosts against the desktop stubs: they are constructed, show their default content,
/// and swap in the view for a routed or hosted view model, without a <c>WhenAny</c> call the binding engine has to
/// dispatch.
/// </summary>
public class ControlHostBehaviourTests
{
    /// <summary>The hosts, and a view model, view and view locator to route between.</summary>
    private const string Source = """
        using System.ComponentModel;
        using System.Windows.Forms;
        using ReactiveUI;
        using ReactiveUI.SourceGenerators.WinForms;

        namespace Hosts
        {
            [RoutedControlHost("System.Windows.Forms.UserControl")]
            public partial class RoutedHost
            {
                private IContainer? components;

                private void InitializeComponent()
                {
                }
            }

            [ViewModelControlHost("System.Windows.Forms.UserControl")]
            public partial class ViewModelHost
            {
                private IContainer? components;

                private void InitializeComponent()
                {
                }
            }

            public sealed class Shell : IScreen
            {
                public RoutingState Router { get; } = new();
            }

            public sealed class PageViewModel(IScreen screen) : ReactiveObject, IRoutableViewModel
            {
                public string UrlPathSegment => "page";

                public IScreen HostScreen => screen;
            }

            public sealed class PageView : UserControl, IViewFor<PageViewModel>
            {
                public PageViewModel? ViewModel { get; set; }

                object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (PageViewModel?)value; }
            }

            public sealed class Locator : IViewLocator
            {
                public IViewFor<TViewModel>? ResolveView<TViewModel>() where TViewModel : class => null;

                public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract) where TViewModel : class => null;

                public IViewFor? ResolveView(object? viewModel) => viewModel is PageViewModel ? new PageView() : null;

                public IViewFor? ResolveView(object? viewModel, string? contract) => ResolveView(viewModel);
            }
        }
        """;

    /// <summary>The hosts' default content property.</summary>
    private const string DefaultContent = nameof(DefaultContent);

    /// <summary>The routed host's router property.</summary>
    private const string Router = nameof(Router);

    /// <summary>The view-model host's view model property, and a view's.</summary>
    private const string ViewModel = nameof(ViewModel);

    /// <summary>The assembly compiled from <see cref="Source"/>, built once.</summary>
    private static readonly Lazy<Assembly> HostAssembly = new(Compile, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>The routed host shows its default content, then the view for the view model the router navigates to.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task RoutedHostShowsDefaultContentThenTheRoutedView()
    {
        var host = Create("Hosts.RoutedHost");
        var defaultContent = Create("System.Windows.Forms.UserControl");
        Set(host, nameof(ViewLocator), Create("Hosts.Locator"));

        Set(host, DefaultContent, defaultContent);
        await AssertShowsOnly(host, defaultContent);

        var shell = Create("Hosts.Shell");
        var router = Get(shell, Router);
        Set(host, Router, router);
        var navigationStack = (IList)Get(router, "NavigationStack");
        _ = navigationStack.Add(Activator.CreateInstance(HostAssembly.Value.GetType("Hosts.PageViewModel", throwOnError: true)!, shell));

        var shown = ShownControls(host);
        await Assert.That(shown.Count).IsEqualTo(1);
        await Assert.That(shown[0]!.GetType().FullName).IsEqualTo("Hosts.PageView");
    }

    /// <summary>The view-model host shows its default content, then the view for the view model it is given.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task ViewModelHostShowsDefaultContentThenTheViewForItsViewModel()
    {
        var host = Create("Hosts.ViewModelHost");
        var defaultContent = Create("System.Windows.Forms.UserControl");
        Set(host, nameof(ViewLocator), Create("Hosts.Locator"));

        Set(host, DefaultContent, defaultContent);
        await AssertShowsOnly(host, defaultContent);

        var viewModel = Activator.CreateInstance(HostAssembly.Value.GetType("Hosts.PageViewModel", throwOnError: true)!, Create("Hosts.Shell"))!;
        Set(host, ViewModel, viewModel);

        var shown = ShownControls(host);
        await Assert.That(shown.Count).IsEqualTo(1);
        await Assert.That(shown[0]!.GetType().FullName).IsEqualTo("Hosts.PageView");
        await Assert.That(Get(shown[0]!, ViewModel)).IsSameReferenceAs(viewModel);
    }

    /// <summary>A host in the global namespace, and a host nested in another type, get their generated members.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task HostsInTheGlobalNamespaceAndNestedTypesGetTheirMembers()
    {
        const string source = """
            using System.ComponentModel;
            using ReactiveUI.SourceGenerators.WinForms;

            [RoutedControlHost("System.Windows.Forms.UserControl")]
            public partial class GlobalRoutedHost
            {
                private IContainer? components;

                private void InitializeComponent()
                {
                }
            }

            namespace Hosts
            {
                public partial class Outer
                {
                    [ViewModelControlHost("System.Windows.Forms.UserControl")]
                    public partial class NestedViewModelHost
                    {
                        private IContainer? components;

                        private void InitializeComponent()
                        {
                        }
                    }
                }
            }
            """;
        var (output, _) = Generate(source);

        await Assert.That(output.GetTypeByMetadataName("GlobalRoutedHost")!.GetMembers(Router)).IsNotEmpty();
        await Assert.That(output.GetTypeByMetadataName("Hosts.Outer+NestedViewModelHost")!.GetMembers(ViewModel)).IsNotEmpty();
        await Assert.That(output.GetTypeByMetadataName("NestedViewModelHost")).IsNull();
        await Assert.That(GetErrors(output)).IsEmpty();
    }

    /// <summary>Neither host calls <c>WhenAny</c>, which the binding engine could not dispatch from generated code.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task HostsDoNotCallWhenAny()
    {
        foreach (var generated in Generate(Source).GeneratedSources)
        {
            await Assert.That(generated).DoesNotContain("WhenAny");
        }
    }

    /// <summary>Creates an instance of a type from the compiled assembly or its references.</summary>
    /// <param name="typeName">The type's full name.</param>
    /// <returns>The instance.</returns>
    private static object Create(string typeName) => Activator.CreateInstance(HostAssembly.Value.GetType(typeName, throwOnError: false) ?? FindType(typeName))!;

    /// <summary>Finds a type in the compiled assembly's load context.</summary>
    /// <param name="typeName">The type's full name.</param>
    /// <returns>The type.</returns>
    private static Type FindType(string typeName)
    {
        foreach (var assembly in AssemblyLoadContext.GetLoadContext(HostAssembly.Value)!.Assemblies)
        {
            if (assembly.GetType(typeName, throwOnError: false) is { } type)
            {
                return type;
            }
        }

        throw new InvalidOperationException($"{typeName} was not found.");
    }

    /// <summary>Reads a public property.</summary>
    /// <param name="target">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The property's value.</returns>
    private static object Get(object target, string name) => target.GetType().GetProperty(name)!.GetValue(target)!;

    /// <summary>Sets a public property.</summary>
    /// <param name="target">The object.</param>
    /// <param name="name">The property name.</param>
    /// <param name="value">The value.</param>
    private static void Set(object target, string name, object? value) => target.GetType().GetProperty(name)!.SetValue(target, value);

    /// <summary>Lists the controls a host shows.</summary>
    /// <param name="host">The host.</param>
    /// <returns>The controls.</returns>
    private static List<object?> ShownControls(object host)
    {
        var controls = new List<object?>();
        foreach (var control in (IEnumerable)Get(host, "Controls"))
        {
            controls.Add(control);
        }

        return controls;
    }

    /// <summary>Asserts that a host shows exactly one control, the given one.</summary>
    /// <param name="host">The host.</param>
    /// <param name="control">The control it should show.</param>
    /// <returns>A task to monitor the async.</returns>
    private static async Task AssertShowsOnly(object host, object control)
    {
        var shown = ShownControls(host);
        await Assert.That(shown.Count).IsEqualTo(1);
        await Assert.That(shown[0]).IsSameReferenceAs(control);
    }

    /// <summary>Formats a compilation's errors.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The errors, one per line.</returns>
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

    /// <summary>Runs both host generators over a consumer source and the desktop stubs.</summary>
    /// <param name="source">The consumer source.</param>
    /// <returns>The output compilation and the generated host sources.</returns>
    private static (Compilation Output, List<string> GeneratedSources) Generate(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var compilation = CSharpCompilation.Create(
            "ControlHostBehaviour",
            [
                CSharpSyntaxTree.ParseText(source, parseOptions),
                CSharpSyntaxTree.ParseText(TestCompilationReferences.WindowsDesktopStubs, parseOptions),
            ],
            TestCompilationReferences.CreatePortableDefault(),
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var driver = CSharpGeneratorDriver
            .Create([new RoutedControlHostGenerator().AsSourceGenerator(), new ViewModelControlHostGenerator().AsSourceGenerator()], parseOptions: parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

        var generated = new List<string>();
        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var file in result.GeneratedSources)
            {
                generated.Add(file.SourceText.ToString());
            }
        }

        return (output, generated);
    }

    /// <summary>Generates, compiles and loads the hosts.</summary>
    /// <returns>The loaded assembly.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "SES1402", Justification = "The test loads the assembly it has just compiled from its own source.")]
    private static Assembly Compile()
    {
        // ReactiveUI marks its default exception handler initialized before it assigns it, so a host constructed on
        // another thread at the same moment could read null. Initialize it here, once, before any host exists.
        _ = RxState.DefaultExceptionHandler;

        var (output, _) = Generate(Source);
        using var image = new MemoryStream();
        var emit = output.Emit(image);
        if (!emit.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, emit.Diagnostics));
        }

        image.Position = 0;
        return new AssemblyLoadContext(nameof(ControlHostBehaviourTests), isCollectible: true).LoadFromStream(image);
    }
}
