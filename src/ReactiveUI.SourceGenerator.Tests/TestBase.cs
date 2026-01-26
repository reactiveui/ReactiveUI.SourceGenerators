// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// A base class for handling test setup and teardown.
/// </summary>
/// <typeparam name="T">Type of Incremental Generator.</typeparam>
/// <seealso cref="System.IDisposable" />
public abstract class TestBase<T> : IDisposable
        where T : IIncrementalGenerator, new()
{
    /// <summary>
    /// Gets the TestHelper instance.
    /// </summary>
    protected TestHelper<T> TestHelper { get; } = new();

    /// <summary>
    /// Initializes the test helper asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OneTimeSetUp]
    public Task InitializeAsync() => TestHelper.InitializeAsync();

    /// <summary>
    /// Disposes the test helper.
    /// </summary>
    [OneTimeTearDown]
    public void DisposeAsync()
    {
        TestHelper.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the TestBase.
    /// </summary>
    /// <param name="isDisposing">True if called from Dispose method, false if called from finalizer.</param>
    protected virtual void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            TestHelper.Dispose();
        }
    }
}
