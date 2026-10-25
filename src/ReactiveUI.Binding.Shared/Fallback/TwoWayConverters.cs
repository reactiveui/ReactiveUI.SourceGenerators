// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>Builds a <see cref="TwoWayConverterPair{TSourceProp,TTargetProp}"/> without naming its type arguments.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TwoWayConverters
{
    /// <summary>Pairs a forward and a reverse conversion.</summary>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="forward">Converts a source value to the target's type.</param>
    /// <param name="reverse">Converts a target value back to the source's type.</param>
    /// <returns>The paired conversions.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TwoWayConverterPair<TSourceProp, TTargetProp> Create<TSourceProp, TTargetProp>(
        Func<TSourceProp, TTargetProp> forward,
        Func<TTargetProp, TSourceProp> reverse) =>
        new(forward, reverse);
}
