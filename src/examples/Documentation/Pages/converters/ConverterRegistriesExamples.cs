// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Examples of working with converter registries and affinity.</summary>
public static class ConverterRegistriesExamples
{
    /// <summary>Reads the shared converter service before and after the application is built; building fills it with the built-in converters.</summary>
    public static void AccessConverterService()
    {
        Console.WriteLine(BindingConverters.Current.TypedConverters.GetAllConverters().Any());

        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices().BuildApp();

        IBindingTypeConverter? converter = BindingConverters.Current.TypedConverters.TryGetConverter(typeof(int), typeof(string));

        Console.WriteLine(BindingConverters.Current.TypedConverters.GetAllConverters().Any());
        Console.WriteLine(converter!.GetType().Name);

        // Output:
        // False
        // True
        // IntegerToStringTypeConverter
    }

    /// <summary>Looks up the built-in converter of several type pairs in the shared converter service.</summary>
    public static void ResolveBuiltInConverters()
    {
        BindingTypeConverterRegistry registry = BindingConverters.Current.TypedConverters;

        Console.WriteLine(registry.TryGetConverter(typeof(bool), typeof(string))!.GetType().Name);
        Console.WriteLine(registry.TryGetConverter(typeof(string), typeof(Guid))!.GetType().Name);
        Console.WriteLine(registry.TryGetConverter(typeof(DateOnly?), typeof(string))!.GetType().Name);
        Console.WriteLine(registry.TryGetConverter(typeof(TimeSpan), typeof(Guid)) is null);

        // Output:
        // BooleanToStringTypeConverter
        // StringToGuidTypeConverter
        // NullableDateOnlyToStringTypeConverter
        // True
    }
}
