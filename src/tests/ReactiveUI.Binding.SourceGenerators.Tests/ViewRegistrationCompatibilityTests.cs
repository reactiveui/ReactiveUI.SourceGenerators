// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers finding views through ReactiveUI.SourceGenerators' <c>[IViewFor]</c>, whose <c>IViewFor&lt;T&gt;</c> another
/// generator adds, and finding views in a System.Reactive consumer.
/// </summary>
public class ViewRegistrationCompatibilityTests
{
    /// <summary>The hint name of the generated view dispatch file.</summary>
    private const string DispatchHintName = "ViewDispatch.g.cs";

    /// <summary>The attributes ReactiveUI.SourceGenerators declares into each consumer, internal to it.</summary>
    private const string SourceGeneratorsAttributes = """
        namespace ReactiveUI.SourceGenerators
        {
            internal enum SplatRegistrationType
            {
                None = 0,
                LazySingleton = 1,
                Constant = 2,
                PerRequest = 3,
            }

            [System.AttributeUsage(System.AttributeTargets.Class)]
            internal sealed class IViewForAttribute<T> : System.Attribute
            {
                public string Label { get; set; }

                public SplatRegistrationType RegistrationType { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.Class)]
            internal sealed class IViewForAttribute : System.Attribute
            {
                public IViewForAttribute(string viewModelType)
                {
                }

                public SplatRegistrationType RegistrationType { get; set; }
            }
        }
        """;

    /// <summary>Views declared only through <c>[IViewFor]</c>, each naming its view model a different way.</summary>
    private const string AttributeViews = """
        using ReactiveUI.Binding;
        using ReactiveUI.SourceGenerators;

        namespace TestApp
        {
            public class Vm1 { }
            public class Vm2 { }
            public class Vm3 { }
            public class Gen<T> { }
            public class Vm5 { }
            public class Vm6 { }
            public class Vm7 { }
            public class Vm8 { }
            public class Vm9 { }
            public class Vm10 { }
            public class Vm11 { }
            public class Vm12 { }
            public class Vm13 { }
            public class Vm14 { }

            public abstract class ViewBase<T> : IViewFor<T>
                where T : class
            {
                public T ViewModel { get; set; }

                object IViewFor.ViewModel
                {
                    get => ViewModel;
                    set => ViewModel = (T)value;
                }
            }

            [IViewFor<Vm1>] public partial class View1 { }
            [IViewFor("TestApp.Vm2")] public partial class View2 { }
            [IViewFor(nameof(Vm3))] public partial class View3 { }
            [IViewFor("Gen<int>")] public partial class View4 { }
            public partial class Outer { [IViewFor<Vm5>] public partial class View5 { } }
            [IViewFor<Vm6>(RegistrationType = SplatRegistrationType.LazySingleton)] public partial class View6 { }
            [ViewContract("wide")] [IViewFor<Vm7>] public partial class View7 { }
            [ExcludeFromViewRegistration] [IViewFor<Vm8>] public partial class View8 { }
            [IViewFor<Vm9>] public partial class Plain9 { }
            [IViewFor<Vm10>(RegistrationType = SplatRegistrationType.Constant)] public partial class View10 { }
            [IViewFor<Vm11>(RegistrationType = SplatRegistrationType.PerRequest)] public partial class View11 { }
            [IViewFor<Vm12>(Label = "other")] public partial class View12 { }
            [IViewFor<Vm13>] public abstract partial class AbstractView13 { }
            [IViewFor<Vm14>] public partial class GenericView14<T> { }

            public static class Usage
            {
                public static string Run()
                {
                    var locator = new DefaultViewLocator();
                    if (!(locator.ResolveView(new Vm1(), null) is View1)) return "generic";
                    if (!(locator.ResolveView(new Vm2(), null) is View2)) return "qualified name";
                    if (!(locator.ResolveView(new Vm3(), null) is View3)) return "nameof";
                    if (!(locator.ResolveView(new Gen<int>(), null) is View4)) return "generic name";
                    if (!(locator.ResolveView(new Vm5(), null) is Outer.View5)) return "nested";
                    var first = locator.ResolveView(new Vm6(), null);
                    if (!(first is View6) || !ReferenceEquals(first, locator.ResolveView(new Vm6(), null))) return "lazy singleton";
                    if (!(locator.ResolveView(new Vm7(), "wide") is View7) || locator.ResolveView(new Vm7(), null) != null) return "contract";
                    if (locator.ResolveView(new Vm8(), null) != null) return "excluded";
                    if (locator.ResolveView(new Vm9(), null) != null) return "no interface";
                    var constant = locator.ResolveView(new Vm10(), null);
                    if (!(constant is View10) || !ReferenceEquals(constant, locator.ResolveView(new Vm10(), null))) return "constant";
                    var perRequest = locator.ResolveView(new Vm11(), null);
                    if (!(perRequest is View11) || ReferenceEquals(perRequest, locator.ResolveView(new Vm11(), null))) return "per request";
                    if (!(locator.ResolveView(new Vm12(), null) is View12)) return "other argument";
                    if (locator.ResolveView(new Vm13(), null) != null || locator.ResolveView(new Vm14(), null) != null) return "abstract or generic";
                    return string.Empty;
                }
            }
        }
        """;

