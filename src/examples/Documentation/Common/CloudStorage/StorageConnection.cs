// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>
/// The link to the storage service. It reports a change to <see cref="State"/> through the plain
/// <see cref="StateChanged"/> event, in the style of many older libraries. It does not implement
/// <c>INotifyPropertyChanged</c>.
/// </summary>
[System.Diagnostics.DebuggerDisplay("StorageConnection: {Endpoint}: {State}")]
public sealed class StorageConnection
{
    /// <summary>Occurs after <see cref="State"/> changes.</summary>
    public event EventHandler? StateChanged;

    /// <summary>Gets the address of the service.</summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>Gets or sets the state of the link. Setting a different state raises <see cref="StateChanged"/>.</summary>
    public ConnectionState State
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
