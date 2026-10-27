// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>Stores the native members inspected for a command binding.</summary>
/// <param name="Kind">The native route.</param>
/// <param name="EventName">The concrete event, when the route uses one.</param>
/// <param name="EventArgsType">The event argument type.</param>
/// <param name="HasEnabled">Whether the native owner exposes a writable Enabled property.</param>
/// <param name="HasAction">Whether the target/action route exposes a writable Action property.</param>
internal sealed record NativeCommandInfo(NativeCommandKind Kind, string? EventName, string? EventArgsType, bool HasEnabled, bool HasAction);
