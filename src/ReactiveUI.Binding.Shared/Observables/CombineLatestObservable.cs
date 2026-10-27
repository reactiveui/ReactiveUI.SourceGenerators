// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Creates an observable that combines the latest value of each of two to sixteen sources.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
public static class CombineLatestObservable
{
    /// <summary>Combines the latest values from two observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        Func<T1, T2, TResult> resultSelector) =>
        new CombineLatestSignal<T1, T2, TResult>(source1, source2, resultSelector);

    /// <summary>Combines the latest values from three observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        Func<T1, T2, T3, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(source1, source2, source3, resultSelector);

    /// <summary>Combines the latest values from four observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        Func<T1, T2, T3, T4, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(source1, source2, source3, source4, resultSelector);

    /// <summary>Combines the latest values from five observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, T5, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        Func<T1, T2, T3, T4, T5, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            resultSelector);

    /// <summary>Combines the latest values from six observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, T5, T6, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        Func<T1, T2, T3, T4, T5, T6, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            resultSelector);

    /// <summary>Combines the latest values from seven observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="T7">Source element type 7.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="source7">Source observable 7.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [SuppressMessage("Design", "SST1472:Signatures should not declare too many parameters", Justification = "Parameter count is intrinsic to the fixed CombineLatest arity.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, T5, T6, T7, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            resultSelector);

    /// <summary>Combines the latest values from eight observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="T7">Source element type 7.</typeparam>
    /// <typeparam name="T8">Source element type 8.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="source7">Source observable 7.</param>
    /// <param name="source8">Source observable 8.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [SuppressMessage("Design", "SST1472:Signatures should not declare too many parameters", Justification = "Parameter count is intrinsic to the fixed CombineLatest arity.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            resultSelector);

    /// <summary>Combines the latest values from nine observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="T7">Source element type 7.</typeparam>
    /// <typeparam name="T8">Source element type 8.</typeparam>
    /// <typeparam name="T9">Source element type 9.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="source7">Source observable 7.</param>
    /// <param name="source8">Source observable 8.</param>
    /// <param name="source9">Source observable 9.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [SuppressMessage("Design", "SST1472:Signatures should not declare too many parameters", Justification = "Parameter count is intrinsic to the fixed CombineLatest arity.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        IObservable<T9> source9,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            resultSelector);

