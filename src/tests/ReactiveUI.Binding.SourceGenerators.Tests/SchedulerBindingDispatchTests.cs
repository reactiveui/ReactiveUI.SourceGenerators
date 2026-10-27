// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Binding call sites that name their own scheduler reach the runtime library through
/// <c>ReactiveSchedulerExtensions</c>, which declares its whole surface as extension blocks rather than
/// classic extension methods. The declaring class of an extension block member is a level further out
/// than it is for a classic one, so these scenarios pin that such a call site is still recognised and
/// still generates a scheduler-carrying overload.
/// </summary>
public class SchedulerBindingDispatchTests
{
    /// <summary>The comment the emitters write into the body of a binding that carries a scheduler.</summary>
    private const string SchedulerMarker = "(with scheduler)";

    /// <summary>A source and a target, each bound both with and without an explicit scheduler.</summary>
    private const string SchedulerBindingSource = """
                                                  using System;
                                                  using System.ComponentModel;
                                                  using ReactiveUI.Binding;
                                                  using ReactiveUI.Primitives.Concurrency;

                                                  namespace SchedulerProbe
                                                  {
                                                      public class ProbeViewModel : INotifyPropertyChanged
                                                      {
                                                          public event PropertyChangedEventHandler PropertyChanged;

                                                          public string Name { get; set; }
                                                      }

                                                      public class ProbeView : INotifyPropertyChanged
                                                      {
                                                          public event PropertyChangedEventHandler PropertyChanged;

                                                          public string DisplayName { get; set; }
                                                      }

                                                      public class ProbeHost
                                                      {
                                                          private ProbeViewModel _source = new ProbeViewModel();

                                                          private ProbeView _target = new ProbeView();

                                                          public IDisposable OneWayPlain()
                                                          {
                                                              return _source.BindOneWay(_target, x => x.Name, x => x.DisplayName);
                                                          }

                                                          public IDisposable OneWayScheduled()
                                                          {
                                                              return _source.BindOneWay(_target, x => x.Name, x => x.DisplayName, ImmediateSequencer.Instance);
                                                          }

                                                          public IDisposable TwoWayScheduled()
                                                          {
                                                              return _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName, ImmediateSequencer.Instance);
                                                          }
                                                      }
                                                  }
                                                  """;

    /// <summary>A BindOneWay call site that supplies a scheduler generates a scheduler-carrying body.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithAScheduler_GeneratesASchedulerCarryingOverload()
    {
        var result = TestHelper.RunGenerator(
            SchedulerBindingSource,
            LanguageVersion.CSharp14,
            "SchedulerProbe");

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains("BindOneWayDispatch.g.cs", SchedulerMarker);
    }

    /// <summary>A BindTwoWay call site that supplies a scheduler generates a scheduler-carrying body.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_WithAScheduler_GeneratesASchedulerCarryingOverload()
    {
        var result = TestHelper.RunGenerator(
            SchedulerBindingSource,
            LanguageVersion.CSharp14,
            "SchedulerProbe");

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains("BindTwoWayDispatch.g.cs", SchedulerMarker);
    }
}
