// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Setup;

/// <summary>Replaces one tag of an editable tag list, so a binding can write a single indexed tag.</summary>
public sealed class TagListSetMethodConverter : ISetMethodBindingConverter
{
    /// <inheritdoc/>
    public int GetAffinityForObjects(Type? fromType, Type? toType) => fromType == typeof(List<string>) ? 1 : 0;

    /// <inheritdoc/>
    public object? PerformSet(object? toTarget, object? newValue, object?[]? arguments)
    {
        if (toTarget is not List<string> tags || newValue is not string tag || arguments is not [int index])
        {
            return null;
        }

        tags[index] = tag;
        return tag;
    }
}
