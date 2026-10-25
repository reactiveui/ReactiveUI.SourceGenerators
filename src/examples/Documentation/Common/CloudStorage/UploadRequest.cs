// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>A file the user chose to upload. The view model stores it in the current folder under its file name.</summary>
/// <param name="FileName">The name of the file, without a folder.</param>
/// <param name="SizeBytes">The size of the file in bytes.</param>
/// <param name="ContentType">The media type of the file.</param>
[System.Diagnostics.DebuggerDisplay("UploadRequest: {FileName} ({SizeBytes} bytes)")]
public sealed record UploadRequest(string FileName, long SizeBytes, string ContentType);
