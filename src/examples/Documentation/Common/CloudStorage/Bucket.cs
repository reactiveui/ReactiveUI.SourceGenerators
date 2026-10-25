// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>A named container of objects in the storage service.</summary>
/// <param name="Name">The name of the bucket, unique across the service.</param>
/// <param name="Region">The region that stores the bucket, such as <c>ap-southeast-2</c>.</param>
/// <param name="CreatedAt">When the bucket was created.</param>
[System.Diagnostics.DebuggerDisplay("Bucket: {Name} ({Region})")]
public sealed record Bucket(string Name, string Region, DateTimeOffset CreatedAt);
