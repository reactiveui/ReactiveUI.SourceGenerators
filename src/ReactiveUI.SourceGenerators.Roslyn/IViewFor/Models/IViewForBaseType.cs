// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Identifies the UI framework base type implemented by an <c>IViewFor</c> target.</summary>
internal enum IViewForBaseType
{
    /// <summary>No supported base type was found.</summary>
    None,
    /// <summary>The target is a Windows Presentation Foundation control.</summary>
    Wpf,
    /// <summary>The target is a WinUI control.</summary>
    WinUI,
    /// <summary>The target is a Uno Platform control.</summary>
    Uno,
    /// <summary>The target is a Windows Forms control.</summary>
    WinForms,
    /// <summary>The target is an Avalonia control.</summary>
    Avalonia,
    /// <summary>The target is a .NET MAUI control.</summary>
    Maui,
}
