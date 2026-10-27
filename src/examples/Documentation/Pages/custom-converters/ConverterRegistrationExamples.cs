// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Examples of registering a converter, choosing between converters by affinity, and overriding a built-in converter.</summary>
public static class ConverterRegistrationExamples
{
    /// <summary>The affinity score for low-priority converters.</summary>
    private const int LowAffinityScore = 1;

    /// <summary>The affinity score for high-priority converters.</summary>
    private const int HighAffinityScore = 100;

    /// <summary>Registers a converter and lists what a service holds.</summary>
    public static void RegisterAndListConverters()
    {
        ConverterService service = new();

        Console.WriteLine(service.TypedConverters.GetAllConverters().Count());

        service.TypedConverters.Register(new DemoIntToStringConverter(LowAffinityScore));

        Console.WriteLine(service.TypedConverters.GetAllConverters().Count());

        // Output:
        // 0
        // 1
    }

    /// <summary>Demonstrates how converter affinity determines which converter is selected.</summary>
    public static void DemonstrateAffinitySelection()
    {
        ConverterService service = new();
        service.TypedConverters.Register(new DemoIntToStringConverter(LowAffinityScore));
        service.TypedConverters.Register(new DemoIntToStringConverter(HighAffinityScore));

        IBindingTypeConverter? resolved = service.TypedConverters.TryGetConverter(typeof(int), typeof(string));

        Console.WriteLine(resolved!.GetAffinityForObjects());

        // Output:
        // 100
    }

    /// <summary>Overrides a built-in converter with a custom one that has a higher affinity.</summary>
    public static void OverrideBuiltInConverter()
    {
        ConverterService service = new();
        DefaultConverterRegistration.RegisterDefaults(service);

        IBindingTypeConverter? builtIn = service.TypedConverters.TryGetConverter(typeof(bool), typeof(string));
        Console.WriteLine(builtIn!.GetType().Name);

        service.TypedConverters.Register(new CustomBoolToStringConverter());

        IBindingTypeConverter? resolved = service.TypedConverters.TryGetConverter(typeof(bool), typeof(string));
        bool success = resolved!.TryConvertTyped(true, null, out var result);

        Console.WriteLine(resolved.GetType().Name);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // BooleanToStringTypeConverter
        // CustomBoolToStringConverter
        // True
        // AFFIRMATIVE
    }

    /// <summary>Creates the three registries on their own, without a converter service, and registers a converter in each.</summary>
    public static void CreateRegistriesWithoutService()
    {
        BindingTypeConverterRegistry typed = new();
        BindingFallbackConverterRegistry fallback = new();
        SetMethodBindingConverterRegistry setMethod = new();

        typed.Register(new PriorityColourConverter());
        fallback.Register(new CustomFallbackConverter());
        setMethod.Register(new DemoSetMethodConverter());

        Console.WriteLine(typed.TryGetConverter(typeof(TodoPriority), typeof(string))!.GetType().Name);
        Console.WriteLine(fallback.TryGetConverter(typeof(TodoPriority), typeof(string))!.GetType().Name);
        Console.WriteLine(setMethod.TryGetConverter(typeof(string), typeof(string))!.GetType().Name);

        // Output:
        // PriorityColourConverter
        // CustomFallbackConverter
        // DemoSetMethodConverter
    }
}
