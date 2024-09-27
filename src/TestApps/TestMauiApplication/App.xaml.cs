// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace MauiApp1
{
    /// <summary>
    /// App.
    /// </summary>
    /// <seealso cref="Microsoft.Maui.Controls.Application" />
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        /// <remarks>
        /// To be added.
        /// </remarks>
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}
