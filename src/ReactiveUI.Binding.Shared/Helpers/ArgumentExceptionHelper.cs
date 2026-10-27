// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Helpers;
#else
namespace ReactiveUI.Binding.Helpers;
#endif

/// <summary>
/// Provides helper methods for argument validation.
/// These methods serve as polyfills for ArgumentNullException.ThrowIfNull and related methods
/// that are only available in newer .NET versions.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class ArgumentExceptionHelper
{
    /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    internal static void ThrowIfNull(
        [ValidatedNotNull][NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
    {
        if (argument is not null)
        {
            return;
        }

        throw new ArgumentNullException(paramName);
    }

    /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    internal static void ThrowIfNullWithMessage(
        [ValidatedNotNull][NotNull] object? argument,
        string message,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
    {
        if (argument is not null)
        {
            return;
        }

        throw new ArgumentNullException(paramName, message);
    }

    /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
    internal static void ThrowIfNullOrEmpty(
        [ValidatedNotNull][NotNull] string? argument,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (argument.Length != 0)
        {
            return;
        }

        throw new ArgumentException("The value cannot be an empty string.", paramName);
    }

    /// <summary>Throws an exception if <paramref name="argument"/> is null, empty, or consists only of white-space characters.</summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
    internal static void ThrowIfNullOrWhiteSpace(
        [ValidatedNotNull][NotNull] string? argument,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (!string.IsNullOrWhiteSpace(argument))
        {
            return;
        }

        throw new ArgumentException(
            "The value cannot be an empty string or composed entirely of whitespace.",
            paramName);
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.</summary>
    /// <param name="value">The argument to validate as non-negative.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
    internal static void ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value >= 0)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(paramName, "The value cannot be negative.");
    }

    /// <summary>Throws an <see cref="ArgumentException"/> if <paramref name="condition"/> is true.</summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="condition"/> corresponds.</param>
    /// <exception cref="ArgumentException"><paramref name="condition"/> is true.</exception>
    internal static void ThrowIf(
        [DoesNotReturnIf(true)] bool condition,
        string message,
        [CallerArgumentExpression(nameof(condition))]
        string? paramName = null)
    {
        if (!condition)
        {
            return;
        }

        throw new ArgumentException(message, paramName);
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero.</summary>
    /// <param name="value">The argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero.</exception>
    internal static void ThrowIfZero(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value != 0)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(paramName, "The value cannot be zero.");
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.</summary>
    /// <param name="value">The argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative or zero.</exception>
    internal static void ThrowIfNegativeOrZero(
        int value,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
    {
        if (value > 0)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(paramName, "The value cannot be negative or zero.");
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.</summary>
    /// <typeparam name="T">The type of the values to compare.</typeparam>
    /// <param name="value">The argument to validate.</param>
    /// <param name="other">The value to compare with.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is equal to <paramref name="other"/>.</exception>
    internal static void ThrowIfEqual<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
        where T : IEquatable<T>
    {
        if (!value.Equals(other))
        {
            return;
        }

        throw new ArgumentOutOfRangeException(paramName, $"The value cannot be equal to {other}.");
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.</summary>
    /// <typeparam name="T">The type of the values to compare.</typeparam>
    /// <param name="value">The argument to validate.</param>
    /// <param name="other">The value to compare with.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not equal to <paramref name="other"/>.</exception>
    internal static void ThrowIfNotEqual<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
        where T : IEquatable<T>
    {
        if (value.Equals(other))
        {
            return;
        }

        throw new ArgumentOutOfRangeException(paramName, $"The value must be equal to {other}.");
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.</summary>
    /// <typeparam name="T">The type of the values to compare.</typeparam>
    /// <param name="value">The argument to validate.</param>
    /// <param name="other">The value to compare with.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than <paramref name="other"/>.</exception>
    internal static void ThrowIfGreaterThan<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(other) <= 0)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(paramName, $"The value cannot be greater than {other}.");
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal to <paramref name="other"/>.</summary>
    /// <typeparam name="T">The type of the values to compare.</typeparam>
    /// <param name="value">The argument to validate.</param>
    /// <param name="other">The value to compare with.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than or equal to <paramref name="other"/>.</exception>
    internal static void ThrowIfGreaterThanOrEqual<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(other) < 0)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(paramName, $"The value cannot be greater than or equal to {other}.");
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.</summary>
    /// <typeparam name="T">The type of the values to compare.</typeparam>
    /// <param name="value">The argument to validate.</param>
    /// <param name="other">The value to compare with.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than <paramref name="other"/>.</exception>
    internal static void ThrowIfLessThan<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(other) >= 0)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(paramName, $"The value cannot be less than {other}.");
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal to <paramref name="other"/>.</summary>
    /// <typeparam name="T">The type of the values to compare.</typeparam>
    /// <param name="value">The argument to validate.</param>
    /// <param name="other">The value to compare with.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than or equal to <paramref name="other"/>.</exception>
    internal static void ThrowIfLessThanOrEqual<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(other) > 0)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(paramName, $"The value cannot be less than or equal to {other}.");
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal to <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate.</param>
    /// <param name="other">The value to compare with.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than or equal to <paramref name="other"/>.</exception>
    internal static void ThrowIfLessThanOrEqual(
        int value,
        int other,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
    {
        if (value > other)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(paramName, $"The value cannot be less than or equal to {other}.");
    }

    /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is default.</summary>
    /// <typeparam name="T">The struct type.</typeparam>
    /// <param name="argument">The argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is the default value of <typeparamref name="T"/>.</exception>
    internal static void ThrowIfDefault<T>(
        T argument,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
        where T : struct
    {
        if (!EqualityComparer<T>.Default.Equals(argument, default))
        {
            return;
        }

        throw new ArgumentNullException(paramName);
    }
}
