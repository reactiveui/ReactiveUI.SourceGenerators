// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Registers the built-in typed converters with a <see cref="ConverterService"/>.</summary>
public static class DefaultConverterRegistration
{
    /// <summary>Registers the string identity, equality, boolean, Guid, Uri, numeric and date and time converters, each with its nullable forms.</summary>
    /// <param name="service">The converter service to register the converters with.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
    /// <remarks>
    /// Registering twice adds every converter twice. <c>DateOnly</c> and <c>TimeOnly</c> converters are registered
    /// on .NET 8 and later only. No fallback or set-method converters are registered.
    /// </remarks>
    public static void RegisterDefaults(ConverterService service)
    {
        ArgumentExceptionHelper.ThrowIfNull(service);

        var registry = service.TypedConverters;

        RegisterCoreConverters(registry);
        RegisterNumericConverters(registry);
        RegisterTemporalConverters(registry);
    }

    /// <summary>Registers the identity, equality, boolean, guid and uri converters.</summary>
    /// <param name="registry">The registry to populate.</param>
    private static void RegisterCoreConverters(BindingTypeConverterRegistry registry)
    {
        // String identity converter
        registry.Register(new StringConverter());

        // Equality converter (last resort, affinity 1)
        registry.Register(new EqualityTypeConverter());

        // Boolean converters
        registry.Register(new BooleanToStringTypeConverter());
        registry.Register(new StringToBooleanTypeConverter());
        registry.Register(new NullableBooleanToStringTypeConverter());
        registry.Register(new StringToNullableBooleanTypeConverter());

        // Guid converters
        registry.Register(new GuidToStringTypeConverter());
        registry.Register(new StringToGuidTypeConverter());
        registry.Register(new NullableGuidToStringTypeConverter());
        registry.Register(new StringToNullableGuidTypeConverter());

        // Uri converters
        registry.Register(new UriToStringTypeConverter());
        registry.Register(new StringToUriTypeConverter());
    }

    /// <summary>Registers the converters for the numeric primitives and their nullable forms.</summary>
    /// <param name="registry">The registry to populate.</param>
    private static void RegisterNumericConverters(BindingTypeConverterRegistry registry)
    {
        // Integer converters
        registry.Register(new IntegerToStringTypeConverter());
        registry.Register(new StringToIntegerTypeConverter());
        registry.Register(new NullableIntegerToIntegerTypeConverter());
        registry.Register(new IntegerToNullableIntegerTypeConverter());
        registry.Register(new NullableIntegerToStringTypeConverter());
        registry.Register(new StringToNullableIntegerTypeConverter());

        // Double converters
        registry.Register(new DoubleToStringTypeConverter());
        registry.Register(new StringToDoubleTypeConverter());
        registry.Register(new NullableDoubleToDoubleTypeConverter());
        registry.Register(new DoubleToNullableDoubleTypeConverter());
        registry.Register(new NullableDoubleToStringTypeConverter());
        registry.Register(new StringToNullableDoubleTypeConverter());

        // Single (float) converters
        registry.Register(new SingleToStringTypeConverter());
        registry.Register(new StringToSingleTypeConverter());
        registry.Register(new NullableSingleToSingleTypeConverter());
        registry.Register(new SingleToNullableSingleTypeConverter());
        registry.Register(new NullableSingleToStringTypeConverter());
        registry.Register(new StringToNullableSingleTypeConverter());

        // Decimal converters
        registry.Register(new DecimalToStringTypeConverter());
        registry.Register(new StringToDecimalTypeConverter());
        registry.Register(new NullableDecimalToDecimalTypeConverter());
        registry.Register(new DecimalToNullableDecimalTypeConverter());
        registry.Register(new NullableDecimalToStringTypeConverter());
        registry.Register(new StringToNullableDecimalTypeConverter());

        // Long converters
        registry.Register(new LongToStringTypeConverter());
        registry.Register(new StringToLongTypeConverter());
        registry.Register(new NullableLongToLongTypeConverter());
        registry.Register(new LongToNullableLongTypeConverter());
        registry.Register(new NullableLongToStringTypeConverter());
        registry.Register(new StringToNullableLongTypeConverter());

        // Short converters
        registry.Register(new ShortToStringTypeConverter());
        registry.Register(new StringToShortTypeConverter());
        registry.Register(new NullableShortToShortTypeConverter());
        registry.Register(new ShortToNullableShortTypeConverter());
        registry.Register(new NullableShortToStringTypeConverter());
        registry.Register(new StringToNullableShortTypeConverter());

        // Byte converters
        registry.Register(new ByteToStringTypeConverter());
        registry.Register(new StringToByteTypeConverter());
        registry.Register(new NullableByteToByteTypeConverter());
        registry.Register(new ByteToNullableByteTypeConverter());
        registry.Register(new NullableByteToStringTypeConverter());
        registry.Register(new StringToNullableByteTypeConverter());
    }

    /// <summary>Registers the date, time and duration converters.</summary>
    /// <param name="registry">The registry to populate.</param>
    private static void RegisterTemporalConverters(BindingTypeConverterRegistry registry)
    {
        // DateTime converters
        registry.Register(new DateTimeToStringTypeConverter());
        registry.Register(new StringToDateTimeTypeConverter());
        registry.Register(new NullableDateTimeToStringTypeConverter());
        registry.Register(new StringToNullableDateTimeTypeConverter());

        // DateTimeOffset converters
        registry.Register(new DateTimeOffsetToStringTypeConverter());
        registry.Register(new StringToDateTimeOffsetTypeConverter());
        registry.Register(new NullableDateTimeOffsetToStringTypeConverter());
        registry.Register(new StringToNullableDateTimeOffsetTypeConverter());

        // TimeSpan converters
        registry.Register(new TimeSpanToStringTypeConverter());
        registry.Register(new StringToTimeSpanTypeConverter());
        registry.Register(new NullableTimeSpanToStringTypeConverter());
        registry.Register(new StringToNullableTimeSpanTypeConverter());

#if NET8_0_OR_GREATER
        // DateOnly converters (NET8+ only)
        registry.Register(new DateOnlyToStringTypeConverter());
        registry.Register(new StringToDateOnlyTypeConverter());
        registry.Register(new NullableDateOnlyToStringTypeConverter());
        registry.Register(new StringToNullableDateOnlyTypeConverter());

        // TimeOnly converters (NET8+ only)
        registry.Register(new TimeOnlyToStringTypeConverter());
        registry.Register(new StringToTimeOnlyTypeConverter());
        registry.Register(new NullableTimeOnlyToStringTypeConverter());
        registry.Register(new StringToNullableTimeOnlyTypeConverter());
#endif
    }
}
