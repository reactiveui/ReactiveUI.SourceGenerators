// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

extern alias analyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Covers RXUIBIND020, a view registered only in the service locator, which <c>ResolveView(object)</c> does not ask.</summary>
public class ServiceLocatorViewAnalyzerTests
{
    /// <summary>The diagnostic reported for a view registered only in the service locator.</summary>
    private const string ServiceLocatorOnlyView = "RXUIBIND020";

    /// <summary>Registrations the analyzer reports, and registrations a generated view, a mapping or the attribute covers.</summary>
    private const string Registrations = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.SourceGenerators;
        using Splat;

        namespace ReactiveUI.SourceGenerators
        {
            [AttributeUsage(AttributeTargets.Class)]
            internal sealed class IViewForAttribute<T> : Attribute
            {
            }
        }

        namespace TestApp
        {
            public class ExcludedViewModel { }
            public class RemoteViewModel { }
            public class SingletonViewModel { }
            public class ConstantViewModel { }
            public class OtherMapViewModel { }
            public class GeneratedViewModel { }
            public class MappedViewModel { }
            public class BuilderViewModel { }
            public class AttributeViewModel { }
            public class LookalikeViewModel { }

            public static class ViewMappingBuilder
            {
                public static void Map<T>() { }
            }

            public class ViewBase<T> : IViewFor<T>
                where T : class
            {
                public T? ViewModel { get; set; }

                object? IViewFor.ViewModel
                {
                    get => ViewModel;
                    set => ViewModel = (T?)value;
                }
            }

            [ExcludeFromViewRegistration] public class ExcludedView : ViewBase<ExcludedViewModel> { }
            [ExcludeFromViewRegistration] public class ConstantView : ViewBase<ConstantViewModel> { }
            [ExcludeFromViewRegistration] public class MappedView : ViewBase<MappedViewModel> { }
            [ExcludeFromViewRegistration] public class BuilderView : ViewBase<BuilderViewModel> { }
            public abstract class RemoteView : ViewBase<RemoteViewModel> { }
            public class GeneratedView : ViewBase<GeneratedViewModel> { }
            [IViewFor<AttributeViewModel>] public partial class AttributeView { }
            public struct NotAView { }

            public static class Other
            {
                public static void Register<T>(Func<T> factory) { }

                public static void Map<T>() { }
            }

            public static class Registrations
            {
                public static void Register(DefaultViewLocator locator)
                {
                    var services = AppLocator.CurrentMutable;
                    services.Register<IViewFor<ExcludedViewModel>>(static () => new ExcludedView());
                    services.Register(static () => null, typeof(IViewFor<RemoteViewModel>));
                    services.RegisterLazySingleton<IViewFor<SingletonViewModel>>(static () => null);
                    services.RegisterConstant<IViewFor<ConstantViewModel>>(new ConstantView());
                    Other.Map<OtherMapViewModel>();
                    services.Register<IViewFor<OtherMapViewModel>>(static () => null);
                    ViewMappingBuilder.Map<LookalikeViewModel>();
                    services.Register<IViewFor<LookalikeViewModel>>(static () => null);

                    services.Register<IViewFor<GeneratedViewModel>>(static () => new GeneratedView());
                    locator.Map<MappedViewModel, MappedView>();
                    services.Register<IViewFor<MappedViewModel>>(static () => new MappedView());
                    locator.CreateMappingBuilder().Map<BuilderViewModel, BuilderView>();
                    services.Register<IViewFor<BuilderViewModel>>(static () => new BuilderView());
                    services.Register<IViewFor<AttributeViewModel>>(static () => null);
                    services.Register<string>(static () => string.Empty);
                    services.Register(static () => string.Empty, typeof(string));
                    Other.Register<IViewFor<ExcludedViewModel>>(static () => null!);
                }
            }
        }
        """;

    /// <summary>
    /// A registration is reported when the project neither generates a view for its view model nor maps one, whichever
    /// registration method or form names it. A generated view, a mapping from either mapping route, or ReactiveUI.SourceGenerators'
    /// attribute each cover the view model; an excluded or abstract view does not; other methods named like these are ignored.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReportsRegistrationsNothingElseCovers()
    {
        var diagnostics = await AnalyzeAsync(Registrations, CodeFixRunner.Runtime.Lean);
        var reported = diagnostics.Select(static d => $"{d.Id} {d.GetMessage().Split('<', '>')[1]}").ToArray();

        await Assert.That(reported).IsEquivalentTo(
            [
                $"{ServiceLocatorOnlyView} ExcludedViewModel",
                $"{ServiceLocatorOnlyView} RemoteViewModel",
                $"{ServiceLocatorOnlyView} SingletonViewModel",
                $"{ServiceLocatorOnlyView} ConstantViewModel",
                $"{ServiceLocatorOnlyView} OtherMapViewModel",
                $"{ServiceLocatorOnlyView} LookalikeViewModel",
            ]);
    }

    /// <summary>A System.Reactive project's registration is reported, and a project with neither runtime is not analysed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RecognisesEitherFlavour()
    {
        const string Reactive = """
            using ReactiveUI.Binding.Reactive;
            using Splat;

            namespace TestApp
            {
                public class ReactiveViewModel { }
                public class ReactiveMappedViewModel { }

                [ExcludeFromViewRegistration]
                public class ReactiveMappedView : IViewFor<ReactiveMappedViewModel>
                {
                    public ReactiveMappedViewModel? ViewModel { get; set; }

                    object? IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (ReactiveMappedViewModel?)value;
                    }
                }

                public static class Registrations
                {
                    public static void Register(DefaultViewLocator locator)
                    {
                        AppLocator.CurrentMutable.Register<IViewFor<ReactiveViewModel>>(static () => null);
                        locator.Map<ReactiveMappedViewModel, ReactiveMappedView>();
                        AppLocator.CurrentMutable.Register<IViewFor<ReactiveMappedViewModel>>(static () => new ReactiveMappedView());
                    }
                }
            }
            """;

        var reactive = await AnalyzeAsync(Reactive, CodeFixRunner.Runtime.Reactive);
        var none = await AnalyzeAsync("namespace TestApp { public class Plain { } }", CodeFixRunner.Runtime.None);

        await Assert.That(reactive.Select(static d => d.Id)).IsEquivalentTo([ServiceLocatorOnlyView]);
        await Assert.That(none).IsEmpty();
    }

    /// <summary>Runs the analyzer over a source.</summary>
    /// <param name="source">The source.</param>
    /// <param name="runtime">The runtime package the project references.</param>
    /// <returns>The diagnostics, in source order.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, CodeFixRunner.Runtime runtime) =>
        CodeFixRunner.AnalyzeAsync(source, LanguageVersion.CSharp12, new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.ServiceLocatorViewAnalyzer(), runtime);
}
