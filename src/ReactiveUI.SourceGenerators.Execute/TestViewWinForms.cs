// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test
{
    /// <summary>
    /// TestViewWinForms.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    [IViewFor<TestViewModel>(RegistrationType = SplatRegistrationType.LazySingleton)]
    public partial class TestViewWinForms : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestViewWinForms"/> class.
        /// </summary>
        public TestViewWinForms()
        {
            InitializeComponent();
            ViewModel = TestViewModel.Instance;
            ViewModel.Activator.Activate();
        }
    }
}
