// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace MauiApp1
{
    /// <summary>
    /// MainPage.
    /// </summary>
    /// <seealso cref="ContentPage" />
    [ReactiveUI.SourceGenerators.IViewFor<MainViewModel>]
    public partial class MainPage : ContentPage
    {
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            _count++;

            if (_count == 1)
            {
                CounterBtn.Text = $"Clicked {_count} time";
            }
            else
            {
                CounterBtn.Text = $"Clicked {_count} times";
            }

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
