// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>
/// An S3-style storage service that lives in memory. <see cref="Latency"/> makes every request, and every part of
/// an upload, take time, so an example decides how long each step lasts. <see cref="ThrottleNext"/> and
/// <see cref="Disconnect"/> make the service refuse requests.
/// </summary>
[System.Diagnostics.DebuggerDisplay("InMemoryObjectStorage: Connection = {Connection.State}, Throttled = {_throttled}")]
public sealed class InMemoryObjectStorage : IObjectStorage
{
    /// <summary>The size of one part of an upload, 4 MiB.</summary>
    private const long PartSizeBytes = 4_194_304;

    /// <summary>The size of the launch banner, about 2.3 MiB.</summary>
    private const long BannerSize = 2_411_724;

    /// <summary>The size of the offsite photo, about 5.3 MiB.</summary>
    private const long OffsitePhotoSize = 5_562_368;

    /// <summary>The size of the warehouse photo, about 1.6 MiB.</summary>
    private const long WarehousePhotoSize = 1_677_722;

    /// <summary>The size of the product tour video, about 148 MiB.</summary>
    private const long ProductTourSize = 155_189_248;

    /// <summary>The size of the landing page, 12 KiB.</summary>
    private const long LandingPageSize = 12_288;

    /// <summary>The size of a nightly database dump, about 96 MiB.</summary>
    private const long DatabaseDumpSize = 100_663_296;

    /// <summary>The media type of a JPEG photo.</summary>
    private const string JpegType = "image/jpeg";

    /// <summary>The media type of a gzip archive.</summary>
    private const string GzipType = "application/gzip";

    /// <summary>The name of the bucket that holds the marketing files.</summary>
    private const string MediaBucket = "acme-media";

    /// <summary>The name of the bucket that holds the backups.</summary>
    private const string BackupsBucket = "acme-backups";

    /// <summary>The buckets of the account.</summary>
    private readonly List<Bucket> _buckets = [];

    /// <summary>The objects of each bucket, by bucket name, ordered by key.</summary>
    private readonly Dictionary<string, List<StorageObject>> _objects = [];

    /// <summary>The number of following requests the service refuses.</summary>
    private int _throttled;

    /// <summary>The number that makes each stored object's tag unique.</summary>
    private int _version;

    /// <summary>Gets or sets how long each request, and each part of an upload, takes. Zero yields once and carries on.</summary>
    public TimeSpan Latency { get; set; }

    /// <inheritdoc/>
    public StorageConnection Connection { get; } = new() { Endpoint = "https://s3.ap-southeast-2.storage.example", State = ConnectionState.Connected };

