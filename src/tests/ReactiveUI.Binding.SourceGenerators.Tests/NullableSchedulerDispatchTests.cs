// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// The scheduler the runtime stubs take is nullable, and null leaves the write to the thread that owns the
/// target. A generated overload or interceptor declares it the same way wherever the target supports nullable
/// reference types, so an explicit null argument compiles without a warning.
/// </summary>
public class NullableSchedulerDispatchTests
{
    /// <summary>The hint name of the BindOneWay dispatch file.</summary>
    private const string BindOneWayHint = "BindOneWayDispatch.g.cs";

    /// <summary>The hint name of the BindTwoWay dispatch file.</summary>
    private const string BindTwoWayHint = "BindTwoWayDispatch.g.cs";

    /// <summary>The hint name of the OneWayBind dispatch file.</summary>
    private const string OneWayBindHint = "OneWayBindDispatch.g.cs";

    /// <summary>The hint name of the Bind dispatch file.</summary>
    private const string BindHint = "BindDispatch.g.cs";

    /// <summary>The warning a compilation reports when a System.Reactive reference was built against an older framework.</summary>
    private const string AssemblyIdentityWarning = "CS1701";

    /// <summary>The scheduler type the lean runtime declares.</summary>
    private const string LeanScheduler = "ISequencer";

    /// <summary>The scheduler type the System.Reactive runtime declares.</summary>
    private const string ReactiveScheduler = "IScheduler";

    /// <summary>The namespace that holds the lean scheduler type.</summary>
    private const string LeanSchedulerNamespace = "ReactiveUI.Primitives.Concurrency";

    /// <summary>The namespace that holds the System.Reactive scheduler type.</summary>
    private const string ReactiveSchedulerNamespace = "System.Reactive.Concurrency";

    /// <summary>The generated scheduler parameter of a lean consumer that supports nullable reference types.</summary>
    private const string NullableLeanParameter = "global::ReactiveUI.Primitives.Concurrency.ISequencer? scheduler";

    /// <summary>The generated scheduler parameter of a lean consumer that predates nullable reference types.</summary>
    private const string ObliviousLeanParameter = "global::ReactiveUI.Primitives.Concurrency.ISequencer scheduler";

    /// <summary>The generated scheduler parameter of a System.Reactive consumer that supports nullable reference types.</summary>
    private const string NullableReactiveParameter = "global::System.Reactive.Concurrency.IScheduler? scheduler";

    /// <summary>Every scheduler-taking property-binding overload, called with an explicit null scheduler.</summary>
    private const string NullableSource = """
        #nullable enable
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;
        using SCHEDULER_NAMESPACE;

        namespace NullableSchedulerProbe
        {
            public class ProbeViewModel : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public string Name { get; set; } = "";

                public string? Nickname { get; set; }

                protected void Raise(string name)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
            }

            public class ProbeView : INotifyPropertyChanged, IViewFor
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public object? ViewModel { get; set; }

                public string DisplayName { get; set; } = "";

                public string? DisplayNickname { get; set; }

                protected void Raise(string name)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
            }

            public class ProbeHost
            {
                private readonly ProbeViewModel _viewModel = new ProbeViewModel();

                private readonly ProbeView _view = new ProbeView();

                public IDisposable OneWay()
                {
                    return _viewModel.BindOneWay(_view, x => x.Name, x => x.DisplayName, (SCHEDULER?)null);
                }

                public IDisposable OneWayNullableProperties()
                {
                    return _viewModel.BindOneWay(_view, x => x.Nickname, x => x.DisplayNickname, (SCHEDULER?)null);
                }

                public IDisposable OneWayConverted()
                {
                    Func<string, string> convert = name => name + "!";
                    return _viewModel.BindOneWay(_view, x => x.Name, x => x.DisplayName, convert, (SCHEDULER?)null);
                }

                public IDisposable TwoWay()
                {
                    return _viewModel.BindTwoWay(_view, x => x.Name, x => x.DisplayName, (SCHEDULER?)null);
                }

                public IDisposable TwoWayConverted()
                {
                    Func<string, string> forward = name => name + "!";
                    Func<string, string> reverse = text => text;
                    return _viewModel.BindTwoWay(_view, x => x.Name, x => x.DisplayName, forward, reverse, (SCHEDULER?)null);
                }

                public IDisposable OneWayBind()
                {
                    Func<string, string> select = name => name + "!";
                    return _view.OneWayBind(_viewModel, x => x.Name, v => v.DisplayName, select, (SCHEDULER?)null);
                }

                public IDisposable Bind()
                {
                    Func<string, string> forward = name => name + "!";
                    Func<string, string> reverse = text => text;
                    return _view.Bind(_viewModel, x => x.Name, v => v.DisplayName, forward, reverse, (SCHEDULER?)null);
                }
            }
        }
        """;

