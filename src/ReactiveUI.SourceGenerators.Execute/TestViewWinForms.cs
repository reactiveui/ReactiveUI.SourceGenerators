// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides the Windows Forms test view.</summary>
/// <seealso cref="System.Windows.Forms.Form" />
public partial class TestViewWinForms : Form, IViewFor<TestViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="TestViewWinForms"/> class.</summary>
    public TestViewWinForms()
    {
        InitializeComponent();
        ViewModel = TestViewModel.Instance;
        _ = ViewModel.Activator.Activate();
    }

    /// <inheritdoc/>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TestViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (TestViewModel?)value; }
}
