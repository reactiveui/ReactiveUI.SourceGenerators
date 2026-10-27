// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Identifies the UI framework base type implemented by an <c>IViewFor</c> target.</summary>
internal enum IViewForBaseType
{
    /// <summary>No supported base type was found.</summary>
    None = 0,

    /// <summary>The target is a Windows Presentation Foundation control.</summary>
    Wpf = 1,

    /// <summary>The target is a WinUI control.</summary>
    WinUI = 2,

    /// <summary>The target is a Uno Platform control.</summary>
    Uno = 3,

    /// <summary>The target is a Windows Forms control.</summary>
    WinForms = 4,

    /// <summary>The target is an Avalonia control.</summary>
    Avalonia = 5,

    /// <summary>The target is a .NET MAUI control.</summary>
    Maui = 6,
}
