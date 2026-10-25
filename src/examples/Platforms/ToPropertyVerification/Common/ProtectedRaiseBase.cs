// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ToPropertyVerification.Common;

/// <summary>A base class whose <c>RaisePropertyChanged</c> method is <see langword="protected"/>.</summary>
[System.Diagnostics.DebuggerDisplay("ProtectedRaiseBase")]
public class ProtectedRaiseBase : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Reports that a property changed.</summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void RaisePropertyChanged(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
