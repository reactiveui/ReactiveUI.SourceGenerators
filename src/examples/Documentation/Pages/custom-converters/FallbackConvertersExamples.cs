// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Examples of fallback and set-method converters.</summary>
public static class FallbackConvertersExamples
{
    /// <summary>Registers a fallback converter for runtime type conversions.</summary>
    public static void RegisterFallbackConverter()
    {
        ConverterService service = new();

        service.FallbackConverters.Register(new CustomFallbackConverter());

        Console.WriteLine(service.FallbackConverters.GetAllConverters().Count());

        // Output:
        // 1
    }

    /// <summary>Registers a set-method converter for specialized write operations.</summary>
    public static void RegisterSetMethodConverter()
    {
        ConverterService service = new();

        service.SetMethodConverters.Register(new DemoSetMethodConverter());

        Console.WriteLine(service.SetMethodConverters.GetAllConverters().Count());

        // Output:
        // 1
    }

    /// <summary>Converts a priority to text with the fallback converter, which accepts any type pair that ends in a string.</summary>
    public static void DemonstrateFallbackSelection()
    {
        ConverterService service = new();
        service.FallbackConverters.Register(new CustomFallbackConverter());

        IBindingFallbackConverter? fallback = service.FallbackConverters.TryGetConverter(typeof(TodoPriority), typeof(string));

        Console.WriteLine(fallback!.GetAffinityForObjects(typeof(TodoPriority), typeof(string)));
        Console.WriteLine(fallback.GetAffinityForObjects(typeof(TodoPriority), typeof(int)));
        Console.WriteLine(fallback.TryConvert(typeof(TodoPriority), TodoPriority.High, typeof(string), null, out var text));
        Console.WriteLine(text);

        // Output:
        // 1
        // 0
        // True
        // High
    }

    /// <summary>Resolves a converter for two type pairs: a typed converter wins over a fallback one whatever their affinities.</summary>
    public static void ResolveTypedConverterBeforeFallback()
    {
        ConverterService service = new();
        DefaultConverterRegistration.RegisterDefaults(service);
        service.FallbackConverters.Register(new CustomFallbackConverter());

        object? forInteger = service.ResolveConverter(typeof(int), typeof(string));
        object? forPriority = service.ResolveConverter(typeof(TodoPriority), typeof(string));
        object? forGuid = service.ResolveConverter(typeof(TodoPriority), typeof(Guid));

        Console.WriteLine(forInteger!.GetType().Name);
        Console.WriteLine(forPriority!.GetType().Name);
        Console.WriteLine(forGuid is null);

        // Output:
        // IntegerToStringTypeConverter
        // CustomFallbackConverter
        // True
    }

    /// <summary>Resolves the set-method converter that replaces how a binding writes to its target.</summary>
    public static void ResolveSetMethodConverter()
    {
        ConverterService service = new();
        service.SetMethodConverters.Register(new DemoSetMethodConverter());

        ISetMethodBindingConverter? converter = service.ResolveSetMethodConverter(typeof(string), typeof(string));

        Console.WriteLine(converter!.GetType().Name);
        Console.WriteLine(converter.PerformSet(null, "Renew car registration", null));

        // Output:
        // DemoSetMethodConverter
        // Renew car registration
    }

    /// <summary>Asks a set-method converter how strongly it applies; it applies only to a target that is a list of tags.</summary>
    public static void ScoreSetMethodConverter()
    {
        ConverterService service = new();
        service.SetMethodConverters.Register(new LegacyTagListSetMethodConverter());

        ISetMethodBindingConverter converter = service.ResolveSetMethodConverter(typeof(IReadOnlyList<string>), typeof(List<string>))!;

        Console.WriteLine(converter.GetAffinityForObjects(typeof(IReadOnlyList<string>), typeof(List<string>)));
        Console.WriteLine(converter.GetAffinityForObjects(typeof(string), typeof(string)));
        Console.WriteLine(converter.GetAffinityForObjects(fromType: null, toType: null));

        // Output:
        // 5
        // 0
        // 0
    }
}
