// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>Specifies what a generated deep-chain observer emits while its parent segment is null.</summary>
internal enum NullParentObservationBehavior
{
    /// <summary>Suppresses values until the parent becomes non-null.</summary>
    SuppressEmission = 0,

    /// <summary>Emits the leaf type's default value so binding consumers can clear their target.</summary>
    EmitDefault = 1,
}
