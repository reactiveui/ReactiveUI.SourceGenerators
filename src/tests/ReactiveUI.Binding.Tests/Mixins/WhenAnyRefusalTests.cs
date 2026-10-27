// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Covers every WhenAny overload refusing a call no generated dispatch claimed.</summary>
/// <remarks>
/// A generated overload wins overload resolution for every call site of its types, so the overload it
/// displaces refuses rather than reaching for reflection behind the caller's back. Each arity declares its own
/// refusal, and the message it carries is how a caller learns which overload resolves the expression instead.
/// </remarks>
public class WhenAnyRefusalTests
{
    /// <summary>The overload each refusal points the caller at.</summary>
    private const string Twin = "WhenAnyUnsafe";

    /// <summary>Arity 1 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity1WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                static (v1) => v1.Value),
            Twin);

    /// <summary>Arity 2 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity2WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                static (v1, v2) => v1.Value),
            Twin);

    /// <summary>Arity 3 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity3WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3) => v1.Value),
            Twin);

    /// <summary>Arity 4 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity4WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4) => v1.Value),
            Twin);

    /// <summary>Arity 5 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity5WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5) => v1.Value),
            Twin);

    /// <summary>Arity 6 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity6WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6) => v1.Value),
            Twin);

    /// <summary>Arity 7 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity7WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7) => v1.Value),
            Twin);

    /// <summary>Arity 8 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity8WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8) => v1.Value),
            Twin);

    /// <summary>Arity 9 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity9WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9) => v1.Value),
            Twin);

    /// <summary>Arity 10 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity10WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10) => v1.Value),
            Twin);

    /// <summary>Arity 11 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity11WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11) => v1.Value),
            Twin);

    /// <summary>Arity 12 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAny_Arity12WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAny(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12) => v1.Value),
            Twin);
}
