// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>The three notifications an observer receives.</summary>
internal enum NotificationKind
{
    /// <summary>A value.</summary>
    Next = 0,

    /// <summary>A terminating error.</summary>
    Error = 1,

    /// <summary>Successful completion.</summary>
    Completed = 2,
}
