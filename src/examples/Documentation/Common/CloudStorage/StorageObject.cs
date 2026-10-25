// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>
/// A file held in a bucket, shaped like the response types of a cloud SDK. It is a plain class: it does not
/// implement <c>INotifyPropertyChanged</c> and raises no event when a property changes, so nothing tells an observer
/// that a property changed.
/// </summary>
[System.Diagnostics.DebuggerDisplay("StorageObject: {Key} ({Size} bytes)")]
public sealed class StorageObject
{
    /// <summary>Gets or sets the full name of the object inside its bucket, such as <c>photos/2026/launch.png</c>.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the size of the object in bytes.</summary>
    public long Size { get; set; }

    /// <summary>Gets or sets when the object was last written.</summary>
    public DateTimeOffset LastModified { get; set; }

    /// <summary>Gets or sets the media type of the object, such as <c>image/png</c>.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the tag that changes whenever the content changes.</summary>
    public string ETag { get; set; } = string.Empty;

    /// <summary>Creates an independent copy, as the service returns a new object for each response.</summary>
    /// <returns>A copy with the same values.</returns>
    public StorageObject Clone() => new() { Key = Key, Size = Size, LastModified = LastModified, ContentType = ContentType, ETag = ETag };
}
