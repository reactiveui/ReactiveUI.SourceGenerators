// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.AotValidation;

/// <summary>Verifies that both signal-driven Unsafe overloads report their trimming boundary at the call site.</summary>
internal static class Program
{
    /// <summary>Compiles the two Unsafe binding forms for trimming analysis.</summary>
    private static void Main()
    {
        var model = new ProbeView();
        var view = new ProbeView();
        using var registered = view.BindUnsafe(model, x => x.Text, x => x.Text, (IObservable<int>?)null);
        using var converted = view.BindUnsafe(
            model,
            x => x.Text,
            x => x.Text,
            static value => value,
            static value => value,
            (IObservable<int>?)null,
            TriggerUpdate.ViewModelToView);
    }
}
