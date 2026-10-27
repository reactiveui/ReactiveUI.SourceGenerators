// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Setup;

/// <summary>Turns any enumeration value into its name, such as <c>High</c> for a to-do priority.</summary>
public sealed class EnumNameFallbackConverter : IBindingFallbackConverter
{
    /// <inheritdoc/>
    public int GetAffinityForObjects(Type fromType, Type toType) => fromType.IsEnum && toType == typeof(string) ? 1 : 0;

    /// <inheritdoc/>
    public bool TryConvert(Type fromType, object from, Type toType, object? conversionHint, out object? result)
    {
        result = Enum.GetName(fromType, from);
        return result is not null;
    }
}
