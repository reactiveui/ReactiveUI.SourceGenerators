// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Tests for <c>ToPropertyUnsafe</c>, which finds the property and its notifications by reflection.</summary>
public sealed class ToPropertyUnsafeTests
{
    /// <summary>The first value a test pushes.</summary>
    private const string First = "first";

    /// <summary>The second value a test pushes.</summary>
    private const string Second = "second";

    /// <summary>The initial value a test starts from.</summary>
    private const string Initial = "initial";

    /// <summary>The record of a before-change notification for <c>Name</c>.</summary>
    private const string ChangingName = "changing:Name";

    /// <summary>The record of an after-change notification for <c>Name</c>.</summary>
    private const string ChangedName = "changed:Name";

    /// <summary>The property name the string overload is given.</summary>
    private const string PropertyName = nameof(ProtectedNameModel.Name);

    /// <summary>The record of the fake ReactiveUI extensions raising after-change for <c>Name</c>.</summary>
    private const string ReactiveChangedName = "changed:Name@FakeReactiveModel";

    /// <summary>The record of the fake ReactiveUI extensions raising before-change for <c>Name</c>.</summary>
    private const string ReactiveChangingName = "changing:Name@FakeReactiveModel";

    /// <summary>A public raise method that takes event args is called with the property's name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PublicEventArgsRaiseMethod_RaisesChanged()
    {
        var source = new Subject<string>();
        var model = new PublicArgsModel();
        model.Helper = source.ToPropertyUnsafe(model, x => x.Name);

        // A helper that subscribes straight away announces its initial value; only later values are counted here.
        model.Raised.Clear();
        source.OnNext(First);

        await Assert.That(model.Name).IsEqualTo(First);
        await Assert.That(model.Raised).IsEquivalentTo([nameof(PublicArgsModel.Name)]);
    }

    /// <summary>A protected raise method that takes a name is reached, with a name that is not a constant.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ProtectedNameRaiseMethod_WithRuntimeName_RaisesChanged()
    {
        var source = new Subject<string?>();
        var model = new ProtectedNameModel();
        model.Helper = source.ToPropertyUnsafe(model, PropertyName, Initial);

        await Assert.That(model.Name).IsEqualTo(Initial);

        model.Raised.Clear();
        source.OnNext(First);

        await Assert.That(model.Name).IsEqualTo(First);
        await Assert.That(model.Raised).IsEquivalentTo([nameof(ProtectedNameModel.Name)]);
    }

    /// <summary>A field-like event declared on a base type raises changing before changed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InheritedFieldLikeEvents_RaiseChangingThenChanged()
    {
        var source = new Subject<string?>();
        var model = new DerivedEventModel();
        _ = source.ToPropertyUnsafe(model, x => x.Name, out var helper);
        model.Helper = helper;
        var raised = new List<string>();
        model.PropertyChanging += (_, e) => raised.Add($"changing:{e.PropertyName}");
        model.PropertyChanged += (_, e) => raised.Add($"changed:{e.PropertyName}");

        source.OnNext(First);
        source.OnNext(Second);

        await Assert.That(model.Name).IsEqualTo(Second);
        await Assert.That(raised).IsEquivalentTo([ChangingName, ChangedName, ChangingName, ChangedName]);
    }

    /// <summary>An IReactiveObject raises through ReactiveUI's public raise extensions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObject_RaisesThroughExtensions()
    {
        var source = new Subject<string>();
        var model = new FakeReactiveModel();
        model.Helper = source.ToPropertyUnsafe(model, x => x.Name, static () => Initial);

        await Assert.That(model.Name).IsEqualTo(Initial);

        source.OnNext(First);

        await Assert.That(IReactiveObjectExtensions.Raised).Contains(ReactiveChangedName);
        await Assert.That(IReactiveObjectExtensions.Raised).Contains(ReactiveChangingName);
    }

    /// <summary>A deferred helper subscribes on its first read.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Deferred_SubscribesOnFirstRead()
    {
        var source = new BehaviorSubject<string>(First);
        var model = new PublicArgsModel();
        model.Helper = source.ToPropertyUnsafe(model, x => x.Name, Initial, true);

        await Assert.That(source.HasObservers).IsFalse();
        await Assert.That(model.Name).IsEqualTo(First);
        await Assert.That(source.HasObservers).IsTrue();
    }

    /// <summary>A type with no reachable notification throws when the helper is created.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoNotification_Throws()
    {
        var source = new Subject<string>();
        var model = new SilentModel();

        await Assert.That(() => source.ToPropertyUnsafe(model, x => x.Name)).Throws<InvalidOperationException>();
    }

    /// <summary>A selector that does not read one member off its parameter throws.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IndirectSelector_Throws()
    {
        var source = new Subject<string>();
        var model = new PublicArgsModel();

        await Assert.That(() => source.ToPropertyUnsafe(model, x => x.Name!.Trim())).Throws<ArgumentException>();
    }

    /// <summary>A model with a public raise method that takes event args.</summary>
    public sealed class PublicArgsModel
    {
        /// <summary>Gets or sets the helper behind <see cref="Name"/>.</summary>
        public ObservableAsPropertyHelper<string?>? Helper { get; set; }

        /// <summary>Gets the names raised.</summary>
        public List<string> Raised { get; } = [];

        /// <summary>Gets the property value.</summary>
        public string? Name => Helper?.Value;

        /// <summary>Records a raised notification.</summary>
        /// <param name="args">The event args.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanged(PropertyChangedEventArgs args) => Raised.Add(args.PropertyName ?? string.Empty);
    }

    /// <summary>A model with a protected raise method that takes a name.</summary>
    public class ProtectedNameModel
    {
        /// <summary>Gets or sets the helper behind <see cref="Name"/>.</summary>
        public ObservableAsPropertyHelper<string?>? Helper { get; set; }

        /// <summary>Gets the names raised.</summary>
        public List<string> Raised { get; } = [];

        /// <summary>Gets the property value.</summary>
        public string? Name => Helper?.Value;

        /// <summary>Records a raised notification.</summary>
        /// <param name="propertyName">The property name.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnPropertyChanged(string propertyName) => Raised.Add(propertyName);
    }

    /// <summary>A base type that declares both events as field-like events.</summary>
    public class BaseEventModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>Gets a value indicating whether either event has a subscriber.</summary>
        public bool HasSubscribers => PropertyChanged is not null || PropertyChanging is not null;
    }

    /// <summary>A derived type whose property the helper backs.</summary>
    public sealed class DerivedEventModel : BaseEventModel
    {
        /// <summary>Gets or sets the helper behind <see cref="Name"/>.</summary>
        public ObservableAsPropertyHelper<string?>? Helper { get; set; }

        /// <summary>Gets the property value.</summary>
        public string? Name => Helper?.Value;
    }

    /// <summary>A model that implements the fake IReactiveObject.</summary>
    public sealed class FakeReactiveModel : IReactiveObject
    {
        /// <summary>Gets the event args passed to the interface's raise method, which the extensions bypass.</summary>
        public List<PropertyChangedEventArgs> DirectRaises { get; } = [];

        /// <summary>Gets or sets the helper behind <see cref="Name"/>.</summary>
        public ObservableAsPropertyHelper<string?>? Helper { get; set; }

        /// <summary>Gets the property value.</summary>
        public string? Name => Helper?.Value;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanged(PropertyChangedEventArgs args) => DirectRaises.Add(args);
    }

    /// <summary>A model that raises no notification.</summary>
    public sealed class SilentModel
    {
        /// <summary>Gets or sets the property value.</summary>
        public string? Name { get; set; }
    }
}
