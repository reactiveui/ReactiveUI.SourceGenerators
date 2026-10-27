// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Covers every WhenAnyObservable overload refusing a call no generated dispatch claimed.</summary>
/// <remarks>
/// A generated overload wins overload resolution for every call site of its types, so the overload it
/// displaces refuses rather than reaching for reflection behind the caller's back. Each arity declares its own
/// refusal, and the message it carries is how a caller learns which overload resolves the expression instead.
/// </remarks>
public class WhenAnyObservableRefusalTests
{
    /// <summary>The overload each refusal points the caller at.</summary>
    private const string Twin = "WhenAnyObservableUnsafe";

    /// <summary>Arity 3 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity3WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                static (v1, v2, v3) => v1),
            Twin);

    /// <summary>Arity 4 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity4WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                static (v1, v2, v3, v4) => v1),
            Twin);

    /// <summary>Arity 5 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity5WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                static (v1, v2, v3, v4, v5) => v1),
            Twin);

    /// <summary>Arity 6 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity6WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                static (v1, v2, v3, v4, v5, v6) => v1),
            Twin);

    /// <summary>Arity 7 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity7WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                static (v1, v2, v3, v4, v5, v6, v7) => v1),
            Twin);

    /// <summary>Arity 8 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity8WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                static (v1, v2, v3, v4, v5, v6, v7, v8) => v1),
            Twin);

    /// <summary>Arity 9 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity9WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9) => v1),
            Twin);

    /// <summary>Arity 10 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity10WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10) => v1),
            Twin);

    /// <summary>Arity 11 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity11WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11) => v1),
            Twin);

    /// <summary>Arity 12 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity12WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12) => v1),
            Twin);

    /// <summary>Arity 1 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity1_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal),
            Twin);

    /// <summary>Arity 2 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity2_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 3 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity3_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 4 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity4_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 5 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity5_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 6 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity6_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 7 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity7_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 8 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity8_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 9 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity9_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 10 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity10_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 11 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity11_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 12 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity12_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal,
                x => x.Signal),
            Twin);

    /// <summary>Arity 2 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyObservable_Arity2WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyObservable(
                x => x.Signal,
                x => x.Signal,
                static (v1, v2) => v1),
            Twin);
}
