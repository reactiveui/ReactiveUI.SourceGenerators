// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace AvaloniaApplication1.ViewModels;

/// <summary>
/// MainViewModel.
/// </summary>
/// <seealso cref="AvaloniaApplication1.ViewModels.ViewModelBase" />
public class MainViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the greeting.
    /// </summary>
    /// <value>
    /// The greeting.
    /// </value>
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
}
