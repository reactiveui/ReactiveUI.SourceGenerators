// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>The part of an S3-style object storage service that the example app calls.</summary>
public interface IObjectStorage
{
    /// <summary>Gets the link to the service.</summary>
    StorageConnection Connection { get; }

    /// <summary>Opens the link. <see cref="Connection"/> moves to <see cref="ConnectionState.Connecting"/> and then to <see cref="ConnectionState.Connected"/>.</summary>
    /// <returns>A task that completes when the link is open.</returns>
    Task ConnectAsync();

    /// <summary>Lists the buckets of the account.</summary>
    /// <returns>The buckets, by name.</returns>
    /// <exception cref="StorageException">The service throttles the request (<see cref="StorageFailure.SlowDown"/>) or the link is down (<see cref="StorageFailure.ConnectionLost"/>).</exception>
    Task<IReadOnlyList<Bucket>> ListBucketsAsync();

    /// <summary>Lists the objects of a bucket whose key starts with a prefix.</summary>
    /// <param name="bucket">The name of the bucket.</param>
    /// <param name="prefix">The start of the keys to list; empty lists everything.</param>
    /// <returns>Copies of the objects, by key.</returns>
    /// <exception cref="StorageException">The bucket does not exist, the service throttles the request or the link is down.</exception>
    Task<IReadOnlyList<StorageObject>> ListObjectsAsync(string bucket, string prefix);

    /// <summary>Uploads an object in parts. The object is stored once the sequence has been read to its end.</summary>
    /// <param name="bucket">The name of the bucket.</param>
    /// <param name="key">The key to store the object under; an existing object with that key is replaced.</param>
    /// <param name="sizeBytes">The size of the object in bytes.</param>
    /// <param name="contentType">The media type of the object.</param>
    /// <returns>A report after each part the service has received.</returns>
    /// <exception cref="StorageException">The bucket does not exist, the service throttles the request or the link is down.</exception>
    IAsyncEnumerable<UploadProgress> UploadAsync(string bucket, string key, long sizeBytes, string contentType);

    /// <summary>Deletes an object.</summary>
    /// <param name="bucket">The name of the bucket.</param>
    /// <param name="key">The key of the object.</param>
    /// <returns>A task that completes when the object is deleted.</returns>
    /// <exception cref="StorageException">The bucket or the object does not exist, the service throttles the request or the link is down.</exception>
    Task DeleteAsync(string bucket, string key);
}
