// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Tests the tier that claims binding call sites with interceptors rather than overloads.</summary>
public class InterceptedCallSiteTests
{
    /// <summary>The attribute text an interceptor carries.</summary>
    private const string InterceptsAttribute = "InterceptsLocation(";

    /// <summary>The declaration text a dispatch overload carries.</summary>
    private const string OverloadDeclaration = "public static global::System.IObservable<";

    /// <summary>The dispatch file these tests read.</summary>
    private const string DispatchFileName = "WhenChangedDispatch.g.cs";

    /// <summary>The root namespace the scenarios build under.</summary>
    private const string RootNamespace = "TestApp";

    /// <summary>The dispatch file a BindTo call site is claimed in.</summary>
    private const string BindToDispatchFileName = "BindToDispatch.g.cs";

    /// <summary>The dispatch file an InvokeCommand call site is claimed in.</summary>
    private const string InvokeCommandDispatchFileName = "InvokeCommandDispatch.g.cs";

    /// <summary>The type the scenario exposes its binding through.</summary>
    private const string UsageTypeName = $"{RootNamespace}.Usage";

    /// <summary>A binding whose value can be read back out of the emitted assembly.</summary>
    private const string Scenario = """
                                    using System;
                                    using System.ComponentModel;
                                    using ReactiveUI.Binding;

                                    namespace TestApp
                                    {
                                        public class MyViewModel : INotifyPropertyChanged
                                        {
                                            private string _name = "start";

                                            public event PropertyChangedEventHandler PropertyChanged;

                                            public string Name
                                            {
                                                get { return _name; }
                                                set
                                                {
                                                    _name = value;
                                                    var handler = PropertyChanged;
                                                    if (handler != null)
                                                    {
                                                        handler(this, new PropertyChangedEventArgs("Name"));
                                                    }
                                                }
                                            }
                                        }

                                        public static class Usage
                                        {
                                            public static string Observe()
                                            {
                                                var viewModel = new MyViewModel();
                                                string seen = null;
                                                var subscription = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(
                                                    viewModel.WhenChanged(x => x.Name),
                                                    delegate(string value) { seen = value; });
                                                viewModel.Name = "changed";
                                                subscription.Dispose();
                                                return seen;
                                            }
                                        }
                                    }
                                    """;

    /// <summary>The same binding written in a namespace no root namespace encloses.</summary>
    private const string OutOfReachScenario = """
                                              using System;
                                              using System.ComponentModel;
                                              using ReactiveUI.Binding;

                                              namespace Elsewhere
                                              {
                                                  public class MyViewModel : INotifyPropertyChanged
                                                  {
                                                      public event PropertyChangedEventHandler PropertyChanged;

                                                      public string Name { get; set; }
                                                  }

                                                  public class Usage
                                                  {
                                                      public void Bind()
                                                      {
                                                          var viewModel = new MyViewModel();
                                                          var observable = viewModel.WhenChanged(x => x.Name);
                                                          GC.KeepAlive(observable);
                                                      }
                                                  }
                                              }
                                              """;

