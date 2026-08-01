// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace WinFormsApp1;

/// <summary>Represents the primary form for the Windows Forms sample application.</summary>
/// <seealso cref="Form" />
public partial class Form1 : Form
{
    /// <summary>Initializes a new instance of the <see cref="Form1"/> class.</summary>
    public Form1()
    {
        InitializeComponent();
        ViewModel = new();
    }
}
