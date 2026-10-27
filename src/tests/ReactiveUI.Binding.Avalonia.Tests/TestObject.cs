// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>An Avalonia object with two registered properties and one plain CLR property.</summary>
public sealed class TestObject : AvaloniaObject
{
    /// <summary>The registered property behind <see cref="Text"/>.</summary>
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<TestObject, string?>(nameof(Text));

    /// <summary>The registered property behind <see cref="Count"/>.</summary>
    public static readonly StyledProperty<int> CountProperty = AvaloniaProperty.Register<TestObject, int>(nameof(Count));

    /// <summary>Gets or sets a registered string property.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Gets or sets a registered integer property.</summary>
    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>Gets or sets a CLR property that registers no Avalonia property.</summary>
    public string? Plain { get; set; }
}