    /// <summary>Combines the latest values from ten observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="T7">Source element type 7.</typeparam>
    /// <typeparam name="T8">Source element type 8.</typeparam>
    /// <typeparam name="T9">Source element type 9.</typeparam>
    /// <typeparam name="T10">Source element type 10.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="source7">Source observable 7.</param>
    /// <param name="source8">Source observable 8.</param>
    /// <param name="source9">Source observable 9.</param>
    /// <param name="source10">Source observable 10.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [SuppressMessage("Design", "SST1472:Signatures should not declare too many parameters", Justification = "Parameter count is intrinsic to the fixed CombineLatest arity.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        IObservable<T9> source9,
        IObservable<T10> source10,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            resultSelector);

    /// <summary>Combines the latest values from eleven observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="T7">Source element type 7.</typeparam>
    /// <typeparam name="T8">Source element type 8.</typeparam>
    /// <typeparam name="T9">Source element type 9.</typeparam>
    /// <typeparam name="T10">Source element type 10.</typeparam>
    /// <typeparam name="T11">Source element type 11.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="source7">Source observable 7.</param>
    /// <param name="source8">Source observable 8.</param>
    /// <param name="source9">Source observable 9.</param>
    /// <param name="source10">Source observable 10.</param>
    /// <param name="source11">Source observable 11.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [SuppressMessage("Design", "SST1472:Signatures should not declare too many parameters", Justification = "Parameter count is intrinsic to the fixed CombineLatest arity.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        IObservable<T9> source9,
        IObservable<T10> source10,
        IObservable<T11> source11,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            resultSelector);

    /// <summary>Combines the latest values from twelve observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="T7">Source element type 7.</typeparam>
    /// <typeparam name="T8">Source element type 8.</typeparam>
    /// <typeparam name="T9">Source element type 9.</typeparam>
    /// <typeparam name="T10">Source element type 10.</typeparam>
    /// <typeparam name="T11">Source element type 11.</typeparam>
    /// <typeparam name="T12">Source element type 12.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="source7">Source observable 7.</param>
    /// <param name="source8">Source observable 8.</param>
    /// <param name="source9">Source observable 9.</param>
    /// <param name="source10">Source observable 10.</param>
    /// <param name="source11">Source observable 11.</param>
    /// <param name="source12">Source observable 12.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [SuppressMessage("Design", "SST1472:Signatures should not declare too many parameters", Justification = "Parameter count is intrinsic to the fixed CombineLatest arity.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        IObservable<T9> source9,
        IObservable<T10> source10,
        IObservable<T11> source11,
        IObservable<T12> source12,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            resultSelector);

    /// <summary>Combines the latest values from thirteen observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="T7">Source element type 7.</typeparam>
    /// <typeparam name="T8">Source element type 8.</typeparam>
    /// <typeparam name="T9">Source element type 9.</typeparam>
    /// <typeparam name="T10">Source element type 10.</typeparam>
    /// <typeparam name="T11">Source element type 11.</typeparam>
    /// <typeparam name="T12">Source element type 12.</typeparam>
    /// <typeparam name="T13">Source element type 13.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="source7">Source observable 7.</param>
    /// <param name="source8">Source observable 8.</param>
    /// <param name="source9">Source observable 9.</param>
    /// <param name="source10">Source observable 10.</param>
    /// <param name="source11">Source observable 11.</param>
    /// <param name="source12">Source observable 12.</param>
    /// <param name="source13">Source observable 13.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [SuppressMessage("Design", "SST1472:Signatures should not declare too many parameters", Justification = "Parameter count is intrinsic to the fixed CombineLatest arity.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        IObservable<T9> source9,
        IObservable<T10> source10,
        IObservable<T11> source11,
        IObservable<T12> source12,
        IObservable<T13> source13,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            resultSelector);

    /// <summary>Combines the latest values from fourteen observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="T7">Source element type 7.</typeparam>
    /// <typeparam name="T8">Source element type 8.</typeparam>
    /// <typeparam name="T9">Source element type 9.</typeparam>
    /// <typeparam name="T10">Source element type 10.</typeparam>
    /// <typeparam name="T11">Source element type 11.</typeparam>
    /// <typeparam name="T12">Source element type 12.</typeparam>
    /// <typeparam name="T13">Source element type 13.</typeparam>
    /// <typeparam name="T14">Source element type 14.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="source7">Source observable 7.</param>
    /// <param name="source8">Source observable 8.</param>
    /// <param name="source9">Source observable 9.</param>
    /// <param name="source10">Source observable 10.</param>
    /// <param name="source11">Source observable 11.</param>
    /// <param name="source12">Source observable 12.</param>
    /// <param name="source13">Source observable 13.</param>
    /// <param name="source14">Source observable 14.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [SuppressMessage("Design", "SST1472:Signatures should not declare too many parameters", Justification = "Parameter count is intrinsic to the fixed CombineLatest arity.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        IObservable<T9> source9,
        IObservable<T10> source10,
        IObservable<T11> source11,
        IObservable<T12> source12,
        IObservable<T13> source13,
        IObservable<T14> source14,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            resultSelector);

    /// <summary>Combines the latest values from fifteen observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="T7">Source element type 7.</typeparam>
    /// <typeparam name="T8">Source element type 8.</typeparam>
    /// <typeparam name="T9">Source element type 9.</typeparam>
    /// <typeparam name="T10">Source element type 10.</typeparam>
    /// <typeparam name="T11">Source element type 11.</typeparam>
    /// <typeparam name="T12">Source element type 12.</typeparam>
    /// <typeparam name="T13">Source element type 13.</typeparam>
    /// <typeparam name="T14">Source element type 14.</typeparam>
    /// <typeparam name="T15">Source element type 15.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="source7">Source observable 7.</param>
    /// <param name="source8">Source observable 8.</param>
    /// <param name="source9">Source observable 9.</param>
    /// <param name="source10">Source observable 10.</param>
    /// <param name="source11">Source observable 11.</param>
    /// <param name="source12">Source observable 12.</param>
    /// <param name="source13">Source observable 13.</param>
    /// <param name="source14">Source observable 14.</param>
    /// <param name="source15">Source observable 15.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [SuppressMessage("Design", "SST1472:Signatures should not declare too many parameters", Justification = "Parameter count is intrinsic to the fixed CombineLatest arity.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15,
        TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        IObservable<T9> source9,
        IObservable<T10> source10,
        IObservable<T11> source11,
        IObservable<T12> source12,
        IObservable<T13> source13,
        IObservable<T14> source14,
        IObservable<T15> source15,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            source15,
            resultSelector);

    /// <summary>Combines the latest values from sixteen observables using a result selector.</summary>
    /// <typeparam name="T1">Source element type 1.</typeparam>
    /// <typeparam name="T2">Source element type 2.</typeparam>
    /// <typeparam name="T3">Source element type 3.</typeparam>
    /// <typeparam name="T4">Source element type 4.</typeparam>
    /// <typeparam name="T5">Source element type 5.</typeparam>
    /// <typeparam name="T6">Source element type 6.</typeparam>
    /// <typeparam name="T7">Source element type 7.</typeparam>
    /// <typeparam name="T8">Source element type 8.</typeparam>
    /// <typeparam name="T9">Source element type 9.</typeparam>
    /// <typeparam name="T10">Source element type 10.</typeparam>
    /// <typeparam name="T11">Source element type 11.</typeparam>
    /// <typeparam name="T12">Source element type 12.</typeparam>
    /// <typeparam name="T13">Source element type 13.</typeparam>
    /// <typeparam name="T14">Source element type 14.</typeparam>
    /// <typeparam name="T15">Source element type 15.</typeparam>
    /// <typeparam name="T16">Source element type 16.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source1">Source observable 1.</param>
    /// <param name="source2">Source observable 2.</param>
    /// <param name="source3">Source observable 3.</param>
    /// <param name="source4">Source observable 4.</param>
    /// <param name="source5">Source observable 5.</param>
    /// <param name="source6">Source observable 6.</param>
    /// <param name="source7">Source observable 7.</param>
    /// <param name="source8">Source observable 8.</param>
    /// <param name="source9">Source observable 9.</param>
    /// <param name="source10">Source observable 10.</param>
    /// <param name="source11">Source observable 11.</param>
    /// <param name="source12">Source observable 12.</param>
    /// <param name="source13">Source observable 13.</param>
    /// <param name="source14">Source observable 14.</param>
    /// <param name="source15">Source observable 15.</param>
    /// <param name="source16">Source observable 16.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    /// <returns>An observable that emits once every source has produced a value, then again on each later value from any source.</returns>
    [SuppressMessage("Design", "SST1472:Signatures should not declare too many parameters", Justification = "Parameter count is intrinsic to the fixed CombineLatest arity.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TResult> Create<
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15,
        T16,
        TResult>(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        IObservable<T9> source9,
        IObservable<T10> source10,
        IObservable<T11> source11,
        IObservable<T12> source12,
        IObservable<T13> source13,
        IObservable<T14> source14,
        IObservable<T15> source15,
        IObservable<T16> source16,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> resultSelector) =>
        LinqExtensions.CombineLatest(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            source15,
            source16,
            resultSelector);
}
