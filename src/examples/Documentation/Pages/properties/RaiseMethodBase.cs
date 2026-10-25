// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Documentation.Properties;

/// <summary>
/// A base class that raises change notifications through a public method, of the kind CommunityToolkit.Mvvm and
/// Prism declare. The generator calls <see cref="RaisePropertyChanged"/> directly, so a type derived from this one
/// does not have to be <see langword="partial"/>.
/// </summary>
public class RaiseMethodBase : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Reports that a property changed.</summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    public void RaisePropertyChanged(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
