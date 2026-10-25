// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Receives values from the generated nullable and numeric-string converters.</summary>
public sealed class TypedAdapterTarget
{
    /// <summary>Gets or sets the nullable numeric destination.</summary>
    public int? Number
    {
        get => field;
        set
        {
            field = value;
            NumberWrites++;
        }
    }

    /// <summary>Gets or sets the formatted numeric destination.</summary>
    public string Text
    {
        get => field;
        set
        {
            field = value;
            TextWrites++;
        }
    } = string.Empty;

    /// <summary>Gets the number of numeric writes actually delivered.</summary>
    public long NumberWrites { get; private set; }

    /// <summary>Gets the number of formatted writes actually delivered.</summary>
    public long TextWrites { get; private set; }
}
