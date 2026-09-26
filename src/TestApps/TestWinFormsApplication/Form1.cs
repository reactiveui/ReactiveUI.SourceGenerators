// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI;

namespace WinFormsApp1;

/// <summary>Represents the primary form for the Windows Forms sample application.</summary>
/// <seealso cref="Form" />
public partial class Form1 : Form, IViewFor<MainViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="Form1"/> class.</summary>
    public Form1()
    {
        InitializeComponent();
        ViewModel = new();
    }

    /// <inheritdoc/>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public MainViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MainViewModel?)value; }
}
