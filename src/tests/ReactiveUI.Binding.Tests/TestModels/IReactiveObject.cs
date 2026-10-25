// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI;

/// <summary>Stands in for ReactiveUI's interface, which <c>ToPropertyUnsafe</c> finds by its full name.</summary>
public interface IReactiveObject
{
    /// <summary>Raises the after-change notification.</summary>
    /// <param name="args">The event args.</param>
    void RaisePropertyChanged(PropertyChangedEventArgs args);
}
