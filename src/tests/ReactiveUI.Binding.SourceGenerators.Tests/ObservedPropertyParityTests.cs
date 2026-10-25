// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers <c>ObservedProperty</c> against the generated bindings it stands in for: the same object, observed both ways
/// through the same changes, produces the same values, in both runtime flavours.
/// </summary>
public class ObservedPropertyParityTests
{
    /// <summary>Observes each shape both ways and reports whether every pair of recordings matches.</summary>
    private const string Scenario = """
        using System;
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.Runtime.CompilerServices;
        using RUNTIME_NAMESPACE;

        namespace Parity
        {
            public sealed class Address : INotifyPropertyChanged
            {
                private string? _city;

                public event PropertyChangedEventHandler? PropertyChanged;

                public string? City
                {
                    get => _city;
                    set { _city = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(City))); }
                }
            }

            public sealed class Person : INotifyPropertyChanged
            {
                private string? _name;
                private int _age;
                private Address? _home;
                private IObservable<string>? _messages;

                public event PropertyChangedEventHandler? PropertyChanged;

                public string? Name { get => _name; set { _name = value; Raise(); } }

                public int Age { get => _age; set { _age = value; Raise(); } }

                public Address? Home { get => _home; set { _home = value; Raise(); } }

                public IObservable<string>? Messages { get => _messages; set { _messages = value; Raise(); } }

                public void Raise([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            public sealed class Feed : IObservable<string>
            {
                private readonly List<IObserver<string>> _observers = new List<IObserver<string>>();

                public void Push(string value)
                {
                    foreach (var observer in _observers.ToArray())
                    {
                        observer.OnNext(value);
                    }
                }

                public IDisposable Subscribe(IObserver<string> observer)
                {
                    _observers.Add(observer);
                    return new Removal(() => _observers.Remove(observer));
                }

                private sealed class Removal : IDisposable
                {
                    private readonly Action _remove;

                    public Removal(Action remove) => _remove = remove;

                    public void Dispose() => _remove();
                }
            }

            public sealed class Recorder<T> : IObserver<T>
            {
                public List<string> Values { get; } = new List<string>();

                public void OnCompleted() => Values.Add("completed");

                public void OnError(Exception error) => Values.Add("error");

                public void OnNext(T value) => Values.Add(value?.ToString() ?? "null");
            }

            public static class Usage
            {
                public static string Run()
                {
                    var mismatches = new List<string>();

                    Compare("single", mismatches, p => p.WhenAnyValue(x => x.Name), p => ObservedProperty.Create(p, static x => x.Name, static x => x.Name), Change);
                    Compare(
                        "pair",
                        mismatches,
                        p => p.WhenAnyValue(x => x.Name, x => x.Age),
                        p => ObservedProperty.Create(p, static x => x.Name, static x => x.Name, static x => x.Age, static x => x.Age),
                        Change);
                    Compare(
                        "selector",
                        mismatches,
                        p => p.WhenAnyValue(x => x.Name, x => x.Age, (name, age) => name + age),
                        p => ObservedProperty.Create(p, static x => x.Name, static x => x.Name, static x => x.Age, static x => x.Age, static (name, age) => name + age),
                        Change);
                    Compare(
                        "chain",
                        mismatches,
                        p => p.WhenAnyValue(x => x.Home!.City),
                        p => ObservedProperty.Create(p, static x => x.Home, static x => x.Home).Then(static a => a.City, static a => a.City),
                        Change);
                    Compare(
                        "observable",
                        mismatches,
                        p => p.WhenAnyObservable(x => x.Messages!),
                        p => ObservedProperty.Create(p, static x => x.Messages, static x => x.Messages).Switch(),
                        Change);

                    return mismatches.Count == 0 ? "match" : string.Join(" | ", mismatches);
                }

                private static void Compare<T>(
                    string shape,
                    List<string> mismatches,
                    Func<Person, IObservable<T>> generated,
                    Func<Person, IObservable<T>> observed,
                    Action<Person, Feed, Feed> change)
                {
                    var first = new Feed();
                    var second = new Feed();
                    var person = new Person { Name = "Ada", Age = 3, Home = new Address { City = "Paris" }, Messages = first };
                    var expected = new Recorder<T>();
                    var actual = new Recorder<T>();

                    using (generated(person).Subscribe(expected))
                    using (observed(person).Subscribe(actual))
                    {
                        change(person, first, second);
                    }

                    var left = string.Join(",", expected.Values);
                    var right = string.Join(",", actual.Values);
                    if (left != right)
                    {
                        mismatches.Add(shape + ": " + left + " != " + right);
                    }
                }

                private static void Change(Person person, Feed first, Feed second)
                {
                    first.Push("one");
                    person.Name = "Grace";
                    person.Name = "Grace";
                    person.Raise(nameof(Person.Name));
                    person.Age = 4;
                    person.Home!.City = "Rome";
                    person.Home = null;
                    person.Home = new Address { City = "Rome" };
                    person.Home = new Address { City = "Oslo" };
                    person.Messages = null;
                    first.Push("lost");
                    person.Messages = second;
                    second.Push("two");
                    person.Messages = second;
                    second.Push("three");
                }
            }
        }
        """;

    /// <summary>Every shape produces the same values as its generated binding, in both runtimes, with and without interceptors.</summary>
    /// <param name="useReactiveRuntime">Whether to reference the System.Reactive flavour rather than the lean one.</param>
    /// <param name="intercept">Whether the build opts into interceptors.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false, false)]
    [Arguments(true, false)]
    [Arguments(false, true)]
    [Arguments(true, true)]
    public async Task ObservedProperty_MatchesGeneratedBindings(bool useReactiveRuntime, bool intercept)
    {
        var parseOptions = intercept
            ? TestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp13)
            : TestHelper.ParseOptionsFor(LanguageVersion.CSharp13);
        var source = Scenario.Replace(
            SourceGeneratorsContractTests.RuntimeNamespace,
            useReactiveRuntime ? "ReactiveUI.Binding.Reactive" : "ReactiveUI.Binding",
            StringComparison.Ordinal);
        var compilation = TestHelper.CreateCompilation(source, parseOptions, useReactiveRuntime, "TestAssembly", []);
        var result = TestHelper.RunGeneratorBeside(compilation, parseOptions, []);
        await Assert.That(result.CompilationErrors.Select(static d => d.ToString())).IsEmpty();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("Parity.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((string)run.Invoke(null, null)!).IsEqualTo("match");
        }
        finally
        {
            context.Unload();
        }
    }
}
