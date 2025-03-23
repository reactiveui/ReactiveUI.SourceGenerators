// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// A base class for handling test setup and teardown.
/// </summary>
/// <typeparam name="T">Type of Incremental Generator.</typeparam>
/// <seealso cref="Xunit.IAsyncLifetime" />
/// <seealso cref="System.IDisposable" />
/// <param name="testOutputHelper">The output helper.</param>
public abstract class TestBase<T>(ITestOutputHelper testOutputHelper) : IAsyncLifetime, IDisposable
        where T : IIncrementalGenerator, new()
{
    /// <summary>
    /// Gets the TestHelper instance.
    /// </summary>
    protected TestHelper<T> TestHelper { get; } = new(testOutputHelper);

    /// <inheritdoc/>
    public Task InitializeAsync() => TestHelper.InitializeAsync();

    /// <inheritdoc/>
    public Task DisposeAsync()
    {
        TestHelper.Dispose();
        return Task.CompletedTask;
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