    /// <summary>
    /// What ReactiveUI.SourceGenerators adds to the views above, except <c>Plain9</c>, whose base it does not support.
    /// The views reach <c>IViewFor&lt;T&gt;</c> through <c>ViewBase&lt;T&gt;</c>, an open generic nothing registers itself.
    /// </summary>
    private const string AddedInterfaces = """
        namespace TestApp
        {
            public partial class View1 : ViewBase<Vm1> { }
            public partial class View2 : ViewBase<Vm2> { }
            public partial class View3 : ViewBase<Vm3> { }
            public partial class View4 : ViewBase<Gen<int>> { }
            public partial class Outer { public partial class View5 : ViewBase<Vm5> { } }
            public partial class View6 : ViewBase<Vm6> { }
            public partial class View7 : ViewBase<Vm7> { }
            public partial class View8 : ViewBase<Vm8> { }
            public partial class View10 : ViewBase<Vm10> { }
            public partial class View11 : ViewBase<Vm11> { }
            public partial class View12 : ViewBase<Vm12> { }
        }
        """;

    /// <summary>Splitting a dispatch file on a view model's type test gives this many parts when the test appears once.</summary>
    private const int PartsForOneBranch = 2;

    /// <summary>
    /// A view declared only through <c>[IViewFor]</c> resolves once the other generator has added its interface, however
    /// the attribute names the view model. Contracts, exclusion and single instances apply, and a class the other
    /// generator left alone compiles and resolves to null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AttributeViews_ResolveOnceTheOtherGeneratorAddsTheInterface()
    {
        var parseOptions = TestHelper.ParseOptionsFor(LanguageVersion.CSharp11);
        var compilation = TestHelper.CreateCompilation($"{AttributeViews}\n{SourceGeneratorsAttributes}", parseOptions, false, "TestAssembly", []);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new BindingGenerator().AsSourceGenerator(), new InterfaceAddingGenerator(AddedInterfaces).AsSourceGenerator()],
            null,
            parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        var result = new GeneratorTestResult(driver, output, diagnostics);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchHintName, "(object)new global::TestApp.Plain9() as global::ReactiveUI.Binding.IViewFor");

        var (assembly, context) = TestHelper.EmitAndLoad(result, true);
        try
        {
            var run = assembly.GetType("TestApp.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((string)run.Invoke(null, null)!).IsEqualTo(string.Empty);
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>A view that implements <c>IViewFor&lt;T&gt;</c> itself and also carries <c>[IViewFor]</c> is registered once.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InterfaceAndAttribute_RegisterTheViewOnce()
    {
        const string Source = """
            using ReactiveUI.Binding;
            using ReactiveUI.SourceGenerators;

            namespace TestApp
            {
                public class PanelViewModel { }

                [IViewFor<PanelViewModel>]
                public partial class PanelView : IViewFor<PanelViewModel>
                {
                    public PanelViewModel ViewModel { get; set; }

                    object IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (PanelViewModel)value;
                    }
                }
            }
            """;

        var result = TestHelper.RunGenerator($"{Source}\n{SourceGeneratorsAttributes}", LanguageVersion.CSharp11);
        await result.CompilationSucceeds();
        var dispatch = result.GeneratedSources[DispatchHintName];
        await Assert.That(dispatch.Split("instance is global::TestApp.PanelViewModel").Length).IsEqualTo(PartsForOneBranch);
        await Assert.That(dispatch).DoesNotContain("(object)new global::TestApp.PanelView()");
    }

    /// <summary>An <c>[IViewFor]</c> naming a type that does not exist, or no type, registers nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UnresolvableViewModel_RegistersNothing()
    {
        const string Source = """
            using ReactiveUI.SourceGenerators;

            namespace TestApp
            {
                [IViewFor("TestApp.Missing")] public partial class MissingView { }
                [IViewFor("")] public partial class EmptyView { }
            }
            """;

        var result = TestHelper.RunGenerator($"{Source}\n{SourceGeneratorsAttributes}", LanguageVersion.CSharp11);
        await result.DoesNotHaveGeneratedSource(DispatchHintName);
    }

    /// <summary>A System.Reactive consumer's views are registered, through that flavour's interface and attributes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveRuntime_RegistersViews()
    {
        const string Source = """
            using ReactiveUI.Binding.Reactive;

            namespace ReactiveApp
            {
                public class ReactiveDashboardViewModel { }

                public class ReactiveDashboardView : IViewFor<ReactiveDashboardViewModel>
                {
                    public ReactiveDashboardViewModel ViewModel { get; set; }

                    object IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (ReactiveDashboardViewModel)value;
                    }
                }

                [ViewContract("compact")]
                public class ReactiveCompactDashboardView : IViewFor<ReactiveDashboardViewModel>
                {
                    public ReactiveDashboardViewModel ViewModel { get; set; }

                    object IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (ReactiveDashboardViewModel)value;
                    }
                }

                public static class Usage
                {
                    public static bool Run()
                    {
                        var locator = new DefaultViewLocator();
                        return locator.ResolveView(new ReactiveDashboardViewModel(), null) is ReactiveDashboardView
                            && locator.ResolveView(new ReactiveDashboardViewModel(), "compact") is ReactiveCompactDashboardView;
                    }
                }
            }
            """;

        var result = TestHelper.RunGenerator(Source, LanguageVersion.CSharp10, null, true);
        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchHintName, "global::ReactiveUI.Binding.Reactive.DefaultViewLocator.SetGeneratedViewDispatch(");

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("ReactiveApp.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>An <c>[IViewFor]</c> view in a System.Reactive consumer is checked against that flavour's interface.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveRuntime_AttributeViewUsesThatFlavour()
    {
        const string Source = """
            using ReactiveUI.SourceGenerators;

            namespace ReactiveApp
            {
                public class ReactivePanelViewModel { }

                [IViewFor<ReactivePanelViewModel>] public partial class ReactivePanelView { }
            }
            """;

        var result = TestHelper.RunGenerator($"{Source}\n{SourceGeneratorsAttributes}", LanguageVersion.CSharp11, null, true);
        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchHintName, "(object)new global::ReactiveApp.ReactivePanelView() as global::ReactiveUI.Binding.Reactive.IViewFor");
    }

    /// <summary>A compilation that references neither runtime registers no views, whether declared by interface or attribute.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoRuntime_RegistersNothing()
    {
        const string Source = """
            using ReactiveUI.SourceGenerators;

            namespace TestApp
            {
                public class PanelViewModel { }

                public interface IPanel { }

                [IViewFor<PanelViewModel>] public partial class PanelView : IPanel { }
            }
            """;

        var compilation = TestHelper.CreateCompilation($"{Source}\n{SourceGeneratorsAttributes}", LanguageVersion.CSharp11);
        compilation = compilation.RemoveReferences(compilation.References.Where(static r => r.Display?.Contains("ReactiveUI.Binding", StringComparison.Ordinal) == true));
        var result = TestHelper.RunGenerator(compilation, LanguageVersion.CSharp11, null, true);
        await result.DoesNotHaveGeneratedSource(DispatchHintName);
    }

    /// <summary>Stands in for ReactiveUI.SourceGenerators: adds fixed source that no other generator in the run can see.</summary>
    /// <param name="source">The source to add.</param>
    private sealed class InterfaceAddingGenerator(string source) : IIncrementalGenerator
    {
        /// <inheritdoc/>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            context.RegisterSourceOutput(context.CompilationProvider, (output, _) => output.AddSource("AddedInterfaces.g.cs", source));
    }
}
