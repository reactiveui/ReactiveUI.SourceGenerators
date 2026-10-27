// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>
/// Covers RXUIBIND011, which reports a binding call answered by ReactiveUI's own mixin. Without it the call
/// generates nothing and takes the runtime expression engine, and no other diagnostic has anything to say
/// because the call was never recognised as one to generate for.
/// </summary>
public class MixinShadowAnalyzerTests
{
    /// <summary>The diagnostic this analyzer reports.</summary>
    private const string DiagnosticId = "RXUIBIND011";

    /// <summary>The namespace ReactiveUI declares its own mixins in.</summary>
    private const string ReactiveUiNamespace = "ReactiveUI";

    /// <summary>One of the APIs this package generates bindings for.</summary>
    private const string GeneratedApi = "WhenAnyValue";

    /// <summary>A declaration of this package's stub class, which is how the analyzer knows it is referenced.</summary>
    private const string BindingPackage = """

                                          namespace ReactiveUI.Binding
                                          {
                                              public static class ReactiveUIBindingExtensions
                                              {
                                              }
                                          }
                                          """;

    /// <summary>A consumer whose call reaches this package's own extension rather than ReactiveUI's.</summary>
    private const string GeneratedOverloadSource = """
                                                   using System;
                                                   using System.ComponentModel;
                                                   using System.Linq.Expressions;
                                                   using ReactiveUI.Binding;

                                                   namespace ReactiveUI.Binding
                                                   {
                                                       public static class ReactiveUIBindingExtensions
                                                       {
                                                           public static IObservable<TRet> WhenAnyValue<TSender, TRet>(
                                                               this TSender sender,
                                                               Expression<Func<TSender, TRet>> property)
                                                               where TSender : class
                                                               => throw new NotImplementedException();
                                                       }
                                                   }

                                                   namespace Consumer
                                                   {
                                                       public class ConsumerViewModel : INotifyPropertyChanged
                                                       {
                                                           public event PropertyChangedEventHandler PropertyChanged;

                                                           public string Name { get; set; }
                                                       }

                                                       public static class Usage
                                                       {
                                                           public static IObservable<string> Observe(ConsumerViewModel viewModel)
                                                           {
                                                               return viewModel.WhenAnyValue(x => x.Name);
                                                           }
                                                       }
                                                   }
                                                   """;

    /// <summary>A mixin declared in the global namespace, which belongs to nobody in particular.</summary>
    private const string GlobalNamespaceMixinSource = """
                                                      using System;
                                                      using System.ComponentModel;
                                                      using System.Linq.Expressions;

                                                      public static class WhenAnyMixins
                                                      {
                                                          public static IObservable<TRet> WhenAnyValue<TSender, TRet>(
                                                              this TSender sender,
                                                              Expression<Func<TSender, TRet>> property)
                                                              where TSender : class
                                                              => throw new NotImplementedException();
                                                      }

                                                      namespace ReactiveUI.Binding
                                                      {
                                                          public static class ReactiveUIBindingExtensions
                                                          {
                                                          }
                                                      }

                                                      namespace Consumer
                                                      {
                                                          public class ConsumerViewModel : INotifyPropertyChanged
                                                          {
                                                              public event PropertyChangedEventHandler PropertyChanged;

                                                              public string Name { get; set; }
                                                          }

                                                          public static class Usage
                                                          {
                                                              public static IObservable<string> Observe(ConsumerViewModel viewModel)
                                                              {
                                                                  return viewModel.WhenAnyValue(x => x.Name);
                                                              }
                                                          }
                                                      }
                                                      """;

    /// <summary>A mixin reached through a type nested inside the one the consumer would name.</summary>
    private const string NestedMixinSource = """
                                             using System;
                                             using System.ComponentModel;
                                             using System.Linq.Expressions;

                                             namespace ReactiveUI
                                             {
                                                 public static class WhenAnyMixins
                                                 {
                                                     public static class Grouping
                                                     {
                                                         public static IObservable<TRet> WhenAnyValue<TSender, TRet>(
                                                             TSender sender,
                                                             Expression<Func<TSender, TRet>> property)
                                                             where TSender : class
                                                             => throw new NotImplementedException();
                                                     }
                                                 }
                                             }

                                             namespace ReactiveUI.Binding
                                             {
                                                 public static class ReactiveUIBindingExtensions
                                                 {
                                                     }
                                             }

                                             namespace Consumer
                                             {
                                                 public class ConsumerViewModel : INotifyPropertyChanged
                                                 {
                                                     public event PropertyChangedEventHandler PropertyChanged;

                                                     public string Name { get; set; }
                                                 }

                                                 public static class Usage
                                                 {
                                                     public static IObservable<string> Observe(ConsumerViewModel viewModel)
                                                     {
                                                         return ReactiveUI.WhenAnyMixins.Grouping.WhenAnyValue(viewModel, x => x.Name);
                                                     }
                                                 }
                                             }
                                             """;