    /// <summary>Creates a service with two buckets and seven objects, already connected.</summary>
    /// <returns>A new service.</returns>
    public static InMemoryObjectStorage CreateSeeded()
    {
        InMemoryObjectStorage storage = new();
        storage.AddBucket(MediaBucket, "ap-southeast-2", new(2025, 6, 12, 8, 30, 0, TimeSpan.Zero));
        storage.Seed(MediaBucket, "index.html", LandingPageSize, "text/html", new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero));
        storage.Seed(MediaBucket, "photos/2025/warehouse.jpg", WarehousePhotoSize, JpegType, new(2025, 11, 20, 16, 45, 0, TimeSpan.Zero));
        storage.Seed(MediaBucket, "photos/2026/launch-banner.png", BannerSize, "image/png", new(2026, 2, 10, 9, 12, 0, TimeSpan.Zero));
        storage.Seed(MediaBucket, "photos/2026/team-offsite.jpg", OffsitePhotoSize, JpegType, new(2026, 2, 27, 18, 30, 0, TimeSpan.Zero));
        storage.Seed(MediaBucket, "videos/product-tour.mp4", ProductTourSize, "video/mp4", new(2026, 1, 30, 13, 5, 0, TimeSpan.Zero));
        storage.AddBucket(BackupsBucket, "us-east-1", new(2024, 9, 1, 0, 0, 0, TimeSpan.Zero));
        storage.Seed(BackupsBucket, "db/2026-03-01.sql.gz", DatabaseDumpSize, GzipType, new(2026, 3, 1, 2, 0, 0, TimeSpan.Zero));
        storage.Seed(BackupsBucket, "db/2026-03-02.sql.gz", DatabaseDumpSize, GzipType, new(2026, 3, 2, 2, 0, 0, TimeSpan.Zero));
        return storage;
    }

    /// <inheritdoc/>
    public async Task ConnectAsync()
    {
        Connection.State = ConnectionState.Connecting;
        await PauseAsync().ConfigureAwait(false);
        Connection.State = ConnectionState.Connected;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Bucket>> ListBucketsAsync()
    {
        await EnterAsync().ConfigureAwait(false);

        return _buckets.ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StorageObject>> ListObjectsAsync(string bucket, string prefix)
    {
        await EnterAsync().ConfigureAwait(false);

        List<StorageObject> matches = [];
        foreach (StorageObject stored in ObjectsOf(bucket))
        {
            if (stored.Key.StartsWith(prefix, StringComparison.Ordinal))
            {
                matches.Add(stored.Clone());
            }
        }

        return matches;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<UploadProgress> UploadAsync(string bucket, string key, long sizeBytes, string contentType)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizeBytes);

        return UploadPartsAsync(bucket, key, sizeBytes, contentType);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string bucket, string key)
    {
        await EnterAsync().ConfigureAwait(false);

        if (ObjectsOf(bucket).RemoveAll(existing => existing.Key == key) == 0)
        {
            throw new StorageException(StorageFailure.NoSuchKey, $"The key '{key}' does not exist in bucket '{bucket}'.");
        }
    }

    /// <summary>Makes the service refuse the next requests with <see cref="StorageFailure.SlowDown"/>.</summary>
    /// <param name="requests">How many requests to refuse.</param>
    public void ThrottleNext(int requests) => _throttled = requests;

    /// <summary>Drops the link. Requests fail with <see cref="StorageFailure.ConnectionLost"/> until <see cref="ConnectAsync"/> completes.</summary>
    public void Disconnect() => Connection.State = ConnectionState.Disconnected;

    /// <summary>Sends an object one part at a time, then stores it.</summary>
    /// <param name="bucket">The name of the bucket.</param>
    /// <param name="key">The key to store the object under.</param>
    /// <param name="sizeBytes">The size of the object in bytes.</param>
    /// <param name="contentType">The media type of the object.</param>
    /// <returns>A report after each part the service has received.</returns>
    /// <exception cref="StorageException">The bucket does not exist, the service throttles the request or the link is down.</exception>
    private async IAsyncEnumerable<UploadProgress> UploadPartsAsync(string bucket, string key, long sizeBytes, string contentType)
    {
        await EnterAsync().ConfigureAwait(false);

        List<StorageObject> stored = ObjectsOf(bucket);
        long sent = 0;
        while (sent < sizeBytes)
        {
            await PauseAsync().ConfigureAwait(false);
            EnsureConnected();
            sent = Math.Min(sent + PartSizeBytes, sizeBytes);
            yield return new(sent, sizeBytes);
        }

        StorageObject uploaded = new() { Key = key, Size = sizeBytes, LastModified = new(2026, 3, 3, 9, 0, 0, TimeSpan.Zero), ContentType = contentType, ETag = NextETag() };

        _ = stored.RemoveAll(existing => existing.Key == key);
        stored.Add(uploaded);
        stored.Sort(static (left, right) => string.CompareOrdinal(left.Key, right.Key));
    }

    /// <summary>Waits for <see cref="Latency"/>, then refuses the request when the link is down or the service is throttling.</summary>
    /// <returns>A task that completes when the request may proceed.</returns>
    /// <exception cref="StorageException">The link is down or the service is throttling.</exception>
    private async Task EnterAsync()
    {
        await PauseAsync().ConfigureAwait(false);
        EnsureConnected();

        if (_throttled == 0)
        {
            return;
        }

        _throttled--;
        throw new StorageException(StorageFailure.SlowDown, "Please reduce your request rate.");
    }

    /// <summary>Waits for <see cref="Latency"/>, or yields once when it is zero.</summary>
    /// <returns>A task that completes when the wait is over.</returns>
    private async Task PauseAsync()
    {
        if (Latency == TimeSpan.Zero)
        {
            await Task.Yield();
            return;
        }

        await Task.Delay(Latency).ConfigureAwait(false);
    }

    /// <summary>Refuses the request when the link is down.</summary>
    /// <exception cref="StorageException">The link is down.</exception>
    private void EnsureConnected()
    {
        if (Connection.State != ConnectionState.Connected)
        {
            throw new StorageException(StorageFailure.ConnectionLost, "The connection to the storage service was lost.");
        }
    }

    /// <summary>Finds the objects of a bucket.</summary>
    /// <param name="bucket">The name of the bucket.</param>
    /// <returns>The stored objects, ordered by key.</returns>
    /// <exception cref="StorageException">The bucket does not exist.</exception>
    private List<StorageObject> ObjectsOf(string bucket) =>
        _objects.TryGetValue(bucket, out var stored)
            ? stored
            : throw new StorageException(StorageFailure.NoSuchBucket, $"The bucket '{bucket}' does not exist.");

    /// <summary>Creates the tag of the next stored object.</summary>
    /// <returns>A tag that no other stored object has.</returns>
    private string NextETag()
    {
        _version++;
        return $"etag-{_version.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>Adds a bucket without waiting.</summary>
    /// <param name="name">The name of the bucket.</param>
    /// <param name="region">The region of the bucket.</param>
    /// <param name="createdAt">When the bucket was created.</param>
    private void AddBucket(string name, string region, DateTimeOffset createdAt)
    {
        _buckets.Add(new(name, region, createdAt));
        _objects[name] = [];
    }

    /// <summary>Adds an object without waiting.</summary>
    /// <param name="bucket">The name of the bucket.</param>
    /// <param name="key">The key of the object.</param>
    /// <param name="size">The size of the object in bytes.</param>
    /// <param name="contentType">The media type of the object.</param>
    /// <param name="lastModified">When the object was written.</param>
    private void Seed(string bucket, string key, long size, string contentType, DateTimeOffset lastModified) =>
        _objects[bucket].Add(new() { Key = key, Size = size, LastModified = lastModified, ContentType = contentType, ETag = NextETag() });
}