    /// <summary>One call site per generated API, so every emitter's tier is settled by the same build.</summary>
    private const string EveryApiScenario = """
                                            using System;
                                            using System.ComponentModel;
                                            using System.Threading.Tasks;
                                            using System.Windows.Input;
                                            using ReactiveUI.Binding;

                                            namespace TestApp
                                            {
                                                public class Person : INotifyPropertyChanged, INotifyPropertyChanging
                                                {
                                                    public event PropertyChangedEventHandler PropertyChanged;

                                                    public event PropertyChangingEventHandler PropertyChanging;

                                                    public string Name { get; set; }

                                                    public int Age { get; set; }

                                                    public ICommand Save { get; set; }

                                                    public IInteraction<string, bool> Confirm { get; set; }

                                                    public IObservable<int> Ticks { get; set; }

                                                    public IObservable<int> Pulses { get; set; }
                                                }

                                                public class Button
                                                {
                                                    public event EventHandler Click;

                                                    public string Text { get; set; }
                                                }

                                                public class PersonView : INotifyPropertyChanged, IViewFor<Person>
                                                {
                                                    public event PropertyChangedEventHandler PropertyChanged;

                                                    public Person ViewModel { get; set; }

                                                    object IViewFor.ViewModel
                                                    {
                                                        get { return ViewModel; }
                                                        set { ViewModel = (Person)value; }
                                                    }

                                                    public Button SaveButton { get; set; }

                                                    public string Display { get; set; }

                                                    public string Summary { get; set; }
                                                }

                                                public class Usage
                                                {
                                                    public void Bind(Person person, PersonView view, IObservable<string> names)
                                                    {
                                                        GC.KeepAlive(person.WhenChanged(x => x.Name));
                                                        GC.KeepAlive(person.WhenChanging(x => x.Name));
                                                        GC.KeepAlive(person.WhenAnyValue(x => x.Age));
                                                        GC.KeepAlive(person.WhenAny(x => x.Name, c => c.Value));
                                                        GC.KeepAlive(person.WhenAnyObservable(x => x.Ticks));
                                                        GC.KeepAlive(person.WhenAnyObservable(x => x.Ticks, x => x.Pulses, (a, b) => a + b));

                                                        person.BindOneWay(view, x => x.Name, v => v.Display);
                                                        person.BindTwoWay(view, x => x.Name, v => v.Display);
                                                        view.OneWayBind(person, x => x.Name, v => v.Summary);
                                                        view.Bind(person, x => x.Name, v => v.Display);
                                                        names.BindTo(view, v => v.Display);
                                                        names.InvokeCommand(person, x => x.Save);
                                                        view.BindCommand(person, x => x.Save, v => v.SaveButton);
                                                        view.BindCommand(person, x => x.Save, v => v.SaveButton, names);
                                                        view.BindInteraction(person, x => x.Confirm, Handle);
                                                    }

                                                    private static Task Handle(IInteractionContext<string, bool> context)
                                                    {
                                                        context.SetOutput(true);
                                                        return Task.CompletedTask;
                                                    }
                                                }
                                            }
                                            """;

    /// <summary>Call sites whose selectors name no property path, so nothing is generated and they reach the stub.</summary>
    private const string RuntimeResolvedScenario = """
                                                   using System;
                                                   using System.ComponentModel;
                                                   using System.Windows.Input;
                                                   using ReactiveUI.Binding;

                                                   namespace TestApp
                                                   {
                                                       public class Person : INotifyPropertyChanged
                                                       {
                                                           public event PropertyChangedEventHandler PropertyChanged;

                                                           public ICommand Save { get; set; }

                                                           public ICommand Resolve()
                                                           {
                                                               return Save;
                                                           }
                                                       }

                                                       public class PersonView : INotifyPropertyChanged
                                                       {
                                                           public event PropertyChangedEventHandler PropertyChanged;

                                                           public string Display { get; set; }

                                                           public string Target()
                                                           {
                                                               return Display;
                                                           }
                                                       }

                                                       public class Usage
                                                       {
                                                           public void Bind(Person person, PersonView view, IObservable<string> names)
                                                           {
                                                               names.BindTo(view, v => v.Target());
                                                               names.InvokeCommand(person, x => x.Resolve());
                                                           }
                                                       }
                                                   }
                                                   """;

    /// <summary>Observations that combine a path generated code can read with one through a private property.</summary>
    private const string PrivatePathScenario = """
                                               using System;
                                               using System.Collections.Generic;
                                               using System.ComponentModel;
                                               using ReactiveUI.Binding;

                                               namespace TestApp
                                               {
                                                   public class ItemsViewModel : INotifyPropertyChanged
                                                   {
                                                       public event PropertyChangedEventHandler PropertyChanged;

                                                       public string FilterText { get; set; }

                                                       public IObservable<string> Filters { get; set; }

                                                       private IReadOnlyList<string> AllItems { get; set; }

                                                       private IObservable<string> Hidden { get; set; }

                                                       public void Observe()
                                                       {
                                                           var counts = this.WhenAnyValue(x => x.AllItems, x => x.FilterText, (items, filter) => items.Count);
                                                           var latest = this.WhenAnyObservable(x => x.Hidden, x => x.Filters);
                                                           GC.KeepAlive(counts);
                                                           GC.KeepAlive(latest);
                                                       }
                                                   }
                                               }
                                               """;

    /// <summary>The dispatch file each generated binding API emits, excluding the view locator's, which claims no call site.</summary>
    private static readonly string[] ApiDispatchFiles =
    [
        "WhenChangedDispatch.g.cs",
        "WhenChangingDispatch.g.cs",
        "WhenAnyValueDispatch.g.cs",
        "WhenAnyDispatch.g.cs",
        "WhenAnyObservableDispatch.g.cs",
        "BindOneWayDispatch.g.cs",
        "BindTwoWayDispatch.g.cs",
        "OneWayBindDispatch.g.cs",
        "BindDispatch.g.cs",
        "BindToDispatch.g.cs",
        "BindCommandDispatch.g.cs",
        "BindInteractionDispatch.g.cs",
        "InvokeCommandDispatch.g.cs",
    ];

