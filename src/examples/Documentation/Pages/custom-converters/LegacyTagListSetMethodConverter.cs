// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>The set-method converter an older to-do app registered with Splat. It fills a tag list instead of replacing it.</summary>
[DebuggerDisplay("LegacyTagListSetMethodConverter: tags -> List<string> set method")]
public sealed class LegacyTagListSetMethodConverter : ISetMethodBindingConverter
{
    /// <summary>The affinity the older app gave the converter.</summary>
    private const int TagListAffinity = 5;

    /// <inheritdoc/>
    public int GetAffinityForObjects(Type? fromType, Type? toType) =>
        toType == typeof(List<string>) ? TagListAffinity : 0;

    /// <inheritdoc/>
    public object? PerformSet(object? toTarget, object? newValue, object?[]? arguments)
    {
        if (toTarget is not List<string> tagList || newValue is not IEnumerable<string> tags)
        {
            return null;
        }

        tagList.Clear();
        tagList.AddRange(tags);
        return tagList;
    }
}
