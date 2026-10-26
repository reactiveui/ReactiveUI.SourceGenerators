// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="ToPropertyAnalyzer"/>, which reports RXUIBIND012 and RXUIBIND013.</summary>
public class ToPropertyAnalyzerTests
{
    /// <summary>The diagnostic reported for a source whose notifications generated code cannot raise.</summary>
    private const string UnraisableId = "RXUIBIND012";

    /// <summary>The diagnostic reported for a property the generator cannot read.</summary>
    private const string UnreadableId = "RXUIBIND013";

    /// <summary>The runtime stub the analyzer recognizes, with its expression and string overloads.</summary>
    private const string Preamble = """
                                    using System;
                                    using System.ComponentModel;
                                    using System.Linq.Expressions;
                                    using ReactiveUI.Binding;

                                    namespace ReactiveUI.Binding
                                    {
                                        public sealed class ObservableAsPropertyHelper<T>
                                        {
                                            public T Value => default;
                                        }

                                        public static class ReactiveUIBindingExtensions
                                        {
                                            public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
                                                this IObservable<TRet> target,
                                                TObj source,
                                                Expression<Func<TObj, TRet>> property)
                                                where TObj : class
                                                => throw new InvalidOperationException();

                                            public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
                                                this IObservable<TRet> target,
                                                TObj source,
                                                string property)
                                                where TObj : class
                                                => throw new InvalidOperationException();
                                        }
                                    }

                                    """;

    /// <summary>A parenthesized selector that reads one member off its parameter names the property directly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsDirectMemberSelector_ParenthesizedLambda_ReturnsTrue() =>
        await Assert.That(ToPropertyAnalyzer.IsDirectMemberSelector(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression("(x) => x.Name"))).IsTrue();

    /// <summary>A selector held in a variable names no property the generator can read.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsDirectMemberSelector_NotALambda_ReturnsFalse() =>
        await Assert.That(ToPropertyAnalyzer.IsDirectMemberSelector(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression("selector"))).IsFalse();

    /// <summary>A type with only a protected raise method and no partial declaration cannot be raised from generated code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND012_ProtectedRaiseOnNonPartialType_Reported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 private readonly ObservableAsPropertyHelper<string> _name;

                                                 public MyViewModel(IObservable<string> names) => _name = names.ToProperty(this, x => x.Name);

                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 public string Name => _name.Value;

                                                 protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(UnraisableId);
    }

    /// <summary>A partial type that declares its own event can be raised through a generated accessor.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND012_PartialTypeWithEvent_NotReported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public partial class MyViewModel : INotifyPropertyChanged
                                             {
                                                 private readonly ObservableAsPropertyHelper<string> _name;

                                                 public MyViewModel(IObservable<string> names) => _name = names.ToProperty(this, x => x.Name);

                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 public string Name => _name.Value;
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.IsEmpty).IsTrue();
    }

    /// <summary>A nested partial type whose containing type is not partial cannot take a generated accessor.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND012_PartialTypeInNonPartialContainer_Reported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public class Outer
                                             {
                                                 public partial class MyViewModel : INotifyPropertyChanged
                                                 {
                                                     public MyViewModel(IObservable<string> names) => names.ToProperty(this, x => x.Name);

                                                     public event PropertyChangedEventHandler PropertyChanged;

                                                     public string Name => "";
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(UnraisableId);
    }

    /// <summary>A public raise method is callable from generated code, so no partial declaration is needed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND012_PublicRaiseMethod_NotReported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public MyViewModel(IObservable<int> counts) => counts.ToProperty(this, x => x.Count);

                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 public int Count => 0;

                                                 public void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.IsEmpty).IsTrue();
    }

    /// <summary>A ReactiveUI object is raised through ReactiveUI's public extensions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND012_ReactiveObject_NotReported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public class MyViewModel : global::ReactiveUI.ReactiveObject
                                             {
                                                 public MyViewModel(IObservable<string> names) => names.ToProperty(this, x => x.Name);

                                                 public string Name => "";
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.IsEmpty).IsTrue();
    }

    /// <summary>A selector that reaches past the source object is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND013_DeepSelector_Reported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public partial class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public MyViewModel(IObservable<int> lengths) => lengths.ToProperty(this, x => x.Name.Length);

                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 public string Name => "";
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(UnreadableId);
    }

    /// <summary>A property named by nameof or a parenthesized selector is readable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND013_NameOfAndParenthesizedSelector_NotReported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public partial class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public MyViewModel(IObservable<string> names)
                                                 {
                                                     names.ToProperty(this, nameof(Name));
                                                     names.ToProperty(this, x => (x.Name));
                                                 }

                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 public string Name => "";
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.IsEmpty).IsTrue();
    }

    /// <summary>A property name held in a variable is not a constant, so it is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND013_NonConstantName_Reported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public partial class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public MyViewModel(IObservable<string> names, string propertyName) => names.ToProperty(this, propertyName);

                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 public string Name => "";
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(UnreadableId);
    }

    /// <summary>A private property is only named by the selector, so the path checks do not report it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PrivateProperty_NoPathDiagnostic()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public partial class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public MyViewModel(IObservable<string> names) => names.ToProperty(this, x => x.Name);

                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 private string Name => "";
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        await Assert.That(diagnostics.IsEmpty).IsTrue();
    }

    /// <summary>The source type is raised rather than observed, so the observable-mechanism check does not report it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PlainSource_NoObservableMechanismDiagnostic()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public class MyViewModel
                                             {
                                                 public MyViewModel(IObservable<string> names) => names.ToProperty(this, x => x.Name);

                                                 public string Name => "";
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.IsEmpty).IsTrue();
    }

    /// <summary>Runs the analyzer with ReactiveUI's own assembly referenced, where its raise extensions live.</summary>
    /// <param name="source">The source to analyze.</param>
    /// <returns>The analyzer's diagnostics.</returns>
    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var compilation = AnalyzerTestHelper.CreateCompilation(source)
            .AddReferences(MetadataReference.CreateFromFile(typeof(ReactiveObject).Assembly.Location));
        var analyzer = new ToPropertyAnalyzer();
        var diagnostics = await compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync();
        return [.. diagnostics.Where(static d => d.Id is UnraisableId or UnreadableId)];
    }
}
