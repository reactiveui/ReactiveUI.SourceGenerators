// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>
/// The view model behind the storage browser. It lists buckets and the objects under a prefix, uploads files
/// and follows the state of the link. Each command starts the matching <c>...Async</c> method and returns; await
/// the method to wait for the work. A failed request never throws from either. It sets <see cref="ErrorMessage"/>.
/// </summary>
[System.Diagnostics.DebuggerDisplay("StorageBrowserViewModel: Bucket = {SelectedBucket}, Objects = {Objects.Count}, UploadPercent = {UploadPercent}")]
public sealed class StorageBrowserViewModel : ObservableObject
{
    /// <summary>The value of <see cref="UploadPercent"/> when the upload is complete.</summary>
    private const double FullPercent = 100;

    /// <summary>The service the view model calls.</summary>
    private readonly IObjectStorage _storage;

    /// <summary>Initializes a new instance of the <see cref="StorageBrowserViewModel"/> class.</summary>
    /// <param name="storage">The service to call.</param>
    public StorageBrowserViewModel(IObjectStorage storage)
    {
        _storage = storage;
        LoadBucketsCommand = new(() => _ = LoadBucketsAsync());
        RefreshCommand = new(() => _ = LoadObjectsAsync(), () => SelectedBucket is not null);
        UploadCommand = new(request => _ = UploadAsync(request!), request => request is not null && SelectedBucket is not null);
        ConnectCommand = new(() => _ = ConnectAsync(), () => ConnectionStatus == ConnectionState.Disconnected);
        ConnectionStatus = storage.Connection.State;
        storage.Connection.StateChanged += OnConnectionStateChanged;
    }

    /// <summary>Gets the command that runs <see cref="LoadBucketsAsync"/>.</summary>
    public Command LoadBucketsCommand { get; }

    /// <summary>Gets the command that runs <see cref="LoadObjectsAsync"/> while a bucket is selected.</summary>
    public Command RefreshCommand { get; }

    /// <summary>Gets the command that runs <see cref="UploadAsync"/> for its <see cref="UploadRequest"/> parameter while a bucket is selected.</summary>
    public Command<UploadRequest> UploadCommand { get; }

    /// <summary>Gets the command that runs <see cref="ConnectAsync"/> while <see cref="ConnectionStatus"/> is <see cref="ConnectionState.Disconnected"/>.</summary>
    public Command ConnectCommand { get; }

    /// <summary>Gets the link to the service. It is a plain class that raises only <see cref="StorageConnection.StateChanged"/>.</summary>
    public StorageConnection Connection => _storage.Connection;

    /// <summary>Gets the state of the link, kept in step with <see cref="Connection"/>.</summary>
    public ConnectionState ConnectionStatus
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            ConnectCommand.ChangeCanExecute();
        }
    }

    /// <summary>Gets the buckets of the account.</summary>
    public IReadOnlyList<Bucket> Buckets
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the bucket the browser shows.</summary>
    public Bucket? SelectedBucket
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            RefreshCommand.ChangeCanExecute();
            UploadCommand.ChangeCanExecute();
        }
    }

    /// <summary>Gets or sets the folder the browser lists, such as <c>photos/2026/</c>. Empty lists the whole bucket.</summary>
    public string CurrentPrefix
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets the objects of the selected bucket under <see cref="CurrentPrefix"/>.</summary>
    public IReadOnlyList<StorageObject> Objects
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the object the user picked. <see cref="StorageObject"/> raises no change notifications.</summary>
    public StorageObject? SelectedObject
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the combined size in bytes of <see cref="Objects"/>.</summary>
    public long TotalSize
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets a value indicating whether an upload is running.</summary>
    public bool IsUploading
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets how much of the running upload the service has received, from 0 to 100.</summary>
    public double UploadPercent
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the message from the last failed request, or an empty string.</summary>
    public string ErrorMessage
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Lists the buckets.</summary>
    /// <returns>A task that completes when the buckets are listed or the request failed.</returns>
    public async Task LoadBucketsAsync()
    {
        try
        {
            Buckets = await _storage.ListBucketsAsync().ConfigureAwait(false);
            ErrorMessage = string.Empty;
        }
        catch (StorageException ex)
        {
            ErrorMessage = $"{ex.Failure}: {ex.Message}";
        }
    }

    /// <summary>Lists the objects of the selected bucket under the current prefix.</summary>
    /// <returns>A task that completes when the objects are listed or the request failed.</returns>
    public async Task LoadObjectsAsync()
    {
        if (SelectedBucket is not { } bucket)
        {
            return;
        }

        try
        {
            IReadOnlyList<StorageObject> objects = await _storage.ListObjectsAsync(bucket.Name, CurrentPrefix).ConfigureAwait(false);
            ErrorMessage = string.Empty;
            Objects = objects;
            SelectedObject = null;
            TotalSize = SumSizes(objects);
        }
        catch (StorageException ex)
        {
            ErrorMessage = $"{ex.Failure}: {ex.Message}";
        }
    }

    /// <summary>Uploads a file to the selected bucket, then lists the objects again.</summary>
    /// <param name="request">The file to upload.</param>
    /// <returns>A task that completes when the file is stored or the request failed.</returns>
    public async Task UploadAsync(UploadRequest request)
    {
        if (SelectedBucket is not { } bucket)
        {
            return;
        }

        UploadPercent = 0;
        IsUploading = true;

        try
        {
            await foreach (UploadProgress report in _storage.UploadAsync(bucket.Name, CurrentPrefix + request.FileName, request.SizeBytes, request.ContentType).ConfigureAwait(false))
            {
                UploadPercent = report.Fraction * FullPercent;
            }

            ErrorMessage = string.Empty;
            await LoadObjectsAsync().ConfigureAwait(false);
        }
        catch (StorageException ex)
        {
            ErrorMessage = $"{ex.Failure}: {ex.Message}";
        }
        finally
        {
            IsUploading = false;
        }
    }

    /// <summary>Reopens the link to the service.</summary>
    /// <returns>A task that completes when the link is open.</returns>
    public Task ConnectAsync() => _storage.ConnectAsync();

    /// <summary>Adds up the sizes of objects.</summary>
    /// <param name="objects">The objects to add up.</param>
    /// <returns>The combined size in bytes.</returns>
    private static long SumSizes(IReadOnlyList<StorageObject> objects) => objects.Sum(static stored => stored.Size);

    /// <summary>Mirrors the state of the link.</summary>
    /// <param name="sender">The connection.</param>
    /// <param name="e">The event data.</param>
    private void OnConnectionStateChanged(object? sender, EventArgs e) => ConnectionStatus = _storage.Connection.State;
}
