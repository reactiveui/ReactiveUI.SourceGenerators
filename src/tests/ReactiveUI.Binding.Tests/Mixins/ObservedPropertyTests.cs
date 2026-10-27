// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Tests.Fallback;
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Primitives.Signals;
using Splat;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>
/// Covers <see cref="ObservedProperty"/>, the observation entry point for code another source generator writes. Its
/// behaviour follows <c>WhenAnyValue</c> and <c>WhenAnyObservable</c>.
/// </summary>
[NotInParallel]
public class ObservedPropertyTests
{
    /// <summary>The name the person starts with.</summary>
    private const string Ada = "Ada";

    /// <summary>The name the person changes to.</summary>
    private const string Grace = "Grace";

    /// <summary>The age the person starts with.</summary>
    private const int Young = 3;

    /// <summary>The age the person changes to.</summary>
    private const int Older = 4;

    /// <summary>A provider score above what change notifications get.</summary>
    private const int ProviderAffinity = 50;

    /// <summary>Both ages, boxed as a property read as <see cref="object"/> produces them.</summary>
    private static readonly object[] BothAges = [Young, Older];

    /// <summary>The name before and after the change, as a nullable property produces them.</summary>
    private static readonly string?[] AdaThenGrace = [Ada, Grace];

    /// <summary>The cities a followed path passes through.</summary>
    private static readonly string?[] Cities = ["Paris", "Rome", "Oslo"];

    /// <summary>Lambdas that name no property of their parameter.</summary>
    /// <returns>The lambdas.</returns>
    public static IEnumerable<Func<Expression<Func<Person, string?>>>> UnnamedProperties()
    {
        yield return static () => static x => x.Home!.City;
        yield return static () => static x => x.ToString();
    }

    /// <summary>A notifying property: its current value first, then each change, with repeats skipped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_NotifyingSource_DeliversCurrentValueThenDistinctChanges()
    {
        var person = new Person { Name = Ada };
        var observer = new RecordingObserver<string?>();

        using var subscription = ObservedProperty.Create(person, static x => x.Name, static x => x.Name).Subscribe(observer);
        person.Name = Grace;
        person.Name = Grace;
        person.Raise(nameof(Person.Name));

        await Assert.That(observer.Values).IsEquivalentTo(AdaThenGrace);
    }

    /// <summary>A lambda that converts the property, as a value type read as <see cref="object"/>, still names it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_ConvertedProperty_IsReadByName()
    {
        var person = new Person { Age = Young };
        var observer = new RecordingObserver<object>();

        using var subscription = ObservedProperty.Create<Person, object>(person, static x => x.Age, static x => x.Age).Subscribe(observer);
        person.Age = Older;

        await Assert.That(observer.Values).IsEquivalentTo(BothAges);
    }

    /// <summary>
    /// A source with no change notification is read once: through the registered fallback provider, and directly when
    /// no provider is registered.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_SourceWithoutNotifications_IsReadOnce()
    {
        var model = new Plain { Value = Ada };
        var withProviders = new RecordingObserver<string>();
        var withoutProviders = new RecordingObserver<string>();

        using var first = ObservedProperty.Create(model, static x => x.Value, static x => x.Value).Subscribe(withProviders);
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            ObservationAffinityChecker.Refresh();
            using var second = ObservedProperty.Create(model, static x => x.Value, static x => x.Value).Subscribe(withoutProviders);
            model.Value = Grace;
        }
        finally
        {
            RuntimeObservationFallbackTests.EnsureInitialized();
            ObservationAffinityChecker.Refresh();
        }

