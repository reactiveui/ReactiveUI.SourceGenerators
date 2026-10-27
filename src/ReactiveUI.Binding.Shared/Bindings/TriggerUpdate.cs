// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Selects the direction driven by a binding's supplied update stream.</summary>
public enum TriggerUpdate
{
    /// <summary>The stream replaces view notifications and requests writes to the view model.</summary>
    ViewToViewModel = 0,

    /// <summary>The stream requests writes to the view in place of view model notifications after the first; view notifications are still observed.</summary>
    ViewModelToView = 1,
}