    /// <summary>Every generated API is claimed the same way, so each emitter's tier follows one decision.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_ClaimsEveryGeneratedApi()
    {
        var result = Generate(EveryApiScenario, LanguageVersion.CSharp11, optIn: true, RootNamespace);

        await Assert.That(result.CompilationErrors).IsEmpty();

        foreach (var file in ApiDispatchFiles)
        {
            await Assert.That(result.GeneratedSources.ContainsKey(file)).IsTrue();

            await Assert.That(result.GeneratedSources[file].Contains(InterceptsAttribute, StringComparison.Ordinal))
                .IsEqualTo(InterceptableLocationReader.IsSupported);
        }
    }

    /// <summary>Emission checks an interceptor against the call it replaces, so every API is emitted here.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_EmitsWithEveryApiClaimed()
    {
        var result = Generate(EveryApiScenario, LanguageVersion.CSharp11, optIn: true, RootNamespace);

        var (_, context) = TestHelper.EmitAndLoad(result);
        context.Unload();

        await Assert.That(result.CompilationErrors).IsEmpty();
    }

    /// <summary>A build listing the generated namespace gets the tier its compiler can honour.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_EmitsInterceptorsWhereTheCompilerCanDescribeACallSite()
    {
        var dispatch = GenerateDispatch(Scenario, LanguageVersion.CSharp11, optIn: true);

        await Assert.That(dispatch.Contains(InterceptsAttribute, StringComparison.Ordinal))
            .IsEqualTo(InterceptableLocationReader.IsSupported);
        await Assert.That(dispatch.Contains(OverloadDeclaration, StringComparison.Ordinal))
            .IsEqualTo(!InterceptableLocationReader.IsSupported);
    }

    /// <summary>The opt-in is what turns the tier on; without it the overloads are emitted either way.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildWithoutTheOptIn_EmitsTheDispatchOverload()
    {
        var dispatch = GenerateDispatch(Scenario, LanguageVersion.CSharp10, optIn: false);

        await Assert.That(dispatch).DoesNotContain(InterceptsAttribute);
        await Assert.That(dispatch).Contains(OverloadDeclaration);
    }

    /// <summary>
    /// A project below C# 11 cannot declare the file-local interception attribute, so an opted-in build dispatches
    /// through the overloads instead.
    /// </summary>
    /// <param name="languageVersion">The consumer's language version.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(LanguageVersion.CSharp7_3)]
    [Arguments(LanguageVersion.CSharp10)]
    public async Task OptedInBuild_BelowCSharp11_DispatchesThroughTheOverloads(LanguageVersion languageVersion)
    {
        var dispatch = GenerateDispatch(Scenario, languageVersion, optIn: true);

        await Assert.That(dispatch).DoesNotContain(InterceptsAttribute);
        await Assert.That(dispatch).Contains(OverloadDeclaration);
    }

    /// <summary>A file declared outside the root namespace is claimed although the overloads cannot reach it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_ClaimsACallSiteOutsideTheRootNamespace()
    {
        var dispatch = GenerateDispatch(OutOfReachScenario, LanguageVersion.CSharp11, optIn: true);

        await Assert.That(dispatch.Contains(InterceptsAttribute, StringComparison.Ordinal))
            .IsEqualTo(InterceptableLocationReader.IsSupported);
    }

    /// <summary>The emitted assembly runs the binding an interceptor claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_RunsTheBindingItClaimed()
    {
        var result = Generate(Scenario, LanguageVersion.CSharp11, optIn: true, RootNamespace);

        await Assert.That(result.CompilationErrors).IsEmpty();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var observe = assembly.GetType(UsageTypeName)?.GetMethod(
            "Observe",
            BindingFlags.Public | BindingFlags.Static);

        await Assert.That(observe).IsNotNull();
        await Assert.That(observe!.Invoke(null, null)).IsEqualTo("changed");

        context.Unload();
    }

    /// <summary>The same binding runs when the overloads are what the build got.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildWithoutTheOptIn_RunsTheBindingItDispatched()
    {
        var result = Generate(Scenario, LanguageVersion.CSharp10, optIn: false, RootNamespace);

        await Assert.That(result.CompilationErrors).IsEmpty();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var observe = assembly.GetType(UsageTypeName)?.GetMethod(
            "Observe",
            BindingFlags.Public | BindingFlags.Static);

        await Assert.That(observe).IsNotNull();
        await Assert.That(observe!.Invoke(null, null)).IsEqualTo("changed");

        context.Unload();
    }

