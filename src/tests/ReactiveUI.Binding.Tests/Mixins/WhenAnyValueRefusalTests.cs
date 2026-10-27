// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Covers every WhenAnyValue overload refusing a call no generated dispatch claimed.</summary>
/// <remarks>
/// A generated overload wins overload resolution for every call site of its types, so the overload it
/// displaces refuses rather than reaching for reflection behind the caller's back. Each arity declares its own
/// refusal, and the message it carries is how a caller learns which overload resolves the expression instead.
/// <para>
/// A one-argument selector is typed explicitly. Left to inference it is indistinguishable from a second
/// property expression, and the call does not compile.
/// </para>
/// </remarks>
public class WhenAnyValueRefusalTests
{
    /// <summary>The overload each refusal points the caller at.</summary>
    private const string Twin = "WhenAnyValueUnsafe";

    /// <summary>Arity 1 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity1WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                static (string v1) => v1),
            Twin);

    /// <summary>Arity 2 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity2WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                static (v1, v2) => v1),
            Twin);

    /// <summary>Arity 3 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity3WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3) => v1),
            Twin);

    /// <summary>Arity 4 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity4WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4) => v1),
            Twin);

    /// <summary>Arity 5 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity5WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5) => v1),
            Twin);

    /// <summary>Arity 6 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity6WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6) => v1),
            Twin);

    /// <summary>Arity 7 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity7WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7) => v1),
            Twin);

    /// <summary>Arity 8 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity8WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8) => v1),
            Twin);

    /// <summary>Arity 9 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity9WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9) => v1),
            Twin);

    /// <summary>Arity 10 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity10WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10) => v1),
            Twin);

    /// <summary>Arity 11 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity11WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11) => v1),
            Twin);

    /// <summary>Arity 12 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity12WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12) => v1),
            Twin);

    /// <summary>Arity 13 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity13WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13) => v1),
            Twin);

    /// <summary>Arity 14 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity14WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14) => v1),
            Twin);

    /// <summary>Arity 15 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity15WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15) => v1),
            Twin);

    /// <summary>Arity 16 with a selector refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity16WithSelector_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16) => v1),
            Twin);

    /// <summary>Arity 1 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity1_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption),
            Twin);

    /// <summary>Arity 2 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity2_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 3 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity3_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 4 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity4_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 5 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity5_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 6 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity6_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 7 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity7_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 8 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity8_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 9 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity9_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 10 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity10_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 11 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity11_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                x => x.Caption),
            Twin);

    /// <summary>Arity 12 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity12_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                x => x.Caption),
            Twin);

    /// <summary>Arity 13 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity13_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                x => x.Caption),
            Twin);

    /// <summary>Arity 14 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity14_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 15 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity15_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                x => x.Caption,
                x => x.Caption,
                x => x.Caption),
            Twin);

    /// <summary>Arity 16 refuses and names the twin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WhenAnyValue_Arity16_NamesTheUnsafeTwin() =>
        RefusalAssertions.AssertRefused(
            static () => new DispatchStubViewModel().WhenAnyValue(
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
                x => x.Caption,
                x => x.Caption,
                x => x.Caption,
                x => x.Caption),
            Twin);
}
