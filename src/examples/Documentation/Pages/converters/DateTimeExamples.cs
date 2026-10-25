// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Demonstrates date and time type converters.</summary>
public static class DateTimeExamples
{
    /// <summary>The text of a todo due date and time.</summary>
    private const string DueDateTimeText = "2025-12-25T10:30:00";

    /// <summary>The text of a file modification time with its timezone offset.</summary>
    private const string ModifiedTimeText = "2025-09-21T10:30:00+00:00";

    /// <summary>The text of a todo due date.</summary>
    private const string DueDateText = "2025-12-25";

    /// <summary>The text of an appointment start time.</summary>
    private const string StartTimeText = "14:30:00";

    /// <summary>The text of a project duration.</summary>
    private const string DurationText = "01:30:00";

    /// <summary>A todo item's due date and time for task scheduling.</summary>
    private static readonly DateTime TodoDueDateTime = new(2025, 12, 25, 10, 30, 0, DateTimeKind.Unspecified);

    /// <summary>A cloud storage file's last modification time with timezone information.</summary>
    private static readonly DateTimeOffset FileLastModifiedTime = new(2025, 9, 21, 10, 30, 0, TimeSpan.Zero);

    /// <summary>A todo item's due date in a task management app.</summary>
    private static readonly DateOnly TodoDueDate = new(2025, 12, 25);

    /// <summary>An appointment's start time in a calendar application.</summary>
    private static readonly TimeOnly AppointmentStartTime = new(14, 30, 0);

    /// <summary>A project's estimated duration for completion.</summary>
    private static readonly TimeSpan ProjectDuration = new(1, 30, 0);

