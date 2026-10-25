// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives.Signals;
using Splat;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Checks the property context supplied to hooks by generated bindings.</summary>
[NotInParallel]
public class GeneratedBindingHookTests
{
    /// <summary>The number of segments in the nested target path.</summary>
    private const int TargetPathSegmentCount = 2;

    /// <summary>A deep target path gives a hook the owner and property of its final segment.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_DeepTarget_ReportsFinalPropertyToHook()
    {
        var model = new HookViewModel { Value = "initial" };
        var view = new HookView { ViewModel = model };
        var hook = new RecordingHook();

        ResetHooks();
        try
        {
            AppLocator.Register<IPropertyBindingHook>(() => hook);
            BindingHooks.Refresh();

            using var binding = view.OneWayBind(model, static x => x.Value, static x => x.Child.ItemsSource);

            await Assert.That(hook.SourceProperties?.Length).IsEqualTo(1);
            await Assert.That(hook.SourceProperties![0].Sender).IsSameReferenceAs(model);
            await Assert.That(hook.SourceProperties[0].GetPropertyName()).IsEqualTo(nameof(HookViewModel.Value));
            await Assert.That(hook.TargetProperties?.Length).IsEqualTo(TargetPathSegmentCount);
            await Assert.That(hook.TargetProperties![1].Sender).IsSameReferenceAs(view.Child);
            await Assert.That(hook.TargetProperties[1].GetPropertyName()).IsEqualTo(nameof(HookChild.ItemsSource));
        }
        finally
        {
            ResetHooks();
        }
    }

    /// <summary>A generated BindTo call gives hooks the target property context before writing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_DeepTarget_ReportsFinalPropertyToHook()
    {
        var view = new HookView();
        var hook = new RecordingHook();

        ResetHooks();
        try
        {
            AppLocator.Register<IPropertyBindingHook>(() => hook);
            BindingHooks.Refresh();

            using var binding = Signal.Emit("updated")
                .BindTo(view, static x => x.Child.ItemsSource);

            await Assert.That(hook.TargetProperties?.Length).IsEqualTo(TargetPathSegmentCount);
            await Assert.That(hook.TargetProperties![1].Sender).IsSameReferenceAs(view.Child);
            await Assert.That(hook.TargetProperties[1].GetPropertyName()).IsEqualTo(nameof(HookChild.ItemsSource));
        }
        finally
        {
            ResetHooks();
        }
    }

    /// <summary>Removes every hook registration and refreshes the hook cache.</summary>
    private static void ResetHooks()
    {
        AppLocator.UnregisterAll<IPropertyBindingHook>();
        BindingHooks.Refresh();
    }

    /// <summary>A notifying view model with one bound property.</summary>
    public sealed class HookViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the source value.</summary>
        public string? Value
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Value)));
            }
        }
    }

    /// <summary>A view with a nested target property.</summary>
    public sealed class HookView : IViewFor<HookViewModel>, INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets the nested target.</summary>
        public HookChild Child { get; } = new();

        /// <summary>Gets or sets the model displayed by the view.</summary>
        public HookViewModel? ViewModel
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(ViewModel)));
            }
        }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (HookViewModel?)value;
        }
    }

    /// <summary>The owner of the final property in the target path.</summary>
    public sealed class HookChild : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the bound target value.</summary>
        public string? ItemsSource
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(ItemsSource)));
            }
        }
    }

    /// <summary>Records the change arrays requested by a generated binding.</summary>
    private sealed class RecordingHook : IPropertyBindingHook
    {
        /// <summary>Gets the last source property changes read by a hook.</summary>
        public IObservedChange<object, object>[]? SourceProperties { get; private set; }

        /// <summary>Gets the last target property changes read by a hook.</summary>
        public IObservedChange<object, object>[]? TargetProperties { get; private set; }

        /// <inheritdoc/>
        public bool ExecuteHook(
            object? source,
            object target,
            Func<IObservedChange<object, object>[]> getCurrentViewModelProperties,
            Func<IObservedChange<object, object>[]> getCurrentViewProperties,
            BindingDirection direction)
        {
            SourceProperties = getCurrentViewModelProperties();
            TargetProperties = getCurrentViewProperties();
            return true;
        }
    }
}
