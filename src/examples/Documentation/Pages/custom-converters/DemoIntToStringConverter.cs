// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Demo converter from int to string with adjustable affinity.</summary>
[DebuggerDisplay("DemoIntToStringConverter: int -> string (affinity {_affinity})")]
public sealed class DemoIntToStringConverter : IBindingTypeConverter
{
    /// <summary>The affinity score for this converter.</summary>
    private readonly int _affinity;

    /// <summary>Initializes a new instance of the <see cref="DemoIntToStringConverter"/> class.</summary>
    /// <param name="affinity">The affinity score for this converter.</param>
    public DemoIntToStringConverter(int affinity) => _affinity = affinity;

    /// <inheritdoc/>
    public Type FromType => typeof(int);

    /// <inheritdoc/>
    public Type ToType => typeof(string);

    /// <inheritdoc/>
    public int GetAffinityForObjects() => _affinity;

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        if (from is not int intValue)
        {
            result = null;
            return false;
        }

        result = intValue.ToString();
        return true;
    }
}