    /// <summary>Parses a todo item's due date and time that a user typed into the task app.</summary>
    public static void ParseTodoDueDateTime()
    {
        StringToDateTimeTypeConverter converter = new StringToDateTimeTypeConverter();

        // Success case: valid date-time
        bool success = converter.TryConvert("2025-12-25T10:30:00", conversionHint: null, out var dueDateTime);
        Console.WriteLine(success);
        Console.WriteLine(dueDateTime);

        // Failure case: invalid date format
        bool failure = converter.TryConvert("not-a-date", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 12/25/2025 10:30:00
        // False
    }

    /// <summary>Formats a todo item's due date-time for display in a task list.</summary>
    public static void FormatTodoDueDateTime()
    {
        DateTimeToStringTypeConverter converter = new DateTimeToStringTypeConverter();

        bool success = converter.TryConvert(TodoDueDateTime, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 12/25/2025 10:30:00
    }

    /// <summary>Parses a cloud storage file's last modified time that includes timezone information.</summary>
    public static void ParseCloudStorageModificationTime()
    {
        StringToDateTimeOffsetTypeConverter converter = new StringToDateTimeOffsetTypeConverter();

        // Success case: valid date-time with timezone
        bool success = converter.TryConvert("2025-09-21T10:30:00+00:00", conversionHint: null, out var modifiedTime);
        Console.WriteLine(success);
        Console.WriteLine(modifiedTime);

        // Failure case: invalid timezone format
        bool failure = converter.TryConvert("invalid-date-time", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 09/21/2025 10:30:00 +00:00
        // False
    }

    /// <summary>Formats a cloud storage file's modification time for display.</summary>
    public static void FormatCloudStorageModificationTime()
    {
        DateTimeOffsetToStringTypeConverter converter = new DateTimeOffsetToStringTypeConverter();

        bool success = converter.TryConvert(FileLastModifiedTime, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 09/21/2025 10:30:00 +00:00
    }

    /// <summary>Parses a todo item's due date that a user typed in the date picker.</summary>
    public static void ParseTodoDueDate()
    {
        StringToDateOnlyTypeConverter converter = new StringToDateOnlyTypeConverter();

        // Success case: valid date
        bool success = converter.TryConvert("2025-12-25", conversionHint: null, out var dueDate);
        Console.WriteLine(success);
        Console.WriteLine(dueDate);

        // Failure case: invalid date
        bool failure = converter.TryConvert("32/13/2025", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 12/25/2025
        // False
    }

    /// <summary>Formats a todo item's due date for display in the task list.</summary>
    public static void FormatTodoDueDate()
    {
        DateOnlyToStringTypeConverter converter = new DateOnlyToStringTypeConverter();

        bool success = converter.TryConvert(TodoDueDate, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 12/25/2025
    }

    /// <summary>Parses an appointment start time from a calendar text field.</summary>
    public static void ParseAppointmentStartTime()
    {
        StringToTimeOnlyTypeConverter converter = new StringToTimeOnlyTypeConverter();

        // Success case: valid time
        bool success = converter.TryConvert("14:30:00", conversionHint: null, out var startTime);
        Console.WriteLine(success);
        Console.WriteLine(startTime);

        // Failure case: invalid time format
        bool failure = converter.TryConvert("25:00:00", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 14:30
        // False
    }

    /// <summary>Formats an appointment start time for display in a calendar event.</summary>
    public static void FormatAppointmentStartTime()
    {
        TimeOnlyToStringTypeConverter converter = new TimeOnlyToStringTypeConverter();

        bool success = converter.TryConvert(AppointmentStartTime, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 14:30
    }

    /// <summary>Parses a project duration that a user typed into a task estimate field.</summary>
    public static void ParseProjectDuration()
    {
        StringToTimeSpanTypeConverter converter = new StringToTimeSpanTypeConverter();

        // Success case: valid duration
        bool success = converter.TryConvert("01:30:00", conversionHint: null, out var duration);
        Console.WriteLine(success);
        Console.WriteLine(duration);

        // Failure case: invalid duration format
        bool failure = converter.TryConvert("not-a-duration", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 01:30:00
        // False
    }

    /// <summary>Formats a project duration for display in task management views.</summary>
    public static void FormatProjectDuration()
    {
        TimeSpanToStringTypeConverter converter = new TimeSpanToStringTypeConverter();

        bool success = converter.TryConvert(ProjectDuration, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 01:30:00
    }

    /// <summary>Converts a todo due date and time to and from text with all four DateTime converters and shows their affinity.</summary>
    public static void ConvertTodoDueDateTimeBothWays()
    {
        StringToDateTimeTypeConverter toDateTime = new StringToDateTimeTypeConverter();
        _ = toDateTime.TryConvert(DueDateTimeText, conversionHint: null, out var dueDateTime);
        Console.WriteLine($"{dueDateTime} affinity {toDateTime.GetAffinityForObjects()}");

        DateTimeToStringTypeConverter fromDateTime = new DateTimeToStringTypeConverter();
        _ = fromDateTime.TryConvert(TodoDueDateTime, conversionHint: null, out var dueDateTimeText);
        Console.WriteLine($"{dueDateTimeText} affinity {fromDateTime.GetAffinityForObjects()}");

        StringToNullableDateTimeTypeConverter toOptionalDateTime = new StringToNullableDateTimeTypeConverter();
        _ = toOptionalDateTime.TryConvert(DueDateTimeText, conversionHint: null, out var optionalDueDateTime);
        Console.WriteLine($"{optionalDueDateTime} affinity {toOptionalDateTime.GetAffinityForObjects()}");

        NullableDateTimeToStringTypeConverter fromOptionalDateTime = new NullableDateTimeToStringTypeConverter();
        _ = fromOptionalDateTime.TryConvert((DateTime?)TodoDueDateTime, conversionHint: null, out var optionalDueDateTimeText);
        Console.WriteLine($"{optionalDueDateTimeText} affinity {fromOptionalDateTime.GetAffinityForObjects()}");

        // Output:
        // 12/25/2025 10:30:00 affinity 2
        // 12/25/2025 10:30:00 affinity 2
        // 12/25/2025 10:30:00 affinity 2
        // 12/25/2025 10:30:00 affinity 2
    }

    /// <summary>Converts a file modification time to and from text with all four DateTimeOffset converters and shows their affinity.</summary>
    public static void ConvertCloudStorageModificationTimeBothWays()
    {
        StringToDateTimeOffsetTypeConverter toOffset = new StringToDateTimeOffsetTypeConverter();
        _ = toOffset.TryConvert(ModifiedTimeText, conversionHint: null, out var modifiedTime);
        Console.WriteLine($"{modifiedTime} affinity {toOffset.GetAffinityForObjects()}");

        DateTimeOffsetToStringTypeConverter fromOffset = new DateTimeOffsetToStringTypeConverter();
        _ = fromOffset.TryConvert(FileLastModifiedTime, conversionHint: null, out var modifiedTimeText);
        Console.WriteLine($"{modifiedTimeText} affinity {fromOffset.GetAffinityForObjects()}");

        StringToNullableDateTimeOffsetTypeConverter toOptionalOffset = new StringToNullableDateTimeOffsetTypeConverter();
        _ = toOptionalOffset.TryConvert(ModifiedTimeText, conversionHint: null, out var optionalModifiedTime);
        Console.WriteLine($"{optionalModifiedTime} affinity {toOptionalOffset.GetAffinityForObjects()}");

        NullableDateTimeOffsetToStringTypeConverter fromOptionalOffset = new NullableDateTimeOffsetToStringTypeConverter();
        _ = fromOptionalOffset.TryConvert((DateTimeOffset?)FileLastModifiedTime, conversionHint: null, out var optionalModifiedTimeText);
        Console.WriteLine($"{optionalModifiedTimeText} affinity {fromOptionalOffset.GetAffinityForObjects()}");

        // Output:
        // 09/21/2025 10:30:00 +00:00 affinity 2
        // 09/21/2025 10:30:00 +00:00 affinity 2
        // 09/21/2025 10:30:00 +00:00 affinity 2
        // 09/21/2025 10:30:00 +00:00 affinity 2
    }

    /// <summary>Converts a todo due date to and from text with all four DateOnly converters and shows their affinity.</summary>
    public static void ConvertTodoDueDateBothWays()
    {
        StringToDateOnlyTypeConverter toDate = new StringToDateOnlyTypeConverter();
        _ = toDate.TryConvert(DueDateText, conversionHint: null, out var dueDate);
        Console.WriteLine($"{dueDate} affinity {toDate.GetAffinityForObjects()}");

        DateOnlyToStringTypeConverter fromDate = new DateOnlyToStringTypeConverter();
        _ = fromDate.TryConvert(TodoDueDate, conversionHint: null, out var dueDateText);
        Console.WriteLine($"{dueDateText} affinity {fromDate.GetAffinityForObjects()}");

        StringToNullableDateOnlyTypeConverter toOptionalDate = new StringToNullableDateOnlyTypeConverter();
        _ = toOptionalDate.TryConvert(DueDateText, conversionHint: null, out var optionalDueDate);
        Console.WriteLine($"{optionalDueDate} affinity {toOptionalDate.GetAffinityForObjects()}");

        NullableDateOnlyToStringTypeConverter fromOptionalDate = new NullableDateOnlyToStringTypeConverter();
        _ = fromOptionalDate.TryConvert((DateOnly?)TodoDueDate, conversionHint: null, out var optionalDueDateText);
        Console.WriteLine($"{optionalDueDateText} affinity {fromOptionalDate.GetAffinityForObjects()}");

        // Output:
        // 12/25/2025 affinity 2
        // 12/25/2025 affinity 2
        // 12/25/2025 affinity 2
        // 12/25/2025 affinity 2
    }

    /// <summary>Converts an appointment start time to and from text with all four TimeOnly converters and shows their affinity.</summary>
    public static void ConvertAppointmentStartTimeBothWays()
    {
        StringToTimeOnlyTypeConverter toTime = new StringToTimeOnlyTypeConverter();
        _ = toTime.TryConvert(StartTimeText, conversionHint: null, out var startTime);
        Console.WriteLine($"{startTime} affinity {toTime.GetAffinityForObjects()}");

        TimeOnlyToStringTypeConverter fromTime = new TimeOnlyToStringTypeConverter();
        _ = fromTime.TryConvert(AppointmentStartTime, conversionHint: null, out var startTimeText);
        Console.WriteLine($"{startTimeText} affinity {fromTime.GetAffinityForObjects()}");

        StringToNullableTimeOnlyTypeConverter toOptionalTime = new StringToNullableTimeOnlyTypeConverter();
        _ = toOptionalTime.TryConvert(StartTimeText, conversionHint: null, out var optionalStartTime);
        Console.WriteLine($"{optionalStartTime} affinity {toOptionalTime.GetAffinityForObjects()}");

        NullableTimeOnlyToStringTypeConverter fromOptionalTime = new NullableTimeOnlyToStringTypeConverter();
        _ = fromOptionalTime.TryConvert((TimeOnly?)AppointmentStartTime, conversionHint: null, out var optionalStartTimeText);
        Console.WriteLine($"{optionalStartTimeText} affinity {fromOptionalTime.GetAffinityForObjects()}");

        // Output:
        // 14:30 affinity 2
        // 14:30 affinity 2
        // 14:30 affinity 2
        // 14:30 affinity 2
    }

    /// <summary>Converts a project duration to and from text with all four TimeSpan converters and shows their affinity.</summary>
    public static void ConvertProjectDurationBothWays()
    {
        StringToTimeSpanTypeConverter toSpan = new StringToTimeSpanTypeConverter();
        _ = toSpan.TryConvert(DurationText, conversionHint: null, out var duration);
        Console.WriteLine($"{duration} affinity {toSpan.GetAffinityForObjects()}");

        TimeSpanToStringTypeConverter fromSpan = new TimeSpanToStringTypeConverter();
        _ = fromSpan.TryConvert(ProjectDuration, conversionHint: null, out var durationText);
        Console.WriteLine($"{durationText} affinity {fromSpan.GetAffinityForObjects()}");

        StringToNullableTimeSpanTypeConverter toOptionalSpan = new StringToNullableTimeSpanTypeConverter();
        _ = toOptionalSpan.TryConvert(DurationText, conversionHint: null, out var optionalDuration);
        Console.WriteLine($"{optionalDuration} affinity {toOptionalSpan.GetAffinityForObjects()}");

        NullableTimeSpanToStringTypeConverter fromOptionalSpan = new NullableTimeSpanToStringTypeConverter();
        _ = fromOptionalSpan.TryConvert((TimeSpan?)ProjectDuration, conversionHint: null, out var optionalDurationText);
        Console.WriteLine($"{optionalDurationText} affinity {fromOptionalSpan.GetAffinityForObjects()}");

        // Output:
        // 01:30:00 affinity 2
        // 01:30:00 affinity 2
        // 01:30:00 affinity 2
        // 01:30:00 affinity 2
    }
}
