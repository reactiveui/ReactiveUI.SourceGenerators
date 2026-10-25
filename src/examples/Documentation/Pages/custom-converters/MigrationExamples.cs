// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Moves the converters an older app kept in a Splat resolver into the binding converter service.</summary>
public static class MigrationExamples
{
    /// <summary>Reads the converters out of the older app's Splat resolver.</summary>
    public static void ExtractConvertersFromLegacyResolver()
    {
        using Splat.ModernDependencyResolver legacyResolver = LegacyAppDependencyResolver.Create();

        ExtractedConverters extracted = ConverterMigrationHelper.ExtractConverters(legacyResolver);

        Console.WriteLine(extracted.TypedConverters.Count);
        Console.WriteLine(extracted.FallbackConverters.Count);
        Console.WriteLine(extracted.SetMethodConverters.Count);
        Console.WriteLine(extracted.TypedConverters[0].GetType().Name);
        Console.WriteLine(extracted.FallbackConverters[0].GetType().Name);
        Console.WriteLine(extracted.SetMethodConverters[0].GetType().Name);

        // Output:
        // 1
        // 1
        // 1
        // LegacyPriorityToColorConverter
        // LegacyObjectToStringFallbackConverter
        // LegacyTagListSetMethodConverter
    }

    /// <summary>Copies the older app's converters into a converter service and uses each one.</summary>
    public static void ImportLegacyConvertersIntoService()
    {
        using Splat.ModernDependencyResolver legacyResolver = LegacyAppDependencyResolver.Create();
        ConverterService service = new();

        service.ImportFrom(legacyResolver);

        IBindingTypeConverter? typed = service.TypedConverters.TryGetConverter(typeof(TodoPriority), typeof(string));
        Console.WriteLine(typed!.TryConvertTyped(TodoPriority.High, null, out var highColour));
        Console.WriteLine(highColour);

        IBindingFallbackConverter? fallback = service.FallbackConverters.TryGetConverter(typeof(IssueState), typeof(string));
        Console.WriteLine(fallback!.TryConvert(typeof(IssueState), IssueState.Closed, typeof(string), null, out var stateText));
        Console.WriteLine(stateText);

        ISetMethodBindingConverter? setMethod = service.SetMethodConverters.TryGetConverter(typeof(IReadOnlyList<string>), typeof(List<string>));
        List<string> tagList = ["draft"];
        List<string> newTags = ["travel", "admin"];

        _ = setMethod!.PerformSet(tagList, newTags, null);

        Console.WriteLine(string.Join(", ", tagList));

        // Output:
        // True
        // #D32F2F
        // True
        // Closed
        // travel, admin
    }

    /// <summary>Binds a priority badge with the converter the older app registered, passed to the binding as the converter override.</summary>
    public static void BindToWithMigratedConverterOverride()
    {
        using Splat.ModernDependencyResolver legacyResolver = LegacyAppDependencyResolver.Create();

        BindingConverters.Current.ImportFrom(legacyResolver);

        IBindingTypeConverter? migrated = BindingConverters.Current.TypedConverters.TryGetConverter(typeof(TodoPriority), typeof(string));
        TodoItem item = new() { Title = "Renew passport", Priority = TodoPriority.High };
        Label priorityBadge = new();

        using (item.WhenChanged(x => x.Priority).BindTo(priorityBadge, x => x.Text, migrated!))
        {
            Console.WriteLine(priorityBadge.Text);

            item.Priority = TodoPriority.Low;

            Console.WriteLine(priorityBadge.Text);
        }

        // Output:
        // #D32F2F
        // #9E9E9E
    }

    /// <summary>Binds a priority badge and lets the binding find the migrated converter in the service.</summary>
    public static void BindToResolvesMigratedConverter()
    {
        using Splat.ModernDependencyResolver legacyResolver = LegacyAppDependencyResolver.Create();

        BindingConverters.Current.ImportFrom(legacyResolver);

        TodoItem item = new() { Title = "Book dentist", Priority = TodoPriority.Normal };
        Label priorityBadge = new();

        using (item.WhenChanged(x => x.Priority).BindTo(priorityBadge, x => x.Text))
        {
            Console.WriteLine(priorityBadge.Text);

            item.Priority = TodoPriority.High;

            Console.WriteLine(priorityBadge.Text);
        }

        // Output:
        // #FB8C00
        // #D32F2F
    }

    /// <summary>Groups converters into an <c>ExtractedConverters</c> yourself and takes the three groups apart again.</summary>
    public static void GroupConvertersAndDeconstruct()
    {
        List<IBindingTypeConverter> typed = [new LegacyPriorityToColorConverter()];
        List<IBindingFallbackConverter> fallback = [new LegacyObjectToStringFallbackConverter()];
        List<ISetMethodBindingConverter> setMethod = [new LegacyTagListSetMethodConverter()];
        ExtractedConverters extracted = new(typed, fallback, setMethod);

        var (typedConverters, fallbackConverters, setMethodConverters) = extracted;

        Console.WriteLine(typedConverters[0].GetType().Name);
        Console.WriteLine(fallbackConverters[0].GetType().Name);
        Console.WriteLine(setMethodConverters[0].GetType().Name);

        // Output:
        // LegacyPriorityToColorConverter
        // LegacyObjectToStringFallbackConverter
        // LegacyTagListSetMethodConverter
    }

    /// <summary>Compares extracted converters; two results are equal when they hold the same three groups.</summary>
    public static void CompareExtractedConverters()
    {
        using Splat.ModernDependencyResolver legacyResolver = LegacyAppDependencyResolver.Create();
        ExtractedConverters extracted = ConverterMigrationHelper.ExtractConverters(legacyResolver);
        ExtractedConverters same = extracted with { };
        ExtractedConverters withoutFallback = extracted with { FallbackConverters = [] };
        object boxedSame = same;

        Console.WriteLine(extracted == same);
        Console.WriteLine(extracted != withoutFallback);
        Console.WriteLine(extracted.Equals(same));
        Console.WriteLine(extracted.Equals(boxedSame));
        Console.WriteLine(extracted.GetHashCode() == same.GetHashCode());
        Console.WriteLine(extracted.ToString().StartsWith(nameof(ExtractedConverters), StringComparison.Ordinal));

        // Output:
        // True
        // True
        // True
        // True
        // True
        // True
    }
}
