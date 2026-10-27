// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Headless;

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>Runs test bodies on the UI thread of one headless Avalonia session shared by the whole test run.</summary>
/// <remarks>
/// Avalonia initialises its platform once per process, so the session is created on first use and never disposed.
/// Its dispatcher runs on a pumped UI thread, so posted work runs and <c>CheckAccess</c> answers for that thread.
/// </remarks>
internal static class AvaloniaTestSession
{
    /// <summary>The session, created on first use.</summary>
    private static readonly Lazy<HeadlessUnitTestSession> Session =
        new(static () => HeadlessUnitTestSession.StartNew(typeof(Application), AvaloniaTestIsolationLevel.PerAssembly), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>Runs <paramref name="body"/> on the session's UI thread.</summary>
    /// <param name="body">The test body.</param>
    /// <returns>A task that completes when <paramref name="body"/> has finished.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Task Run(Func<Task> body) =>
        Session.Value.Dispatch(
            async () =>
            {
                await body();
                return true;
            },
            CancellationToken.None);
}
