// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Examples of creating and using custom binding converters.</summary>
public static class CustomConvertersExamples
{
    /// <summary>The title of the car registration task.</summary>
    private const string CarRegistrationTitle = "Renew car registration";

    /// <summary>The tags the customer typed, with an extra tag on the end.</summary>
    private const string TypedTags = "car, admin, urgent";

    /// <summary>The currency symbol of the customer's account.</summary>
    private const string DollarSymbol = "$";

    /// <summary>The amount of a transfer.</summary>
    private const decimal TransferAmount = 250.50M;

    /// <summary>Registers a custom TodoPriority-to-color converter and converts a priority through the service.</summary>
    public static void RegisterAndUseCustomConverter()
    {
        BindingConverters.Current.TypedConverters.Register(new TodoPriorityToColorConverter());

        IBindingTypeConverter? converter = BindingConverters.Current.TypedConverters.TryGetConverter(typeof(TodoPriority), typeof(string));

        bool success = converter!.TryConvertTyped(TodoPriority.High, null, out var highColor);
        Console.WriteLine(success);
        Console.WriteLine(highColor);

        success = converter.TryConvertTyped(TodoPriority.Low, null, out var lowColor);
        Console.WriteLine(success);
        Console.WriteLine(lowColor);

        // Output:
        // True
        // Red
        // True
        // Gray
    }

    /// <summary>Demonstrates looking up color names for all priority levels.</summary>
    public static void DisplayPriorityColors()
    {
        TodoPriorityToColorConverter converter = new TodoPriorityToColorConverter();

        foreach (TodoPriority priority in Enum.GetValues<TodoPriority>())
        {
            bool success = converter.TryConvert(priority, null, out var colorName);
            Console.WriteLine($"{priority}: {success} {colorName}");
        }

        // Output:
        // Low: True Gray
        // Normal: True Orange
        // High: True Red
    }

    /// <summary>Writes a converter from scratch by implementing <c>IBindingTypeConverter</c>, and calls it without a binding.</summary>
    public static void WriteConverterFromScratch()
    {
        PriorityColourConverter converter = new PriorityColourConverter();

        Console.WriteLine($"{converter.FromType.Name} -> {converter.ToType.Name}");
        Console.WriteLine(converter.GetAffinityForObjects());
        Console.WriteLine(converter.TryConvertTyped(TodoPriority.High, null, out var colour));
        Console.WriteLine(colour);
        Console.WriteLine(converter.TryConvertTyped("High", null, out var notAPriority));
        Console.WriteLine(notAPriority is null);

        // Output:
        // TodoPriority -> String
        // 20
        // True
        // Crimson
        // False
        // True
    }

    /// <summary>Converts the tags of a task to text and back with a pair of converters.</summary>
    public static void ConvertTagsBothWays()
    {
        TagListToTextConverter toText = new TagListToTextConverter();
        TextToTagListConverter toTags = new TextToTagListConverter();
        TodoItem item = new() { Title = CarRegistrationTitle, Tags = ["car", "admin"] };

        Console.WriteLine(toText.TryConvert(item.Tags, null, out var text));
        Console.WriteLine(text);
        Console.WriteLine(toTags.TryConvert(TypedTags, null, out var tags));
        Console.WriteLine(string.Join("|", tags!));

        // Output:
        // True
        // car, admin
        // True
        // car|admin|urgent
    }

    /// <summary>Registers a converter in a converter service of your own and resolves it by type pair.</summary>
    public static void RegisterInConverterService()
    {
        ConverterService service = new();

        service.TypedConverters.Register(new PriorityColourConverter());

        object? resolved = service.ResolveConverter(typeof(TodoPriority), typeof(string));

        Console.WriteLine(resolved!.GetType().Name);

        // Output:
        // PriorityColourConverter
    }

    /// <summary>Shows a priority as a colour name with <c>BindOneWay</c> and the converter written from scratch.</summary>
    public static void BindPriorityWithConverter()
    {
        TodoItem item = new() { Title = CarRegistrationTitle, Priority = TodoPriority.High };
        Label badge = new();

        using IDisposable binding = item.BindOneWay(badge, x => x.Priority, x => x.Text, new PriorityColourConverter());
        Console.WriteLine(badge.Text);

        item.Priority = TodoPriority.Low;
        Console.WriteLine(badge.Text);

        // Output:
        // Crimson
        // Green
    }

    /// <summary>Keeps the tags of a task and a text box in step with <c>BindTwoWay</c>, one converter for each direction.</summary>
    public static void BindTagsBothWays()
    {
        TodoItem item = new() { Title = CarRegistrationTitle, Tags = ["car", "admin"] };
        Entry tagsBox = new();

        using IDisposable binding = item.BindTwoWay(tagsBox, x => x.Tags, x => x.Text, new TagListToTextConverter(), new TextToTagListConverter());
        Console.WriteLine(tagsBox.Text);

        tagsBox.Text = TypedTags;
        Console.WriteLine(string.Join("|", item.Tags));

        // Output:
        // car, admin
        // car|admin|urgent
    }

    /// <summary>Registers a typed, a fallback and a set-method converter with the builder. Building the application makes them the shared converter service.</summary>
    public static void RegisterWithBuilder()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder
            .WithCoreServices()
            .WithConverter(new PriorityColourConverter())
            .WithFallbackConverter(new CustomFallbackConverter())
            .WithSetMethodConverter(new DemoSetMethodConverter())
            .BuildApp();

        ConverterService service = BindingConverters.Current;

        Console.WriteLine(service.TypedConverters.TryGetConverter(typeof(TodoPriority), typeof(string))!.GetType().Name);
        Console.WriteLine(service.FallbackConverters.TryGetConverter(typeof(TodoPriority), typeof(string))!.GetType().Name);
        Console.WriteLine(service.SetMethodConverters.TryGetConverter(typeof(string), typeof(string))!.GetType().Name);

        // Output:
        // PriorityColourConverter
        // CustomFallbackConverter
        // DemoSetMethodConverter
    }

    /// <summary>Writes a converter by deriving from <c>BindingTypeConverter</c> and calls it through that base class.</summary>
    public static void ConvertAmountThroughBaseClass()
    {
        BindingTypeConverter<decimal, string> converter = new CurrencyTextConverter(DollarSymbol);

        Console.WriteLine($"{converter.FromType.Name} -> {converter.ToType.Name}");
        Console.WriteLine(converter.GetAffinityForObjects());
        Console.WriteLine(converter.TryConvert(TransferAmount, conversionHint: null, out var amountText));
        Console.WriteLine(amountText);

        // Output:
        // Decimal -> String
        // 10
        // True
        // $250.50
    }

    /// <summary>Resolves a converter from a service and calls it through the untyped and the typed converter interfaces.</summary>
    public static void ConvertAmountThroughInterfaces()
    {
        ConverterService service = new();
        service.TypedConverters.Register(new CurrencyTextConverter(DollarSymbol));

        IBindingTypeConverter untyped = service.TypedConverters.TryGetConverter(typeof(decimal), typeof(string))!;
        IBindingTypeConverter<decimal, string> typed = (IBindingTypeConverter<decimal, string>)untyped;

        Console.WriteLine($"{untyped.FromType.Name} -> {untyped.ToType.Name}");
        Console.WriteLine(typed.TryConvert(TransferAmount, conversionHint: null, out var amountText));
        Console.WriteLine(amountText);

        // Output:
        // Decimal -> String
        // True
        // $250.50
    }
}
