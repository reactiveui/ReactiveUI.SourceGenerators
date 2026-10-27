// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Analyzer.Benchmarks.Support;

/// <summary>Writes the consumer source the analyzer benchmarks run over.</summary>
/// <remarks>
/// A corpus is a set of files, each holding the same calls in its own namespace. The null-forgiving variants
/// write the same paths with a <c>!</c> after every link that can carry one, so the pair isolates what
/// reading through the operator costs.
/// </remarks>
internal static class AnalyzerCorpus
{
    /// <summary>A single empty class: the fixed cost every compilation pays before an analyzer has anything to decide.</summary>
    internal const string Startup = "Startup";

    /// <summary>Calls the analyzers have nothing to report on, written without a null-forgiving operator.</summary>
    internal const string Clean = "Clean";

    /// <summary>The <see cref="Clean"/> calls with a null-forgiving operator after each link.</summary>
    internal const string CleanNullForgiving ="CleanNullForgiving";

    /// <summary>Calls each of the private, unsupported-segment, silent-link and no-notification checks reports on.</summary>
    internal const string Violating = "Violating";

    /// <summary>The <see cref="Violating"/> calls with a null-forgiving operator after each link.</summary>
    internal const string ViolatingNullForgiving ="ViolatingNullForgiving";

    /// <summary>Calls to the runtime-reflection overloads whose first type argument is not the object they observe.</summary>
    internal const string UnsafeTargets ="UnsafeTargets";

    /// <summary>The number of files in every corpus but <see cref="Startup"/>.</summary>
    internal const int FileCount = 24;

    /// <summary>The token a template writes where a null-forgiving operator may go.</summary>
    private const string BangToken = "__B__";

    /// <summary>The token a template writes where its file number goes.</summary>
    private const string NumberToken = "__N__";

    /// <summary>The null-forgiving operator.</summary>
    private const string Bang = "!";

    /// <summary>The types a file declares, shared by every template.</summary>
    private const string Header = """
        using System;
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.Windows.Input;
        using ReactiveUI.Binding;

        namespace Corpus__N__
        {
            public class NotifyingAddress : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public string City { get; set; } = "";
            }

            public class SilentAddress
            {
                public string City { get; set; } = "";
            }

            public class PlainView : IViewFor
            {
                public object? ViewModel { get; set; }

                public string Text { get; set; } = "";

                public decimal Amount { get; set; }
            }

            public class PlainModel
            {
                public string Name { get; set; } = "";
            }

            public class View : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public string Text { get; set; } = "";

                public string Caption { get; set; } = "";

                public decimal Amount { get; set; }
            }

            public class Model : INotifyPropertyChanged
            {
                public string Field = "";

                public event PropertyChangedEventHandler? PropertyChanged;

                public string Name { get; set; } = "";

                public decimal Amount { get; set; }

                public NotifyingAddress? Address { get; set; }

                public SilentAddress? Silent { get; set; }

                public ICommand? Save { get; set; }

                public List<string> Items { get; set; } = new();

                private string Secret { get; set; } = "";

                public string GetName() => "";

                public void ObservePrivate()
                {
                    __PRIVATE__
                }
            }

            public class Usage
            {
        __BODY__
            }
        }
        """;

    /// <summary>Calls whose paths, types and notifications are all in order.</summary>
    private const string CleanBody = """
                public void Bindings(Model model, View view, IObservable<decimal> values)
                {
                    model.WhenChanged(x => x.Name__B__);
                    model.WhenChanged(x => x.Address__B__.City__B__);
                    model.BindOneWay(view, x => x.Address__B__.City__B__, x => x.Text);
                    model.BindTwoWay(view, x => x.Name__B__, x => x.Caption);
                    values.BindTo(view, x => x.Amount);
                    values.InvokeCommand(model, x => x.Save);
                }
        """;

    /// <summary>Calls that a private member, an unsupported segment, a silent link and a plain type each trip.</summary>
    private const string ViolatingBody = """
                public void Bindings(Model model, PlainModel plain, View view)
                {
                    model.WhenChanged(x => x.Silent__B__.City__B__);
                    model.WhenChanged(x => x.Field__B__);
                    model.WhenChanged(x => x.GetName()__B__);
                    model.WhenChanged(x => x.Items[0]__B__);
                    model.WhenChanged(x => x.Items__B__[0]);
                    model.BindOneWay(view, x => x.Silent__B__.City__B__, x => x.Text);
                    plain.WhenChanged(x => x.Name__B__);
                }
        """;

    /// <summary>Runtime-reflection calls whose first type argument is a stream's value type or a write-only target.</summary>
    private const string UnsafeTargetsBody = """
                public void Bindings(Model model, PlainView plainView, IObservable<decimal> values)
                {
                    values.BindToUnsafe(plainView, x => x.Amount);
                    values.InvokeCommandUnsafe(model, x => x.Save);
                    plainView.OneWayBindUnsafe(model, x => x.Name, x => x.Text);
                    plainView.OneWayBindUnsafe(model, x => x.Name, x => x.Text, s => s, null);
                }
        """;

    /// <summary>The call a violating corpus makes to a private member from inside its own class.</summary>
    private const string PrivateCall = "this.WhenChanged(x => x.Secret__B__);";

    /// <summary>The token a template writes where the private member call goes.</summary>
    private const string PrivateToken = "__PRIVATE__";

    /// <summary>The single file of the <see cref="Startup"/> corpus.</summary>
    private const string EmptyClass = "namespace Corpus { public class Empty { } }";

    /// <summary>Builds the source files of a corpus.</summary>
    /// <param name="corpus">The corpus to build, one of the constants on this type.</param>
    /// <returns>The source text of each file.</returns>
    /// <exception cref="ArgumentException">The name is not a corpus.</exception>
    internal static string[] Build(string corpus)
    {
        if (corpus == Startup)
        {
            return [EmptyClass];
        }

        var (body, privateCall, bang) = corpus switch
        {
            Clean => (CleanBody, string.Empty, string.Empty),
            CleanNullForgiving => (CleanBody, string.Empty, Bang),
            Violating => (ViolatingBody, PrivateCall, string.Empty),
            ViolatingNullForgiving => (ViolatingBody, PrivateCall, Bang),
            UnsafeTargets => (UnsafeTargetsBody, string.Empty, string.Empty),
            _ => throw new ArgumentException($"'{corpus}' is not a corpus.", nameof(corpus)),
        };

        var template = Header
            .Replace("__BODY__", body)
            .Replace(PrivateToken, privateCall)
            .Replace(BangToken, bang);
        var files = new string[FileCount];
        for (var i = 0; i < files.Length; i++)
        {
            files[i] = template.Replace(NumberToken, i.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return files;
    }
}