    /// <summary>A call site whose selector cannot be read is left to the stub, even in a build that claims call sites.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_LeavesACallSiteItCannotReadToTheStub()
    {
        var result = Generate(RuntimeResolvedScenario, LanguageVersion.CSharp11, optIn: true, RootNamespace);

        await Assert.That(result.CompilationErrors).IsEmpty();
        await result.DoesNotHaveGeneratedSource(BindToDispatchFileName);
        await result.DoesNotHaveGeneratedSource(InvokeCommandDispatchFileName);
    }

    /// <summary>
    /// A call site with one path through a private property is left to the stub whole. Claiming it with the paths
    /// that remain would declare fewer parameters than the call, which the compiler rejects.
    /// </summary>
    /// <param name="languageVersion">The consumer's language version.</param>
    /// <param name="optIn">Whether the build lists the generated namespace for interception.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(LanguageVersion.CSharp11, true)]
    [Arguments(LanguageVersion.CSharp10, false)]
    public async Task CallSiteWithAPrivatePath_IsLeftToTheStub(LanguageVersion languageVersion, bool optIn)
    {
        var result = Generate(PrivatePathScenario, languageVersion, optIn, RootNamespace);

        // The compiler checks an interceptor's signature against its call only when it emits.
        var emitErrors = result.OutputCompilation.Emit(Stream.Null).Diagnostics
            .Where(static d => d.Severity == DiagnosticSeverity.Error)
            .Select(static d => d.Id)
            .ToArray();

        await Assert.That(result.CompilationErrors).IsEmpty();
        await Assert.That(emitErrors).IsEmpty();
        await result.DoesNotHaveGeneratedSource("WhenAnyValueDispatch.g.cs");
        await result.DoesNotHaveGeneratedSource("WhenAnyObservableDispatch.g.cs");
    }

    /// <summary>The generated namespace sits below the one the opt-in names, and belongs to this assembly alone.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_GeneratesIntoTheInterceptedNamespace()
    {
        var dispatch = GenerateDispatch(Scenario, LanguageVersion.CSharp11, optIn: true);

        await Assert.That(dispatch.Contains($"namespace {Constants.InterceptorNamespace}.TestAssembly", StringComparison.Ordinal))
            .IsEqualTo(InterceptableLocationReader.IsSupported);
    }

    /// <summary>A file that intercepts declares its own file-local interception attribute.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_DeclaresTheInterceptionAttributeInTheInterceptingFile()
    {
        var dispatch = GenerateDispatch(Scenario, LanguageVersion.CSharp11, optIn: true);

        await Assert.That(dispatch.Contains("file sealed class InterceptsLocationAttribute", StringComparison.Ordinal))
            .IsEqualTo(InterceptableLocationReader.IsSupported);
    }

    /// <summary>Runs the generator and returns the dispatch file it produced.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="languageVersion">The language version the consumer builds at.</param>
    /// <param name="optIn">Whether the build lists the generated namespace for interception.</param>
    /// <param name="rootNamespace">The root namespace the build exposes.</param>
    /// <returns>The generated dispatch file.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GenerateDispatch(
        string source,
        LanguageVersion languageVersion,
        bool optIn,
        string? rootNamespace = RootNamespace) =>
        Generate(source, languageVersion, optIn, rootNamespace).GeneratedSources[DispatchFileName];

    /// <summary>Runs the generator over a compilation parsed the way the build in question parses.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="languageVersion">The language version the consumer builds at.</param>
    /// <param name="optIn">Whether the build lists the generated namespace for interception.</param>
    /// <param name="rootNamespace">The root namespace the build exposes.</param>
    /// <returns>The generator result.</returns>
    private static GeneratorTestResult Generate(
        string source,
        LanguageVersion languageVersion,
        bool optIn,
        string? rootNamespace)
    {
        var parseOptions = optIn
            ? TestHelper.InterceptingParseOptionsFor(languageVersion)
            : TestHelper.ParseOptionsFor(languageVersion);

        var compilation = TestHelper.CreateCompilation(source, parseOptions, false, "TestAssembly", []);

        return TestHelper.RunGenerator(compilation, parseOptions, rootNamespace, true);
    }
}
