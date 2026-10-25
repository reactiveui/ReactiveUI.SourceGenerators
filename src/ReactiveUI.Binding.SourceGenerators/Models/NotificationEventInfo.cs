// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>Stores a validated event and its concrete delegate type.</summary>
/// <param name="Name">The event member name.</param>
/// <param name="HandlerType">The fully qualified event delegate type.</param>
internal sealed record NotificationEventInfo(string Name, string HandlerType);
