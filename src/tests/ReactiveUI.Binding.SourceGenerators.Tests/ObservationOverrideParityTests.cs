// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Executes custom observation voting, timing, typed reads and cache refresh in generated consumers.</summary>
public class ObservationOverrideParityTests
{
    /// <summary>The notifying source and independently driven custom provider.</summary>
    private const string Fixture = """
        using System;
        using System.ComponentModel;
        using System.Linq.Expressions;
        using ReactiveUI.Binding;
        public class Source : INotifyPropertyChanged, INotifyPropertyChanging
        {
            private int _value;
            public event PropertyChangedEventHandler PropertyChanged;
            public event PropertyChangingEventHandler PropertyChanging;
            public int Value
            {
                get { return _value; }
                set { PropertyChanging?.Invoke(this, new PropertyChangingEventArgs("Value")); _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value")); }
            }
        }
        public class DerivedSource : Source {}
        public class Observer : IObserver<int>
        {
            public int Last;
            public void OnNext(int value) { Last = value; }
            public void OnError(Exception error) { throw error; }
            public void OnCompleted() { throw new InvalidOperationException("Observation completed"); }
        }
        public class Provider : ICreatesObservableForProperty
        {
            public int Score, Votes, Calls;
            public bool Before;
            public object Sender;
            public Expression Expression;
            public ReactiveUI.Primitives.Signals.Signal<IObservedChange<object, object>> Notifications = new ReactiveUI.Primitives.Signals.Signal<IObservedChange<object, object>>();
            public int GetAffinityForObject(Type type, string property, bool before)
            {
                Votes++;
                return type == typeof(DerivedSource) && property == "Value" && before == Before ? Score : 0;
            }
            public IObservable<IObservedChange<object, object>> GetNotificationForProperty(object sender, Expression expression, string property, bool before, bool suppress)
            {
                if (!(expression is MemberExpression member) || member.Member.Name != property || before != Before) throw new InvalidOperationException("Incorrect property contract");
                Calls++;
                Sender = sender;
                Expression = expression;
                return Notifications;
            }
            public void Fire() { Notifications.OnNext(new ObservedChange<object, object>(Sender, Expression, 999)); }
        }
        """;

    /// <summary>Custom providers need higher property-specific affinity, and refresh invalidates cached votes.</summary>
    /// <param name="score">The custom provider's score.</param>
    /// <param name="before">Whether the call observes before-change notifications.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(4, false)]
    [Arguments(5, false)]
    [Arguments(6, false)]
    [Arguments(4, true)]
    [Arguments(5, true)]
    [Arguments(6, true)]
    public async Task Observation_RespectsVotesAndRefresh(int score, bool before)
    {
        var result = TestHelper.RunGenerator(Scenario(score, before), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        var (assembly, context) = TestHelper.EmitAndLoad(result, true);
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

    /// <summary>Exercises a generated observation with runtime-derived ownership and registration changes.</summary>
    /// <param name="score">The initial provider's affinity.</param>
    /// <param name="before">The requested notification timing.</param>
    /// <returns>The executable consumer.</returns>
    private static string Scenario(int score, bool before) => Fixture + $$"""
        public static class Usage
        {
            public static IObservable<int> Observe(Source source) => source.{{(before ? "WhenChanging" : "WhenChanged")}}(x => x.Value);
            public static bool Run()
            {
                var provider = new Provider { Score = {{score}}, Before = {{(before ? "true" : "false")}} };
                using (var resolver = new Splat.ModernDependencyResolver())
                using (Splat.DependencyResolverMixins.WithResolver(resolver))
                {
                    resolver.Register<ICreatesObservableForProperty>(() => provider);
                    ReactiveUI.Binding.Fallback.ObservationAffinityChecker.Refresh();
                    Source source = new DerivedSource();
                    var observer = new Observer();
                    using (Observe(source).Subscribe(observer))
                    {
                        var votes = provider.Votes;
                        source.Value = 7;
                        source.Value = 9;
                        var custom = {{score}} > 5;
                        if (observer.Last != (custom ? 0 : {{(before ? "7" : "9")}})) return false;
                        provider.Fire();
                        if (observer.Last != (custom ? 9 : {{(before ? "7" : "9")}})) return false;
                        if (provider.Votes != votes || provider.Calls != (custom ? 1 : 0)) return false;
                    }
                    if (provider.Notifications.HasObservers) return false;
                    var replacement = new Provider { Score = 99, Before = provider.Before };
                    resolver.Register<ICreatesObservableForProperty>(() => replacement);
                    using (Observe(source).Subscribe(observer))
                        if (replacement.Calls != 0) return false;
                    ReactiveUI.Binding.Fallback.ObservationAffinityChecker.Refresh();
                    using (Observe(source).Subscribe(observer))
                        if (replacement.Calls != 1) return false;
                    return !replacement.Notifications.HasObservers;
                }
            }
        }
        """;
}
