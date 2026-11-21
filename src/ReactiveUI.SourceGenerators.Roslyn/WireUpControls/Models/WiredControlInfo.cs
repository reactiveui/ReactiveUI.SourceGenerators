// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Describes a single Android view property to wire via <c>FindViewById</c>.</summary>
/// <param name="PropertyName">The property name on the declaring class.</param>
/// <param name="PropertyTypeName">The fully qualified view type name (non-nullable).</param>
/// <param name="ResourceName">The <c>Resource.Id</c> member name.</param>
internal sealed record WiredControlInfo(
    string PropertyName,
    string PropertyTypeName,
    string ResourceName);
