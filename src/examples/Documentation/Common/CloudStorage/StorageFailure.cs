// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>Why the storage service refused a request.</summary>
public enum StorageFailure
{
    /// <summary>The service is asking the caller to slow down and try again.</summary>
    SlowDown = 0,

    /// <summary>The bucket does not exist.</summary>
    NoSuchBucket = 1,

    /// <summary>The object does not exist.</summary>
    NoSuchKey = 2,

    /// <summary>The link to the service is down.</summary>
    ConnectionLost = 3,
}
