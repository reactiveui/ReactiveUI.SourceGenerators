// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>Reads current values on the delivery thread and compares after conversion.</summary>
/// <typeparam name="TVMProp">The view model property type.</typeparam>
/// <typeparam name="TVProp">The view property type.</typeparam>
/// <param name="viewModel">The view model root.</param>
/// <param name="view">The view root.</param>
/// <param name="modelChain">The cached view model path.</param>
/// <param name="viewChain">The cached view path.</param>
/// <param name="conversions">Explicit conversions, or null for converter registration.</param>
internal sealed class TriggeredBindingPlan<TVMProp, TVProp>(
    object viewModel,
    object view,
    List<Expression> modelChain,
    List<Expression> viewChain,
    TwoWayConverterPair<TVMProp, TVProp>? conversions)
{
    /// <summary>Whether the serialized projection has initialized the view from the model.</summary>
    private bool _initialized;

    /// <summary>Drops unavailable, unconvertible and equal values before a write.</summary>
    /// <param name="fromViewModel">Whether the signal came from the view model side.</param>
    /// <returns>A converted value and its direction when a write is needed.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    internal (bool HasValue, BindingChange Value) Project(bool fromViewModel)
    {
        if (!Reflection.TryGetValueForPropertyChain<TVMProp>(out var modelValue, viewModel, modelChain)
            || !Reflection.TryGetValueForPropertyChain<TVProp>(out var viewValue, view, viewChain))
        {
            return default;
        }

        if (!_initialized)
        {
            fromViewModel = true;
            _initialized = true;
        }

        if (fromViewModel)
        {
            TVProp converted;
            if (conversions is not null)
            {
                converted = conversions.Forward(modelValue);
            }
            else if (!RuntimeBindingConverter.TryConvert(modelValue, null, null, out converted))
            {
                return default;
            }

            return (!EqualityComparer<TVProp>.Default.Equals(converted, viewValue), new(converted, true));
        }

        TVMProp reverse;
        if (conversions is not null)
        {
            reverse = conversions.Reverse(viewValue);
        }
        else if (!RuntimeBindingConverter.TryConvert(viewValue, null, null, out reverse))
        {
            return default;
        }

        return (!EqualityComparer<TVMProp>.Default.Equals(reverse, modelValue), new(reverse, false));
    }

    /// <summary>Writes to the side opposite the signal's source.</summary>
    /// <param name="change">The converted value and its source side.</param>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Write(BindingChange change) =>
        _ = Reflection.TrySetValueToPropertyChain(
            change.FromViewModel ? view : viewModel,
            change.FromViewModel ? viewChain : modelChain,
            change.Value);
}