    /// <summary>A call ReactiveUI's mixin answered, in a project that references this package.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MixinCall_WithTheBindingPackageReferenced_IsReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MixinShadowAnalyzer>(
            Source(ReactiveUiNamespace, GeneratedApi, BindingPackage));

        await Assert.That(diagnostics.Count(static d => d.Id == DiagnosticId)).IsEqualTo(1);
    }

    /// <summary>An InvokeCommand answered by ReactiveUI's mixin is reported like any other generated API.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MixinCall_ToInvokeCommand_IsReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MixinShadowAnalyzer>(
            Source(ReactiveUiNamespace, "InvokeCommand", BindingPackage));

        await Assert.That(diagnostics.Count(static d => d.Id == DiagnosticId)).IsEqualTo(1);
    }

    /// <summary>A ReactiveUI method that shares a binding API's name but takes no selector is a different API.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SameNamedReactiveUiMethodWithoutASelector_IsNotReported()
    {
        const string Source = """
            using System;
            using System.Collections.Generic;

            namespace ReactiveUI
            {
                public sealed class TableView
                {
                }

                public static class TableSourceExtensions
                {
                    public static IDisposable BindTo<TSource>(this IObservable<IReadOnlyList<TSource>> sections, TableView tableView) =>
                        throw new NotImplementedException();
                }
            }

            namespace ReactiveUI.Binding
            {
                public static class ReactiveUIBindingExtensions
                {
                }
            }

            namespace Consumer
            {
                using ReactiveUI;

                public static class Usage
                {
                    public static IDisposable Bind(IObservable<IReadOnlyList<string>> sections) => sections.BindTo(new TableView());
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MixinShadowAnalyzer>(Source);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>Without this package there is no generated overload to have lost, so nothing is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MixinCall_WithoutTheBindingPackage_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MixinShadowAnalyzer>(
            Source(ReactiveUiNamespace, GeneratedApi, string.Empty));

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>A method this package does not generate for is ReactiveUI's business alone.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MixinCall_WithAMethodThisPackageDoesNotGenerate_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MixinShadowAnalyzer>(
            Source(ReactiveUiNamespace, "Observe", BindingPackage));

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>A same-named method belonging to somebody else entirely is not ReactiveUI's mixin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MixinCall_FromAnUnrelatedNamespace_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MixinShadowAnalyzer>(
            Source("Fabrikam", GeneratedApi, BindingPackage));

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>A namespace merely ending in ReactiveUI is somebody else's, so it is left alone.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MixinCall_FromANamespaceNestedUnderAnother_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MixinShadowAnalyzer>(
            Source($"Contoso.{ReactiveUiNamespace}", GeneratedApi, BindingPackage));

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>A call that reached this package's own extension is the outcome being asked for.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GeneratedOverload_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MixinShadowAnalyzer>(
            GeneratedOverloadSource);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>A mixin in the global namespace is nobody's, so it is left alone.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MixinCall_FromTheGlobalNamespace_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MixinShadowAnalyzer>(
            GlobalNamespaceMixinSource);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>
    /// A method declared in a type nested inside the mixin class is still ReactiveUI's, which is the shape an
    /// extension block produces: its members belong to a synthesized type the consumer never wrote.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MixinCall_ThroughANestedType_IsReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<MixinShadowAnalyzer>(NestedMixinSource);

        await Assert.That(diagnostics.Count(static d => d.Id == DiagnosticId)).IsEqualTo(1);
    }

    /// <summary>Builds a consumer whose call is answered by a mixin in the named namespace.</summary>
    /// <param name="mixinNamespace">The namespace the mixin is declared in.</param>
    /// <param name="methodName">The name the mixin exposes, and the name the consumer calls.</param>
    /// <param name="bindingPackage">A declaration of this package's stub class, or an empty string.</param>
    /// <returns>The source to analyze.</returns>
    private static string Source(string mixinNamespace, string methodName, string bindingPackage) => $$"""
        using System;
        using System.ComponentModel;
        using System.Linq.Expressions;
        using {{mixinNamespace}};

        namespace {{mixinNamespace}}
        {
            public static class WhenAnyMixins
            {
                public static IObservable<TRet> {{methodName}}<TSender, TRet>(
                    this TSender sender,
                    Expression<Func<TSender, TRet>> property)
                    where TSender : class
                    => throw new NotImplementedException();
            }
        }
        {{bindingPackage}}

        namespace Consumer
        {
            public class ConsumerViewModel : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;

                public string Name { get; set; }
            }

            public static class Usage
            {
                public static IObservable<string> Observe(ConsumerViewModel viewModel)
                {
                    return viewModel.{{methodName}}(x => x.Name);
                }
            }
        }
        """;
}
