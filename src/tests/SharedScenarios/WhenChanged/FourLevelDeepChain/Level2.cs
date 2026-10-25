// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.WhenChanged.FourLevelDeepChain;

/// <summary>Second level in the deep chain.</summary>
public class Level2 : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Model"/>.</summary>
    private Level3 _model = new Level3();

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the level 3 model.</summary>
    public Level3 Model
    {
        get => _model;
        set
        {
            if (_model == value)
            {
                return;
            }

            _model = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
        }
    }
}
