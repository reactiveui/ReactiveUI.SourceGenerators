// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>Second-level object in a 4-level deep chain: ObjChain1 → ObjChain2 → ObjChain3 → HostTestFixture.</summary>
public class ObjChain2 : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the next link in the chain.</summary>
    public ObjChain3? Chain3
    {
        get => field;
        set
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Raises the PropertyChanged event.</summary>
    /// <param name="propertyName">The property name.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new(propertyName));
}