    /// <summary>The same call sites in a consumer that predates nullable reference types.</summary>
    private const string ObliviousSource = """
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;
        using ReactiveUI.Primitives.Concurrency;

        namespace NullableSchedulerProbe
        {
            public class ProbeViewModel : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;

                public string Name { get; set; }
            }

            public class ProbeView : INotifyPropertyChanged, IViewFor
            {
                public event PropertyChangedEventHandler PropertyChanged;

                public object ViewModel { get; set; }

                public string DisplayName { get; set; }
            }

            public class ProbeHost
            {
                private readonly ProbeViewModel _viewModel = new ProbeViewModel();

                private readonly ProbeView _view = new ProbeView();

                public IDisposable OneWay()
                {
                    ISequencer scheduler = null;
                    return _viewModel.BindOneWay(_view, x => x.Name, x => x.DisplayName, scheduler);
                }

                public IDisposable TwoWay()
                {
                    ISequencer scheduler = null;
                    return _viewModel.BindTwoWay(_view, x => x.Name, x => x.DisplayName, scheduler);
                }
            }
        }
        """;

    /// <summary>Every scheduler-taking overload declares the scheduler nullable, and the explicit null compiles clean.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SchedulerOverloads_UnderNullableReferenceTypes_DeclareTheSchedulerNullable()
    {
        var result = await TestHelper.TestPassWithResult(
            NullableSourceFor(LeanScheduler, LeanSchedulerNamespace),
            typeof(NullableSchedulerDispatchTests),
            LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoCompilationWarnings();
        await result.GeneratedSourceContains(BindOneWayHint, NullableLeanParameter);
        await result.GeneratedSourceContains(BindTwoWayHint, NullableLeanParameter);
        await result.GeneratedSourceContains(OneWayBindHint, NullableLeanParameter);
        await result.GeneratedSourceContains(BindHint, NullableLeanParameter);
    }

    /// <summary>The System.Reactive flavour declares its scheduler nullable in the same places.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SchedulerOverloads_OnTheReactiveRuntime_DeclareTheSchedulerNullable()
    {
        var result = TestHelper.RunGenerator(
            NullableSourceFor(ReactiveScheduler, ReactiveSchedulerNamespace),
            LanguageVersion.CSharp10,
            null,
            true);

        await result.CompilationSucceeds();
        await Assert.That(result.CompilationWarnings.Where(static d => d.Id != AssemblyIdentityWarning)).IsEmpty();
        await result.GeneratedSourceContains(BindOneWayHint, NullableReactiveParameter);
        await result.GeneratedSourceContains(BindTwoWayHint, NullableReactiveParameter);
        await result.GeneratedSourceContains(OneWayBindHint, NullableReactiveParameter);
        await result.GeneratedSourceContains(BindHint, NullableReactiveParameter);
    }

    /// <summary>A build that opts in to interception declares the scheduler nullable, whichever form claims the call site.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SchedulerDispatch_InAnInterceptionBuild_DeclaresTheSchedulerNullable()
    {
        var parseOptions = TestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp11);
        var compilation = TestHelper.CreateCompilation(
            NullableSourceFor(LeanScheduler, LeanSchedulerNamespace),
            parseOptions,
            false,
            "TestAssembly",
            []);

        var result = TestHelper.RunGenerator(compilation, parseOptions, null, true);

        await result.CompilationSucceeds();
        await result.HasNoCompilationWarnings();
        await result.GeneratedSourceContains(BindOneWayHint, NullableLeanParameter);
    }

    /// <summary>A consumer that predates nullable reference types gets the scheduler with no annotation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SchedulerOverloads_BelowNullableReferenceTypes_DeclareNoAnnotation()
    {
        var result = TestHelper.RunGenerator(ObliviousSource, LanguageVersion.CSharp7_3);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(BindOneWayHint, ObliviousLeanParameter);
        await result.GeneratedSourceContains(BindTwoWayHint, ObliviousLeanParameter);
        await result.GeneratedSourceDoesNotContain(BindOneWayHint, "ISequencer?");
        await result.GeneratedSourceDoesNotContain(BindTwoWayHint, "ISequencer?");
    }

    /// <summary>Writes the nullable scenario against one of the two runtime packages' scheduler types.</summary>
    /// <param name="scheduler">The scheduler type name.</param>
    /// <param name="schedulerNamespace">The namespace that holds the scheduler type.</param>
    /// <returns>The scenario source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string NullableSourceFor(string scheduler, string schedulerNamespace) =>
        NullableSource
            .Replace("using ReactiveUI.Binding;", scheduler == ReactiveScheduler ? "using ReactiveUI.Binding.Reactive;" : "using ReactiveUI.Binding;", StringComparison.Ordinal)
            .Replace("SCHEDULER_NAMESPACE", schedulerNamespace, StringComparison.Ordinal)
            .Replace("SCHEDULER", scheduler, StringComparison.Ordinal);
}
