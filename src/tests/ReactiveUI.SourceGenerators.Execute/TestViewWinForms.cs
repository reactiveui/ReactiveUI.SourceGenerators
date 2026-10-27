// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides the Windows Forms test view.</summary>
/// <seealso cref="System.Windows.Forms.Form" />
[IViewFor<TestViewModel>]
public partial class TestViewWinForms : Form
{
    /// <summary>Initializes a new instance of the <see cref="TestViewWinForms"/> class.</summary>
    public TestViewWinForms()
    {
        InitializeComponent();
        ViewModel = TestViewModel.Instance;
        _ = ViewModel.Activator.Activate();
    }
}
