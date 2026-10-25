// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers the <c>ToProperty</c> overload shapes the scenario tests leave out: an intercepted call that hands its
/// helper back through <c>out</c>, a scheduler overload without nullable annotations, and call sites that disagree
/// on the nullable annotation of their value type.
/// </summary>
public class ToPropertyCoverageTests
{
    /// <summary>The dispatch file the ToProperty call sites are generated into.</summary>
    private const string DispatchFileName = "ToPropertyDispatch.g.cs";

    /// <summary>The root namespace the generator runs under.</summary>
    private const string RootNamespace = "CoverageProbe";

    /// <summary>The value type every group in these tests shares.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>A scheduler overload written for a compiler that has no nullable reference types.</summary>
    private const string SchedulerWithoutNullableSource = """
                                                          using System;
                                                          using System.ComponentModel;
                                                          using ReactiveUI.Binding;
                                                          using ReactiveUI.Primitives.Concurrency;

                                                          namespace CoverageProbe
                                                          {
                                                              public class ProbeViewModel : INotifyPropertyChanged
                                                              {
                                                                  public event PropertyChangedEventHandler PropertyChanged;

                                                                  public string Caption { get { return ""; } }

                                                                  internal void RaisePropertyChanged(string name)
                                                                  {
                                                                      var handler = PropertyChanged;
                                                                      if (handler != null)
                                                                      {
                                                                          handler(this, new PropertyChangedEventArgs(name));
                                                                      }
                                                                  }
                                                              }

                                                              public static class Scenario
                                                              {
                                                                  public static ObservableAsPropertyHelper<string> Create(IObservable<string> source, ProbeViewModel vm, ISequencer scheduler)
                                                                  {
                                                                      return source.ToProperty(vm, x => x.Caption, "(none)", true, scheduler);
                                                                  }
                                                              }
                                                          }
                                                          """;

    /// <summary>Two call sites in one group, one inferring <c>string</c> and one inferring <c>string?</c>.</summary>
    private const string DisagreeingAnnotationSource = """
                                                       #nullable enable
                                                       using System;
                                                       using System.ComponentModel;
                                                       using ReactiveUI.Binding;

                                                       namespace CoverageProbe
                                                       {
                                                           public class ProbeViewModel : INotifyPropertyChanged
                                                           {
                                                               public event PropertyChangedEventHandler? PropertyChanged;

                                                               public string Caption => "";

                                                               public string? Subtitle => null;

                                                               internal void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                                                           }

                                                           public static class Scenario
                                                           {
                                                               public static ObservableAsPropertyHelper<string> Caption(IObservable<string> source, ProbeViewModel vm) =>
                                                                   source.ToProperty(vm, x => x.Caption);

                                                               public static ObservableAsPropertyHelper<string?> Subtitle(IObservable<string?> source, ProbeViewModel vm) =>
                                                                   source.ToProperty(vm, x => x.Subtitle);
                                                           }
                                                       }
                                                       """;

    /// <summary>An intercepted call that hands its helper back through <c>out</c> forwards the worker's result.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OutResult_UnderInterception_ForwardsTheResult()
    {
        var source = SharedSourceReader.ReadScenario("ToProperty/FactoryOutResult");
        var parseOptions = TestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp11);
        var compilation = TestHelper.CreateCompilation(source, parseOptions, false, "TestAssembly", []);

        var result = TestHelper.RunGenerator(compilation, parseOptions, RootNamespace, true);

        await result.CompilationSucceeds();
        if (InterceptableLocationReader.IsSupported)
        {
            await result.GeneratedSourceContains(DispatchFileName, "result = __ToProperty_");
        }
    }

    /// <summary>Without nullable reference types the scheduler parameter is declared without an annotation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Scheduler_WithoutNullable_DeclaresAnUnannotatedScheduler()
    {
        var result = TestHelper.RunGenerator(
            SchedulerWithoutNullableSource,
            TestHelper.FallbackLanguageVersion(nullableEnabled: false),
            RootNamespace);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFileName, "ISequencer scheduler");
    }

    /// <summary>Call sites that disagree on the value type's nullable annotation still compile together.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DisagreeingAnnotations_ShareOneCompilingGroup()
    {
        var result = TestHelper.RunGenerator(DisagreeingAnnotationSource, LanguageVersion.CSharp10, RootNamespace);

        await result.CompilationSucceeds();
    }

    /// <summary>A group with no call sites declares its value type without an annotation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmptyGroup_DisplaysThePlainValueType()
    {
        var group = new ToPropertyCodeGenerator.ToPropertyTypeGroup(
            "global::CoverageProbe.ProbeViewModel",
            StringTypeName,
            new ToPropertyOverloadShape(false, false, ToPropertyInitialValueKind.None, false, false),
            []);

        await Assert.That(group.ValueTypeDisplay).IsEqualTo(StringTypeName);
    }
}
