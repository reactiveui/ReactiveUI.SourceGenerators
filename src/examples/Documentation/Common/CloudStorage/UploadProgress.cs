// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>A report on how much of an upload the service has received.</summary>
/// <param name="BytesSent">The number of bytes the service has received.</param>
/// <param name="TotalBytes">The size of the whole upload in bytes.</param>
[System.Diagnostics.DebuggerDisplay("UploadProgress: {BytesSent} of {TotalBytes}")]
public sealed record UploadProgress(long BytesSent, long TotalBytes)
{
    /// <summary>Gets how much has been received as a fraction from 0 to 1.</summary>
    public double Fraction => TotalBytes == 0 ? 1 : (double)BytesSent / TotalBytes;
}
