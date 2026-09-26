// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.CodeGeneration;

/// <summary>The members both Windows Forms hosts share, written through a <see cref="SourceWriter"/>.</summary>
/// <remarks>
/// A host never calls <c>WhenAny</c>: the binding engine's generator cannot see call sites in this generator's output,
/// so a <c>WhenAny</c> in a host would not be dispatched. With ReactiveUI.Binding 8.4.0 or later a host follows its
/// properties through Binding's <c>ObservedProperty</c>, which has <c>WhenAnyValue</c>'s semantics; with anything older
/// it follows them through its own <c>PropertyObservable</c> over its <c>PropertyChanged</c> event.
/// </remarks>
internal static class ControlHostExtensions
{
    /// <summary>Writes the observables a host follows its own properties with.</summary>
    /// <param name="writer">The writer the observable expression is appended to.</param>
    extension(SourceWriter writer)
    {
        /// <summary>Writes an observable of a host property's value: its current value, then each change.</summary>
        /// <param name="integration">The detected ReactiveUI integration, which says whether <c>ObservedProperty</c> exists.</param>
        /// <param name="type">The property's type.</param>
        /// <param name="property">The property's name.</param>
        /// <returns>The writer.</returns>
        internal SourceWriter AppendPropertyValue(ReactiveUiIntegration integration, string type, string property) =>
            integration.HasObservedProperty
                ? writer.AppendObservedPropertyCreate(integration, property)
                : writer.Append("new PropertyObservable<").Append(type).Append(">(this, nameof(").Append(property)
                    .Append("), () => new ReturnObservable<").Append(type).Append(">(").Append(property).Append("))");

        /// <summary>Writes an observable of what the observable a host property holds produces, switching as it changes.</summary>
        /// <param name="integration">The ReactiveUI integration, which names the <c>ObservedProperty</c> flavour when there is one.</param>
        /// <param name="type">The type the property's observable produces.</param>
        /// <param name="property">The name of the property holding the observable.</param>
        /// <returns>The writer, after the expression.</returns>
        internal SourceWriter AppendPropertyObservable(ReactiveUiIntegration integration, string type, string property) =>
            integration.HasObservedProperty
                ? writer.Append(integration.ObservedProperty).Append(".Switch(").AppendObservedPropertyCreate(integration, property).Append(')')
                : writer.Append("new PropertyObservable<").Append(type).Append(">(this, nameof(").Append(property)
                    .Append("), () => ").Append(property).Append(')');

        /// <summary>Writes an observable of the routed host's current view model, following <c>Router.CurrentViewModel</c>.</summary>
        /// <param name="integration">The integration that decides between <c>ObservedProperty</c> and the host's own observable.</param>
        /// <returns>The writer, after the routed view model expression.</returns>
        internal SourceWriter AppendRoutedViewModel(ReactiveUiIntegration integration) =>
            integration.HasObservedProperty
                ? writer.Append(integration.ObservedProperty).Append(".Switch(")
                    .Append(integration.ObservedProperty).Append(".Then(").AppendObservedPropertyCreate(integration, "Router")
                    .Append(", static router => router.CurrentViewModel, static router => router.CurrentViewModel))")
                : writer.Append("new PropertyObservable<IRoutableViewModel?>(this, nameof(Router), () => Router?.CurrentViewModel)");

        /// <summary>Writes <c>ObservedProperty.Create</c> for a host property, with static lambdas that do not allocate.</summary>
        /// <param name="integration">The integration naming the flavour <c>ObservedProperty</c> is written from.</param>
        /// <param name="property">The observed property's name.</param>
        /// <returns>The writer, after the call.</returns>
        private SourceWriter AppendObservedPropertyCreate(ReactiveUiIntegration integration, string property) =>
            writer.Append(integration.ObservedProperty).Append(".Create(this, static x => x.").Append(property)
                .Append(", static x => x.").Append(property).Append(')');
    }

    /// <summary>Writes the host's change events and its <c>IReactiveObject</c> implementation.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    /// <remarks>
    /// ReactiveUI raises the classic events of a hand-written <c>IReactiveObject</c> only once it has called
    /// <c>SubscribePropertyChangedEvents</c> and <c>SubscribePropertyChangingEvents</c>, so the first handler added
    /// makes that call, as <c>ReactiveObject</c> does.
    /// </remarks>
    internal static void WritePropertyChangeEvents(SourceWriter writer) =>
        _ = writer.Lines("""
            private bool _propertyChangingEventsSubscribed;
            private bool _propertyChangedEventsSubscribed;

            /// <inheritdoc/>
            public event PropertyChangingEventHandler? PropertyChanging
            {
                add
                {
                    if (!_propertyChangingEventsSubscribed)
                    {
                        this.SubscribePropertyChangingEvents();
                        _propertyChangingEventsSubscribed = true;
                    }

                    PropertyChangingHandler += value;
                }
                remove => PropertyChangingHandler -= value;
            }

            /// <inheritdoc/>
            public event PropertyChangedEventHandler? PropertyChanged
            {
                add
                {
                    if (!_propertyChangedEventsSubscribed)
                    {
                        this.SubscribePropertyChangedEvents();
                        _propertyChangedEventsSubscribed = true;
                    }

                    PropertyChangedHandler += value;
                }
                remove => PropertyChangedHandler -= value;
            }

            private event PropertyChangingEventHandler? PropertyChangingHandler;

            private event PropertyChangedEventHandler? PropertyChangedHandler;

            /// <inheritdoc/>
            void IReactiveObject.RaisePropertyChanging(PropertyChangingEventArgs args) => PropertyChangingHandler?.Invoke(this, args);

            /// <inheritdoc/>
            void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChangedHandler?.Invoke(this, args);
            """);

    /// <summary>Writes the nested observable that follows one of the host's own properties.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    /// <remarks>
    /// Each change of the property switches to the observable the selector returns, as <c>WhenAnyObservable</c> does;
    /// a value is followed by selecting a <c>ReturnObservable</c> of it, as <c>WhenAnyValue</c> does.
    /// </remarks>
    internal static void WritePropertyObservable(SourceWriter writer) =>
        _ = writer.Lines("""
            private sealed class PropertyObservable<T>(INotifyPropertyChanged source, string propertyName, Func<IObservable<T>?> select) : IObservable<T>
            {
                public IDisposable Subscribe(IObserver<T> observer) => new Subscription(source, propertyName, select, observer);

                private sealed class Subscription : IDisposable
                {
                    private readonly INotifyPropertyChanged _source;
                    private readonly string _propertyName;
                    private readonly Func<IObservable<T>?> _select;
                    private readonly IObserver<T> _observer;
                    private readonly SubscriptionSlot _inner = new();

                    public Subscription(INotifyPropertyChanged source, string propertyName, Func<IObservable<T>?> select, IObserver<T> observer)
                    {
                        _source = source;
                        _propertyName = propertyName;
                        _select = select;
                        _observer = observer;
                        _source.PropertyChanged += OnPropertyChanged;
                        Switch();
                    }

                    public void Dispose()
                    {
                        _source.PropertyChanged -= OnPropertyChanged;
                        _inner.Dispose();
                    }

                    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
                    {
                        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _propertyName)
                        {
                            Switch();
                        }
                    }

                    private void Switch()
                    {
                        var inner = _select();
                        _inner.Set(inner is null ? EmptyDisposable.Instance : inner.Subscribe(new ValueObserver<T>(_observer.OnNext, _observer.OnError)));
                    }
                }
            }
            """);
}
