// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>A base class for handling test setup and teardown.</summary>
/// <typeparam name="T">Type of Incremental Generator.</typeparam>
/// <seealso cref="System.IDisposable" />
public class TestBase<T> : IDisposable
        where T : IIncrementalGenerator, new()
{
    /// <summary>Gets the TestHelper instance.</summary>
    protected TestHelper<T> TestHelper { get; } = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes the resources used by the TestBase.</summary>
    /// <param name="isDisposing">True if called from Dispose method, false if called from finalizer.</param>
    protected virtual void Dispose(bool isDisposing)
    {
        if (!isDisposing)
        {
            return;
        }

        TestHelper.Dispose();
    }
}
