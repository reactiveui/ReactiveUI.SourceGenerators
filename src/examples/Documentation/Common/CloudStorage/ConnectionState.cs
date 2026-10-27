// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>The state of the link to the storage service.</summary>
public enum ConnectionState
{
    /// <summary>There is no link, and requests fail.</summary>
    Disconnected = 0,

    /// <summary>The link is being opened.</summary>
    Connecting = 1,

    /// <summary>The link is open.</summary>
    Connected = 2,
}