        await Assert.That(withProviders.Values).IsEquivalentTo([Ada]);
        await Assert.That(withoutProviders.Values).IsEquivalentTo([Ada]);
    }

    /// <summary>Two properties produce a pair for their current values and a pair each time either changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_TwoProperties_DeliversPairs()
    {
        var person = new Person { Name = Ada, Age = Young };
        var observer = new RecordingObserver<PropertyValues<string?, int>>();

        using var subscription = ObservedProperty.Create(person, static x => x.Name, static x => x.Name, static x => x.Age, static x => x.Age)
            .Subscribe(observer);
        person.Age = Older;

        await Assert.That(observer.Values).IsEquivalentTo(new PropertyValues<string?, int>[] { new(Ada, Young), new(Ada, Older) });
    }

    /// <summary>Two properties and a selector produce the projection of each pair.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_TwoPropertiesWithSelector_DeliversProjection()
    {
        var person = new Person { Name = Ada, Age = Young };
        var observer = new RecordingObserver<string>();

        using var subscription = ObservedProperty.Create(
                person,
                static x => x.Name,
                static x => x.Name,
                static x => x.Age,
                static x => x.Age,
                static (name, age) => $"{name} {age}")
            .Subscribe(observer);
        person.Name = Grace;

        await Assert.That(observer.Values).IsEquivalentTo(["Ada 3", "Grace 3"]);
    }

    /// <summary>A path follows the object it passes through, including its replacement and a null link.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Then_FollowsEachReplacementOfTheParent()
    {
        var person = new Person { Home = new Address { City = "Paris" } };
        var observer = new RecordingObserver<string?>();

        using var subscription = ObservedProperty.Create(person, static x => x.Home, static x => x.Home)
            .Then(static a => a.City, static a => a.City)
            .Subscribe(observer);
        person.Home!.City = "Rome";
        person.Home = null;
        person.Home = new Address { City = "Rome" };
        person.Home = new Address { City = "Oslo" };

        await Assert.That(observer.Values).IsEquivalentTo(Cities);
    }

    /// <summary>A property that holds an observable produces what its latest observable produces.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Switch_FollowsTheLatestHeldObservable()
    {
        using var first = new Subject<string>();
        using var second = new Subject<string>();
        var person = new Person { Messages = first };
        var observer = new RecordingObserver<string>();

        using var subscription = ObservedProperty.Create(person, static x => x.Messages, static x => x.Messages)
            .Switch()
            .Subscribe(observer);
        first.OnNext("one");
        person.Messages = null;
        first.OnNext("lost");
        person.Messages = second;
        first.OnNext("stale");
        second.OnNext("two");

        await Assert.That(observer.Values).IsEquivalentTo(["one", "two"]);
    }

    /// <summary>A registered provider that scores higher takes over, for a notifying source and for any other.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_HigherScoringProvider_TakesOver()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            using var provider = new ConstantProvider(ProviderAffinity);
            AppLocator.Register<ICreatesObservableForProperty>(() => provider);
            ObservationAffinityChecker.Refresh();

            var person = new Person { Name = Ada };
            var plain = new Plain { Value = Ada };
            var names = new RecordingObserver<string?>();
            var plainValues = new RecordingObserver<string>();

            using var first = ObservedProperty.Create(person, static x => x.Name, static x => x.Name).Subscribe(names);
            using var second = ObservedProperty.Create(plain, static x => x.Value, static x => x.Value).Subscribe(plainValues);
            provider.Changes.OnNext(new ObservedChange<object, object?>(person, null, null));
            person.Name = Grace;
            provider.Changes.OnNext(new ObservedChange<object, object?>(person, null, null));

            await Assert.That(provider.Expressions.Select(static e => e.ToString())).Contains("x.Name");
            await Assert.That(names.Values).IsEquivalentTo(AdaThenGrace);
            await Assert.That(plainValues.Values).IsEquivalentTo([Ada]);
        }
        finally
        {
            RuntimeObservationFallbackTests.EnsureInitialized();
            ObservationAffinityChecker.Refresh();
        }
    }

    /// <summary>A lambda that does not name a property of its parameter is refused.</summary>
    /// <param name="property">The lambda.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodDataSource(nameof(UnnamedProperties))]
    public async Task Create_LambdaThatNamesNoProperty_Throws(Expression<Func<Person, string?>> property) =>
        await Assert.That(() => ObservedProperty.Create(new(), property, static x => x.Name)).ThrowsExactly<ArgumentException>();

    /// <summary>Null arguments are refused.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullArguments_Throw()
    {
        await Assert.That(static () => ObservedProperty.Create<Person, string?>(null!, static x => x.Name, static x => x.Name))
            .ThrowsExactly<ArgumentNullException>();
        await Assert.That(static () => ObservedProperty.Create(new Person(), null!, static x => x.Name)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(static () => ObservedProperty.Create(new Person(), static x => x.Name, null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(static () => ObservedProperty.Create(
                new Person(),
                static x => x.Name,
                static x => x.Name,
                static x => x.Age,
                static x => x.Age,
                (Func<string?, int, int>)null!))
            .ThrowsExactly<ArgumentNullException>();
        await Assert.That(static () => ObservedProperty.Create<Person, string?, int, int>(
                null!,
                static x => x.Name,
                static x => x.Name,
                static x => x.Age,
                static x => x.Age,
                static (_, age) => age))
            .ThrowsExactly<ArgumentNullException>();
        await Assert.That(static () => ((IObservable<Address?>)null!).Then(static a => a.City, static a => a.City))
            .ThrowsExactly<ArgumentNullException>();
        await Assert.That(static () => Signal.Return<Address?>(null).Then(static a => a.City, null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(static () => ((IObservable<IObservable<string>?>)null!).Switch()).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>A person who notifies when a property changes.</summary>
    public sealed class Person : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the name.</summary>
        public string? Name
        {
            get;
            set
            {
                field = value;
                Raise();
            }
        }

        /// <summary>Gets or sets the age.</summary>
        public int Age
        {
            get;
            set
            {
                field = value;
                Raise();
            }
        }

        /// <summary>Gets or sets the home address.</summary>
        public Address? Home
        {
            get;
            set
            {
                field = value;
                Raise();
            }
        }

        /// <summary>Gets or sets the stream of messages.</summary>
        public IObservable<string>? Messages
        {
            get;
            set
            {
                field = value;
                Raise();
            }
        }

        /// <summary>Raises a change notification.</summary>
        /// <param name="propertyName">The property that changed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Raise([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>An address that notifies when its city changes.</summary>
    public sealed class Address : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the city.</summary>
        public string? City
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(City)));
            }
        }
    }

    /// <summary>A model that raises no notification.</summary>
    public sealed class Plain
    {
        /// <summary>Gets or sets the value.</summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>A provider that scores every property the same and reports the changes a test pushes.</summary>
    /// <param name="affinity">The score for every type and property.</param>
    private sealed class ConstantProvider(int affinity) : ICreatesObservableForProperty, IDisposable
    {
        /// <summary>Gets the changes the provider reports.</summary>
        public Subject<IObservedChange<object, object?>> Changes { get; } = new();

        /// <summary>Gets the expressions the provider was handed.</summary>
        public List<System.Linq.Expressions.Expression> Expressions { get; } = [];

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) => affinity;

        /// <inheritdoc/>
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            System.Linq.Expressions.Expression expression,
            string propertyName,
            bool beforeChanged,
            bool suppressWarnings)
        {
            Expressions.Add(expression);
            return Changes;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Changes.Dispose();
    }
}
