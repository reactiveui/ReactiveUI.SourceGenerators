// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.CodeGeneration;

/// <summary>The members both Windows Forms hosts share, written through a <see cref="SourceWriter"/>.</summary>
/// <remarks>
/// A host follows its own properties through its <c>PropertyChanged</c> event, never through <c>WhenAny</c>: the
/// binding engine's generator cannot see call sites in this generator's output, so a <c>WhenAny</c> in a host would not
/// be dispatched.
/// </remarks>
internal static class ControlHostWriter
{
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
