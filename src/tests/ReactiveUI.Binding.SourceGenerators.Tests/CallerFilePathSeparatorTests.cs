// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// File-and-line dispatch matches a call site the way the compiler reports it. The compiler hands
/// <c>CallerFilePath</c> the path as it saw it, so a Windows build passes backslashes where the generator writes
/// its suffix with a forward slash. It hands <c>CallerLineNumber</c> the line of the invoked member's name, which
/// for a chained call written across lines is not the line the invocation starts on.
/// </summary>
public class CallerFilePathSeparatorTests
{
    /// <summary>A Windows path, as a Windows build passes it to <c>CallerFilePath</c>.</summary>
    private const string WindowsPath = @"C:\src\App\Views\MainView.cs";

    /// <summary>A chained call written across lines, with a runner that checks the binding delivers.</summary>
    private const string ChainedSource = """
                                         using System;
                                         using System.ComponentModel;
                                         using ReactiveUI.Binding;

                                         namespace TestApp
                                         {
                                             public class SourceModel : INotifyPropertyChanged
                                             {
                                                 private string _name;

                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 public string Name
                                                 {
                                                     get { return _name; }
                                                     set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
                                                 }
                                             }

                                             public class SummaryModel : INotifyPropertyChanged
                                             {
                                                 private readonly ObservableAsPropertyHelper<string> _label;

                                                 public SummaryModel(SourceModel source)
                                                 {
                                                     _label = source
                                                         .WhenChanged(x => x.Name)
                                                         .ToProperty(this, x => x.Label);
                                                 }

                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 public string Label => _label.Value;

                                                 public void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                                             }
                                         }

                                         public static class Usage
                                         {
                                             public static bool Run()
                                             {
                                                 var source = new TestApp.SourceModel { Name = "first" };
                                                 var summary = new TestApp.SummaryModel(source);
                                                 if (summary.Label != "first")
                                                 {
                                                     return false;
                                                 }

                                                 source.Name = "second";
                                                 return summary.Label == "second";
                                             }
                                         }
                                         """;

    /// <summary>One call site per emitter that writes a file-and-line condition.</summary>
    private const string Source = """
                                  using System;
                                  using System.ComponentModel;
                                  using ReactiveUI.Binding;

                                  namespace TestApp
                                  {
                                      public class SourceModel : INotifyPropertyChanged
                                      {
                                          public event PropertyChangedEventHandler PropertyChanged;

                                          public string Name { get; set; }
                                      }

                                      public class TargetModel : INotifyPropertyChanged
                                      {
                                          public event PropertyChangedEventHandler PropertyChanged;

                                          public string Title { get; set; }
                                      }

                                      public class SummaryModel : INotifyPropertyChanged
                                      {
                                          private readonly ObservableAsPropertyHelper<string> _label;

                                          public SummaryModel(IObservable<string> labels)
                                          {
                                              _label = labels.ToProperty(this, x => x.Label);
                                          }

                                          public event PropertyChangedEventHandler PropertyChanged;

                                          public string Label => _label.Value;

                                          public void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                                      }

                                      public static class Usage
                                      {
                                          public static IDisposable Run(SourceModel source, TargetModel target)
                                          {
                                              var names = source.WhenChanged(x => x.Name);
                                              var bindTo = names.BindTo(target, x => x.Title);
                                              var oneWay = source.BindOneWay(target, x => x.Name, x => x.Title);
                                              return oneWay;
                                          }
                                      }
                                  }
                                  """;

    /// <summary>Every file-and-line condition tests the suffix with a backslash as well as a forward slash.</summary>
    /// <param name="hintName">The dispatch file to check.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("WhenChangedDispatch.g.cs")]
    [Arguments("BindToDispatch.g.cs")]
    [Arguments("BindOneWayDispatch.g.cs")]
    [Arguments("ToPropertyDispatch.g.cs")]
    public async Task WindowsPath_IsMatchedWithEitherSeparator(string hintName)
    {
        var result = RunWithPath(Source, WindowsPath);

        await result.GeneratedSourceContains(hintName, "callerFilePath.EndsWith(\"Views/MainView.cs\"");
        await result.GeneratedSourceContains(hintName, "callerFilePath.EndsWith(\"Views\\\\MainView.cs\"");
        await result.CompilationSucceeds();
    }

    /// <summary>A chained call written across lines, in a file with a Windows path, dispatches at run time.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ChainedCallAcrossLines_WithWindowsPath_DispatchesAtRunTime()
    {
        var result = RunWithPath(ChainedSource, WindowsPath);
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>A file at the root of the path has no separator in its suffix, so it keeps a single test.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RootFile_KeepsSingleTest()
    {
        var result = RunWithPath(Source, "MainView.cs");

        await result.GeneratedSourceContains("WhenChangedDispatch.g.cs", "callerFilePath.EndsWith(\"MainView.cs\"");
        await result.CompilationSucceeds();
    }

    /// <summary>Generates for source parsed as the file at the given path, below C# 10.</summary>
    /// <param name="source">The source to generate for.</param>
    /// <param name="path">The path the compiler reports for the file.</param>
    /// <returns>The generator result.</returns>
    private static GeneratorTestResult RunWithPath(string source, string path)
    {
        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp7_3);
        var tree = compilation.SyntaxTrees.Single();
        compilation = compilation.ReplaceSyntaxTree(tree, tree.WithFilePath(path));
        return TestHelper.RunGenerator(compilation, LanguageVersion.CSharp7_3, null, false);
    }
}
