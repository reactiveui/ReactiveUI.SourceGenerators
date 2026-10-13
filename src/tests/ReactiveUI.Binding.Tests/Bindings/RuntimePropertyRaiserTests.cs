// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Fallback;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Verifies runtime notification methods and optional before-change dispatch.</summary>
public sealed class RuntimePropertyRaiserTests
{
    /// <summary>Event-argument methods that return a value still deliver both cached notification arguments.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ReturningEventArgsMethods_RaiseBothNotifications()
    {
        var model = new ReturningArgsModel();
        var raiser = RuntimePropertyRaiser.Create(model, "Name");
        raiser.RaiseChanging();
        raiser.RaiseChanged();
        raiser.RaiseChanging();
        raiser.RaiseChanged();

        await Assert.That(raiser.RaisesChanging).IsTrue();
        await Assert.That(string.Join(",", model.Raised)).IsEqualTo("changing:Name,changed:Name,changing:Name,changed:Name");
        await Assert.That(ReferenceEquals(model.ChangedArgs[0], model.ChangedArgs[1])).IsTrue();
        await Assert.That(ReferenceEquals(model.ChangingArgs[0], model.ChangingArgs[1])).IsTrue();
    }

    /// <summary>Name-based methods with return values remain usable as notification methods.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ReturningNameMethods_RaiseBothNotifications()
    {
        var model = new ReturningNameModel();
        var raiser = RuntimePropertyRaiser.Create(model, "Name");
        raiser.RaiseChanging();
        raiser.RaiseChanged();

        await Assert.That(string.Join(",", model.Raised)).IsEqualTo("changing:Name,changed:Name");
    }

    /// <summary>A model supporting only after-change safely ignores before-change dispatch.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NoChangingMethod_IgnoresBeforeChange()
    {
        var model = new ToPropertyUnsafeTests.PublicArgsModel();
        var raiser = RuntimePropertyRaiser.Create(model, "Name");
        raiser.RaiseChanging();
        raiser.RaiseChanged();

        await Assert.That(raiser.RaisesChanging).IsFalse();
        await Assert.That(model.Raised).IsEquivalentTo(["Name"]);
    }

    /// <summary>Void event-argument methods bind both notification phases directly to the owner.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task VoidEventArgsMethods_RaiseBothNotifications()
    {
        var model = new VoidArgsModel();
        var raiser = RuntimePropertyRaiser.Create(model, "Name");
        raiser.RaiseChanging();
        raiser.RaiseChanged();
        raiser.RaiseChanging();
        raiser.RaiseChanged();

        await Assert.That(string.Join(",", model.Raised)).IsEqualTo("changing:Name,changed:Name,changing:Name,changed:Name");
    }

    /// <summary>Records argument-based notifications and returns the count for ordinary callers.</summary>
    public sealed class ReturningArgsModel
    {
        /// <summary>Gets the notification order.</summary>
        public List<string> Raised { get; } = [];

        /// <summary>Gets after-change arguments.</summary>
        public List<PropertyChangedEventArgs> ChangedArgs { get; } = [];

        /// <summary>Gets before-change arguments.</summary>
        public List<PropertyChangingEventArgs> ChangingArgs { get; } = [];

        /// <summary>Records an after-change notification.</summary>
        /// <param name="args">The notification arguments.</param>
        /// <returns>The number of notifications.</returns>
        public int RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            ChangedArgs.Add(args);
            Raised.Add($"changed:{args.PropertyName}");
            return Raised.Count;
        }

        /// <summary>Records a before-change notification.</summary>
        /// <param name="args">The notification arguments.</param>
        /// <returns>The number of notifications.</returns>
        public int RaisePropertyChanging(PropertyChangingEventArgs args)
        {
            ChangingArgs.Add(args);
            Raised.Add($"changing:{args.PropertyName}");
            return Raised.Count;
        }
    }

    /// <summary>Records name-based notifications with a return value.</summary>
    public sealed class ReturningNameModel
    {
        /// <summary>Gets the notification order.</summary>
        public List<string> Raised { get; } = [];

        /// <summary>Records an after-change notification.</summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The name supplied to the notification method.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string RaisePropertyChanged(string propertyName)
        {
            Raised.Add($"changed:{propertyName}");
            return propertyName;
        }

        /// <summary>Records a before-change notification.</summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The name supplied to the notification method.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string RaisePropertyChanging(string propertyName)
        {
            Raised.Add($"changing:{propertyName}");
            return propertyName;
        }
    }

    /// <summary>Records notifications through void methods taking event arguments.</summary>
    public sealed class VoidArgsModel
    {
        /// <summary>Gets the notification order.</summary>
        public List<string> Raised { get; } = [];

        /// <summary>Records an after-change notification.</summary>
        /// <param name="args">The notification arguments.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanged(PropertyChangedEventArgs args) => Raised.Add($"changed:{args.PropertyName}");

        /// <summary>Records a before-change notification.</summary>
        /// <param name="args">The notification arguments.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanging(PropertyChangingEventArgs args) => Raised.Add($"changing:{args.PropertyName}");
    }
}
