// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Demonstrates boolean, GUID, and URI type converters.</summary>
public static class BooleanGuidUriExamples
{
    /// <summary>The text of a session correlation ID.</summary>
    private const string SessionCorrelationIdText = "550e8400-e29b-41d4-a716-446655440000";

    /// <summary>A unique identifier (correlation ID) for tracking user sessions.</summary>
    private static readonly Guid SessionCorrelationId = new("550e8400-e29b-41d4-a716-446655440000");

    /// <summary>A GitHub repository URL for linking to source code.</summary>
    private static readonly Uri ReactiveUiRepositoryUrl = new("https://github.com/reactiveui/ReactiveUI");

    /// <summary>Parses an enabled/disabled checkbox value from user input.</summary>
    public static void ParseFeatureToggleState()
    {
        StringToBooleanTypeConverter converter = new StringToBooleanTypeConverter();

        // Success case: feature enabled
        bool success = converter.TryConvert("True", conversionHint: null, out var isEnabled);
        Console.WriteLine(success);
        Console.WriteLine(isEnabled);

        // Failure case: invalid toggle input
        bool failure = converter.TryConvert("maybe", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // True
        // False
    }

    /// <summary>Formats a checkbox state for display in a settings panel.</summary>
    public static void FormatFeatureToggleState()
    {
        BooleanToStringTypeConverter converter = new BooleanToStringTypeConverter();

        bool success = converter.TryConvert(true, conversionHint: null, out var enabledText);
        Console.WriteLine(success);
        Console.WriteLine(enabledText);

        // Output:
        // True
        // True
    }

    /// <summary>Formats an optional feature toggle state for display (can be null/undecided).</summary>
    public static void FormatOptionalFeatureToggleState()
    {
        NullableBooleanToStringTypeConverter converter = new NullableBooleanToStringTypeConverter();

        bool success = converter.TryConvert((bool?)true, conversionHint: null, out var enabledText);
        Console.WriteLine(success);
        Console.WriteLine(enabledText);

        // Output:
        // True
        // True
    }

    /// <summary>Parses a session correlation ID from a string field.</summary>
    public static void ParseSessionCorrelationId()
    {
        StringToGuidTypeConverter converter = new StringToGuidTypeConverter();

        // Success case: valid GUID format
        bool success = converter.TryConvert("550e8400-e29b-41d4-a716-446655440000", conversionHint: null, out var correlationId);
        Console.WriteLine(success);
        Console.WriteLine(correlationId);

        // Failure case: invalid GUID format
        bool failure = converter.TryConvert("not-a-guid", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 550e8400-e29b-41d4-a716-446655440000
        // False
    }

    /// <summary>Formats a session correlation ID for logging and display.</summary>
    public static void FormatSessionCorrelationId()
    {
        GuidToStringTypeConverter converter = new GuidToStringTypeConverter();

        bool success = converter.TryConvert(SessionCorrelationId, conversionHint: null, out var correlationIdText);
        Console.WriteLine(success);
        Console.WriteLine(correlationIdText);

        // Output:
        // True
        // 550e8400-e29b-41d4-a716-446655440000
    }

    /// <summary>Parses a repository URL that a user pasted into the source link field.</summary>
    public static void ParseRepositoryUrl()
    {
        StringToUriTypeConverter converter = new StringToUriTypeConverter();

        // Success case: valid repository URL
        bool success = converter.TryConvert("https://github.com/reactiveui/ReactiveUI", conversionHint: null, out var repoUrl);
        Console.WriteLine(success);
        Console.WriteLine(repoUrl);

        // Output:
        // True
        // https://github.com/reactiveui/ReactiveUI
    }

    /// <summary>Formats a repository URL for display in a hyperlink control.</summary>
    public static void FormatRepositoryUrl()
    {
        UriToStringTypeConverter converter = new UriToStringTypeConverter();

        bool success = converter.TryConvert(ReactiveUiRepositoryUrl, conversionHint: null, out var urlText);
        Console.WriteLine(success);
        Console.WriteLine(urlText);

        // Output:
        // True
        // https://github.com/reactiveui/ReactiveUI
    }

    /// <summary>Converts a feature toggle to and from text with all four boolean converters and shows their affinity.</summary>
    public static void ConvertFeatureToggleBothWays()
    {
        StringToBooleanTypeConverter toBoolean = new StringToBooleanTypeConverter();
        _ = toBoolean.TryConvert("True", conversionHint: null, out var isEnabled);
        Console.WriteLine($"{isEnabled} affinity {toBoolean.GetAffinityForObjects()}");

        BooleanToStringTypeConverter fromBoolean = new BooleanToStringTypeConverter();
        _ = fromBoolean.TryConvert(true, conversionHint: null, out var enabledText);
        Console.WriteLine($"{enabledText} affinity {fromBoolean.GetAffinityForObjects()}");

        StringToNullableBooleanTypeConverter toOptionalBoolean = new StringToNullableBooleanTypeConverter();
        _ = toOptionalBoolean.TryConvert("False", conversionHint: null, out var isOptionalEnabled);
        Console.WriteLine($"{isOptionalEnabled} affinity {toOptionalBoolean.GetAffinityForObjects()}");

        NullableBooleanToStringTypeConverter fromOptionalBoolean = new NullableBooleanToStringTypeConverter();
        _ = fromOptionalBoolean.TryConvert((bool?)false, conversionHint: null, out var optionalEnabledText);
        Console.WriteLine($"{optionalEnabledText} affinity {fromOptionalBoolean.GetAffinityForObjects()}");

        // Output:
        // True affinity 2
        // True affinity 2
        // False affinity 2
        // False affinity 2
    }

    /// <summary>Converts a session correlation ID to and from text with all four GUID converters and shows their affinity.</summary>
    public static void ConvertSessionCorrelationIdBothWays()
    {
        StringToGuidTypeConverter toGuid = new StringToGuidTypeConverter();
        _ = toGuid.TryConvert(SessionCorrelationIdText, conversionHint: null, out var correlationId);
        Console.WriteLine($"{correlationId} affinity {toGuid.GetAffinityForObjects()}");

        GuidToStringTypeConverter fromGuid = new GuidToStringTypeConverter();
        _ = fromGuid.TryConvert(SessionCorrelationId, conversionHint: null, out var correlationIdText);
        Console.WriteLine($"{correlationIdText} affinity {fromGuid.GetAffinityForObjects()}");

        StringToNullableGuidTypeConverter toOptionalGuid = new StringToNullableGuidTypeConverter();
        _ = toOptionalGuid.TryConvert(SessionCorrelationIdText, conversionHint: null, out var optionalCorrelationId);
        Console.WriteLine($"{optionalCorrelationId} affinity {toOptionalGuid.GetAffinityForObjects()}");

        NullableGuidToStringTypeConverter fromOptionalGuid = new NullableGuidToStringTypeConverter();
        _ = fromOptionalGuid.TryConvert((Guid?)SessionCorrelationId, conversionHint: null, out var optionalCorrelationIdText);
        Console.WriteLine($"{optionalCorrelationIdText} affinity {fromOptionalGuid.GetAffinityForObjects()}");

        // Output:
        // 550e8400-e29b-41d4-a716-446655440000 affinity 2
        // 550e8400-e29b-41d4-a716-446655440000 affinity 2
        // 550e8400-e29b-41d4-a716-446655440000 affinity 2
        // 550e8400-e29b-41d4-a716-446655440000 affinity 2
    }

    /// <summary>Converts a repository URL to and from text with both URI converters and shows their affinity.</summary>
    public static void ConvertRepositoryUrlBothWays()
    {
        StringToUriTypeConverter toUri = new StringToUriTypeConverter();
        _ = toUri.TryConvert("https://github.com/reactiveui/ReactiveUI", conversionHint: null, out var repoUrl);
        Console.WriteLine($"{repoUrl} affinity {toUri.GetAffinityForObjects()}");

        UriToStringTypeConverter fromUri = new UriToStringTypeConverter();
        _ = fromUri.TryConvert(ReactiveUiRepositoryUrl, conversionHint: null, out var urlText);
        Console.WriteLine($"{urlText} affinity {fromUri.GetAffinityForObjects()}");

        // Output:
        // https://github.com/reactiveui/ReactiveUI affinity 2
        // https://github.com/reactiveui/ReactiveUI affinity 2
    }
}
