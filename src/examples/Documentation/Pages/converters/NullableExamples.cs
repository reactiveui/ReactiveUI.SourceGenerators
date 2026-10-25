// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Demonstrates nullable type converters.</summary>
public static class NullableExamples
{
    /// <summary>Invalid number text for testing parse failures.</summary>
    private const string InvalidNumberText = "not-a-number";

    /// <summary>A GitHub issue number for tracking and reference.</summary>
    private const int IssueNumber = 42;

    /// <summary>A file size in bytes (512 KB for cloud storage).</summary>
    private const long FileSize = 524_288L;

    /// <summary>A TCP port number for network configuration.</summary>
    private const short PortNumber = 8080;

    /// <summary>A scientific constant (pi) value.</summary>
    private const float Pi = 3.14F;

    /// <summary>A mathematical constant (Euler's number).</summary>
    private const double EulersNumber = 2.71828D;

    /// <summary>A customer's transfer amount in banking applications.</summary>
    private const decimal TransferAmount = 250.50M;

    /// <summary>An RGB color channel value (0-255).</summary>
    private const byte ColorChannelValue = 255;

    /// <summary>A student's score in an educational system.</summary>
    private const decimal StudentScore = 72.5M;

    /// <summary>A todo item's due date.</summary>
    private static readonly DateOnly TodoDueDate = new(2025, 12, 25);

    /// <summary>A cloud storage file's last modified time.</summary>
    private static readonly DateTimeOffset FileLastModifiedTime = new(2025, 9, 21, 10, 30, 0, TimeSpan.Zero);

    /// <summary>A unique session correlation ID.</summary>
    private static readonly Guid SessionCorrelationId = new("550e8400-e29b-41d4-a716-446655440000");

    /// <summary>An appointment start time.</summary>
    private static readonly TimeOnly AppointmentStartTime = new(14, 30, 0);

    /// <summary>A project's estimated duration.</summary>
    private static readonly TimeSpan ProjectDuration = new(1, 30, 0);

    /// <summary>Wraps an issue number in a nullable integer.</summary>
    public static void WrapGitHubIssueNumber()
    {
        IntegerToNullableIntegerTypeConverter converter = new IntegerToNullableIntegerTypeConverter();
        bool success = converter.TryConvert(IssueNumber, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 42
    }

    /// <summary>Unwraps an optional issue number to a required integer.</summary>
    public static void UnwrapGitHubIssueNumber()
    {
        NullableIntegerToIntegerTypeConverter converter = new NullableIntegerToIntegerTypeConverter();
        bool success = converter.TryConvert((int?)IssueNumber, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 42
    }

    /// <summary>Wraps a file size in a nullable long.</summary>
    public static void WrapCloudStorageFileSize()
    {
        LongToNullableLongTypeConverter converter = new LongToNullableLongTypeConverter();
        bool success = converter.TryConvert(FileSize, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 524288
    }

    /// <summary>Unwraps an optional file size to a required long.</summary>
    public static void UnwrapCloudStorageFileSize()
    {
        NullableLongToLongTypeConverter converter = new NullableLongToLongTypeConverter();
        bool success = converter.TryConvert((long?)FileSize, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 524288
    }

    /// <summary>Wraps a port number in a nullable short.</summary>
    public static void WrapNetworkPortNumber()
    {
        ShortToNullableShortTypeConverter converter = new ShortToNullableShortTypeConverter();
        bool success = converter.TryConvert(PortNumber, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 8080
    }

    /// <summary>Unwraps an optional port number to a required short.</summary>
    public static void UnwrapNetworkPortNumber()
    {
        NullableShortToShortTypeConverter converter = new NullableShortToShortTypeConverter();
        bool success = converter.TryConvert((short?)PortNumber, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 8080
    }

    /// <summary>Wraps a scientific constant in a nullable single.</summary>
    public static void WrapScientificConstant()
    {
        SingleToNullableSingleTypeConverter converter = new SingleToNullableSingleTypeConverter();
        bool success = converter.TryConvert(Pi, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 3.14
    }

    /// <summary>Unwraps an optional scientific value to a required single.</summary>
    public static void UnwrapScientificConstant()
    {
        NullableSingleToSingleTypeConverter converter = new NullableSingleToSingleTypeConverter();
        bool success = converter.TryConvert((float?)Pi, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 3.14
    }

    /// <summary>Wraps Euler's number in a nullable double.</summary>
    public static void WrapEulersNumber()
    {
        DoubleToNullableDoubleTypeConverter converter = new DoubleToNullableDoubleTypeConverter();
        bool success = converter.TryConvert(EulersNumber, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 2.71828
    }

    /// <summary>Unwraps an optional mathematical constant to a required double.</summary>
    public static void UnwrapEulersNumber()
    {
        NullableDoubleToDoubleTypeConverter converter = new NullableDoubleToDoubleTypeConverter();
        bool success = converter.TryConvert((double?)EulersNumber, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 2.71828
    }

    /// <summary>Wraps a transfer amount in a nullable decimal.</summary>
    public static void WrapBankTransferAmount()
    {
        DecimalToNullableDecimalTypeConverter converter = new DecimalToNullableDecimalTypeConverter();
        bool success = converter.TryConvert(TransferAmount, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 250.50
    }

    /// <summary>Unwraps an optional transfer amount to a required decimal.</summary>
    public static void UnwrapBankTransferAmount()
    {
        NullableDecimalToDecimalTypeConverter converter = new NullableDecimalToDecimalTypeConverter();
        bool success = converter.TryConvert((decimal?)TransferAmount, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 250.50
    }

    /// <summary>Wraps an RGB color channel value in a nullable byte.</summary>
    public static void WrapRGBColorChannel()
    {
        ByteToNullableByteTypeConverter converter = new ByteToNullableByteTypeConverter();
        bool success = converter.TryConvert(ColorChannelValue, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 255
    }

    /// <summary>Unwraps an optional color channel value to a required byte.</summary>
    public static void UnwrapRGBColorChannel()
    {
        NullableByteToByteTypeConverter converter = new NullableByteToByteTypeConverter();
        bool success = converter.TryConvert((byte?)ColorChannelValue, conversionHint: null, out var result);
        Console.WriteLine(success);
        Console.WriteLine(result);

        // Output:
        // True
        // 255
    }

    /// <summary>Parses an optional GitHub issue number from text input.</summary>
    public static void ParseOptionalGitHubIssueNumber()
    {
        StringToNullableIntegerTypeConverter converter = new StringToNullableIntegerTypeConverter();

        // Success case: valid issue number
        bool success = converter.TryConvert("42", conversionHint: null, out var issueNumber);
        Console.WriteLine(success);
        Console.WriteLine(issueNumber);

        // Success case: empty string returns null
        bool empty = converter.TryConvert(string.Empty, conversionHint: null, out var noIssue);
        Console.WriteLine(empty);
        Console.WriteLine(noIssue is null);

        // Failure case: invalid text
        bool failure = converter.TryConvert(InvalidNumberText, conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 42
        // True
        // True
        // False
    }

    /// <summary>Parses an optional transfer amount from a user input field.</summary>
    public static void ParseOptionalBankTransferAmount()
    {
        StringToNullableDecimalTypeConverter converter = new StringToNullableDecimalTypeConverter();

        // Success case: valid amount
        bool success = converter.TryConvert("250.50", conversionHint: null, out var amount);
        Console.WriteLine(success);
        Console.WriteLine(amount);

        // Success case: empty returns null
        bool empty = converter.TryConvert(string.Empty, conversionHint: null, out var noAmount);
        Console.WriteLine(empty);
        Console.WriteLine(noAmount is null);

        // Failure case: invalid currency
        bool failure = converter.TryConvert("not-money", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 250.50
        // True
        // True
        // False
    }

    /// <summary>Formats an optional GitHub issue number for display.</summary>
    public static void FormatOptionalGitHubIssueNumber()
    {
        NullableIntegerToStringTypeConverter converter = new NullableIntegerToStringTypeConverter();
        bool success = converter.TryConvert((int?)IssueNumber, conversionHint: null, out var issueText);
        Console.WriteLine(success);
        Console.WriteLine(issueText);

        // Output:
        // True
        // 42
    }

    /// <summary>Formats an optional transfer amount for display in a banking receipt.</summary>
    public static void FormatOptionalBankTransferAmount()
    {
        NullableDecimalToStringTypeConverter converter = new NullableDecimalToStringTypeConverter();
        bool success = converter.TryConvert((decimal?)TransferAmount, conversionHint: null, out var amountText);
        Console.WriteLine(success);
        Console.WriteLine(amountText);

        // Output:
        // True
        // 250.50
    }

    /// <summary>Parses an optional customer's bank balance from user input.</summary>
    public static void ParseOptionalBankBalance()
    {
        StringToNullableDecimalTypeConverter converter = new StringToNullableDecimalTypeConverter();

        // Success case: valid balance amount
        bool success = converter.TryConvert("1500.00", conversionHint: null, out var balance);
        Console.WriteLine(success);
        Console.WriteLine(balance);

        // Success case: empty field means unknown balance
        bool empty = converter.TryConvert(string.Empty, conversionHint: null, out var unknownBalance);
        Console.WriteLine(empty);
        Console.WriteLine(unknownBalance is null);

        // Output:
        // True
        // 1500.00
        // True
        // True
    }

    /// <summary>Wraps a student's score in a nullable decimal.</summary>
    public static void WrapStudentScore()
    {
        DecimalToNullableDecimalTypeConverter converter = new DecimalToNullableDecimalTypeConverter();
        bool success = converter.TryConvert(StudentScore, conversionHint: null, out var score);
        Console.WriteLine(success);
        Console.WriteLine(score);

        // Output:
        // True
        // 72.5
    }

    /// <summary>Parses an optional student score from a grading system text field.</summary>
    public static void ParseOptionalStudentScore()
    {
        StringToNullableDecimalTypeConverter converter = new StringToNullableDecimalTypeConverter();

        // Success case: valid score
        bool success = converter.TryConvert("72.5", conversionHint: null, out var score);
        Console.WriteLine(success);
        Console.WriteLine(score);

        // Success case: no score recorded
        bool empty = converter.TryConvert(string.Empty, conversionHint: null, out var noScore);
        Console.WriteLine(empty);
        Console.WriteLine(noScore is null);

        // Output:
        // True
        // 72.5
        // True
        // True
    }

    /// <summary>Parses an optional byte value (color channel) from text input.</summary>
    public static void ParseOptionalRGBColorChannel()
    {
        StringToNullableByteTypeConverter converter = new StringToNullableByteTypeConverter();

        // Success case: valid color intensity
        bool success = converter.TryConvert("255", conversionHint: null, out var channelValue);
        Console.WriteLine(success);
        Console.WriteLine(channelValue);

        // Success case: not set
        bool notSet = converter.TryConvert(string.Empty, conversionHint: null, out var noChannel);
        Console.WriteLine(notSet);
        Console.WriteLine(noChannel is null);

        // Failure case: out of range
        bool outOfRange = converter.TryConvert("256", conversionHint: null, out _);
        Console.WriteLine(outOfRange);

        // Output:
        // True
        // 255
        // True
        // True
        // False
    }

    /// <summary>Formats an optional byte value for display in a color picker.</summary>
    public static void FormatOptionalRGBColorChannel()
    {
        NullableByteToStringTypeConverter converter = new NullableByteToStringTypeConverter();

        bool success = converter.TryConvert((byte?)ColorChannelValue, conversionHint: null, out var channelText);
        Console.WriteLine(success);
        Console.WriteLine(channelText);

        // Output:
        // True
        // 255
    }

    /// <summary>Parses an optional short value (port) from text input.</summary>
    public static void ParseOptionalNetworkPortNumber()
    {
        StringToNullableShortTypeConverter converter = new StringToNullableShortTypeConverter();

        // Success case: valid port
        bool success = converter.TryConvert("8080", conversionHint: null, out var portValue);
        Console.WriteLine(success);
        Console.WriteLine(portValue);

        // Success case: empty means default
        bool empty = converter.TryConvert(string.Empty, conversionHint: null, out var defaultPort);
        Console.WriteLine(empty);
        Console.WriteLine(defaultPort is null);

        // Failure case: out of range
        bool outOfRange = converter.TryConvert("99999", conversionHint: null, out _);
        Console.WriteLine(outOfRange);

        // Output:
        // True
        // 8080
        // True
        // True
        // False
    }

    /// <summary>Formats an optional short value for display in network settings.</summary>
    public static void FormatOptionalNetworkPortNumber()
    {
        NullableShortToStringTypeConverter converter = new NullableShortToStringTypeConverter();

        bool success = converter.TryConvert((short?)PortNumber, conversionHint: null, out var portText);
        Console.WriteLine(success);
        Console.WriteLine(portText);

        // Output:
        // True
        // 8080
    }

    /// <summary>Parses an optional long value (file size) from text input.</summary>
    public static void ParseOptionalCloudStorageFileSize()
    {
        StringToNullableLongTypeConverter converter = new StringToNullableLongTypeConverter();

        // Success case: valid file size
        bool success = converter.TryConvert("524288", conversionHint: null, out var fileSize);
        Console.WriteLine(success);
        Console.WriteLine(fileSize);

        // Success case: empty means unknown size
        bool empty = converter.TryConvert(string.Empty, conversionHint: null, out var unknownSize);
        Console.WriteLine(empty);
        Console.WriteLine(unknownSize is null);

        // Failure case: overflow
        bool overflow = converter.TryConvert("999999999999999999999999", conversionHint: null, out _);
        Console.WriteLine(overflow);

        // Output:
        // True
        // 524288
        // True
        // True
        // False
    }

    /// <summary>Formats an optional long value for display in storage usage views.</summary>
    public static void FormatOptionalCloudStorageFileSize()
    {
        NullableLongToStringTypeConverter converter = new NullableLongToStringTypeConverter();

        bool success = converter.TryConvert((long?)FileSize, conversionHint: null, out var sizeText);
        Console.WriteLine(success);
        Console.WriteLine(sizeText);

        // Output:
        // True
        // 524288
    }

    /// <summary>Parses an optional float value (pi) from text input.</summary>
    public static void ParseOptionalScientificConstant()
    {
        StringToNullableSingleTypeConverter converter = new StringToNullableSingleTypeConverter();

        // Success case: valid constant
        bool success = converter.TryConvert("3.14", conversionHint: null, out var piValue);
        Console.WriteLine(success);
        Console.WriteLine(piValue);

        // Success case: not specified
        bool notSpecified = converter.TryConvert(string.Empty, conversionHint: null, out var noPi);
        Console.WriteLine(notSpecified);
        Console.WriteLine(noPi is null);

        // Failure case: invalid input
        bool failure = converter.TryConvert(InvalidNumberText, conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 3.14
        // True
        // True
        // False
    }

    /// <summary>Formats an optional float value for display in calculator results.</summary>
    public static void FormatOptionalScientificConstant()
    {
        NullableSingleToStringTypeConverter converter = new NullableSingleToStringTypeConverter();

        bool success = converter.TryConvert((float?)Pi, conversionHint: null, out var piText);
        Console.WriteLine(success);
        Console.WriteLine(piText);

        // Output:
        // True
        // 3.14
    }

    /// <summary>Parses an optional double value (Euler's number) from text input.</summary>
    public static void ParseOptionalEulersNumber()
    {
        StringToNullableDoubleTypeConverter converter = new StringToNullableDoubleTypeConverter();

        // Success case: valid mathematical constant
        bool success = converter.TryConvert("2.71828", conversionHint: null, out var eValue);
        Console.WriteLine(success);
        Console.WriteLine(eValue);

        // Success case: empty means unknown
        bool empty = converter.TryConvert(string.Empty, conversionHint: null, out var noE);
        Console.WriteLine(empty);
        Console.WriteLine(noE is null);

        // Failure case: invalid text
        bool failure = converter.TryConvert(InvalidNumberText, conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 2.71828
        // True
        // True
        // False
    }

    /// <summary>Formats an optional double value for display in scientific calculations.</summary>
    public static void FormatOptionalEulersNumber()
    {
        NullableDoubleToStringTypeConverter converter = new NullableDoubleToStringTypeConverter();

        bool success = converter.TryConvert((double?)EulersNumber, conversionHint: null, out var eText);
        Console.WriteLine(success);
        Console.WriteLine(eText);

        // Output:
        // True
        // 2.71828
    }

    /// <summary>Parses an optional boolean from text input for a feature toggle.</summary>
    public static void ParseOptionalFeatureToggleBoolean()
    {
        StringToNullableBooleanTypeConverter converter = new StringToNullableBooleanTypeConverter();

        // Success case: feature enabled
        bool success = converter.TryConvert("True", conversionHint: null, out var isEnabled);
        Console.WriteLine(success);
        Console.WriteLine(isEnabled);

        // Success case: undecided/not set
        bool notSet = converter.TryConvert(string.Empty, conversionHint: null, out var undecided);
        Console.WriteLine(notSet);
        Console.WriteLine(undecided is null);

        // Failure case: invalid toggle
        bool failure = converter.TryConvert("maybe", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // True
        // True
        // True
        // False
    }

    /// <summary>Formats an optional boolean for display in settings UI.</summary>
    public static void FormatOptionalFeatureToggleBoolean()
    {
        NullableBooleanToStringTypeConverter converter = new NullableBooleanToStringTypeConverter();

        bool success = converter.TryConvert((bool?)true, conversionHint: null, out var toggleText);
        Console.WriteLine(success);
        Console.WriteLine(toggleText);

        // Output:
        // True
        // True
    }

    /// <summary>Parses an optional DateOnly (todo due date) from text input.</summary>
    public static void ParseOptionalTodoDueDate()
    {
        StringToNullableDateOnlyTypeConverter converter = new StringToNullableDateOnlyTypeConverter();

        // Success case: valid due date
        bool success = converter.TryConvert("2025-12-25", conversionHint: null, out var dueDate);
        Console.WriteLine(success);
        Console.WriteLine(dueDate);

        // Success case: not set yet
        bool notSet = converter.TryConvert(string.Empty, conversionHint: null, out var noDueDate);
        Console.WriteLine(notSet);
        Console.WriteLine(noDueDate is null);

        // Failure case: invalid date
        bool failure = converter.TryConvert("32/13/2025", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 12/25/2025
        // True
        // True
        // False
    }

    /// <summary>Formats an optional DateOnly for display in task management views.</summary>
    public static void FormatOptionalTodoDueDate()
    {
        NullableDateOnlyToStringTypeConverter converter = new NullableDateOnlyToStringTypeConverter();

        bool success = converter.TryConvert((DateOnly?)TodoDueDate, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 12/25/2025
    }

    /// <summary>Parses an optional DateTimeOffset (file modification time) from text input.</summary>
    public static void ParseOptionalCloudStorageModificationTime()
    {
        StringToNullableDateTimeOffsetTypeConverter converter = new StringToNullableDateTimeOffsetTypeConverter();

        // Success case: valid timestamp with timezone
        bool success = converter.TryConvert("2025-09-21T10:30:00+00:00", conversionHint: null, out var modifiedTime);
        Console.WriteLine(success);
        Console.WriteLine(modifiedTime);

        // Success case: not available
        bool notAvailable = converter.TryConvert(string.Empty, conversionHint: null, out var noModifiedTime);
        Console.WriteLine(notAvailable);
        Console.WriteLine(noModifiedTime is null);

        // Failure case: invalid format
        bool failure = converter.TryConvert("invalid-date", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 09/21/2025 10:30:00 +00:00
        // True
        // True
        // False
    }

    /// <summary>Formats an optional DateTimeOffset for display in file properties.</summary>
    public static void FormatOptionalCloudStorageModificationTime()
    {
        NullableDateTimeOffsetToStringTypeConverter converter = new NullableDateTimeOffsetToStringTypeConverter();

        bool success = converter.TryConvert((DateTimeOffset?)FileLastModifiedTime, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 09/21/2025 10:30:00 +00:00
    }

    /// <summary>Parses an optional DateTime from text input.</summary>
    public static void ParseOptionalDateTime()
    {
        StringToNullableDateTimeTypeConverter converter = new StringToNullableDateTimeTypeConverter();

        // Success case: valid due date-time
        bool success = converter.TryConvert("2025-12-25T10:30:00", conversionHint: null, out var dueDateTime);
        Console.WriteLine(success);
        Console.WriteLine(dueDateTime);

        // Failure case: invalid format
        bool failure = converter.TryConvert("not-a-date", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 12/25/2025 10:30:00
        // False
    }

    /// <summary>Formats an optional DateTime for display.</summary>
    public static void FormatOptionalDateTime()
    {
        DateTime testDateTime = new DateTime(2025, 12, 25, 10, 30, 0, DateTimeKind.Unspecified);
        NullableDateTimeToStringTypeConverter converter = new NullableDateTimeToStringTypeConverter();

        bool success = converter.TryConvert((DateTime?)testDateTime, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 12/25/2025 10:30:00
    }

    /// <summary>Parses an optional TimeOnly from text input.</summary>
    public static void ParseOptionalAppointmentTime()
    {
        StringToNullableTimeOnlyTypeConverter converter = new StringToNullableTimeOnlyTypeConverter();

        // Success case: valid time
        bool success = converter.TryConvert("14:30:00", conversionHint: null, out var appointmentTime);
        Console.WriteLine(success);
        Console.WriteLine(appointmentTime);

        // Success case: not scheduled yet
        bool notScheduled = converter.TryConvert(string.Empty, conversionHint: null, out var noAppointment);
        Console.WriteLine(notScheduled);
        Console.WriteLine(noAppointment is null);

        // Failure case: invalid time
        bool failure = converter.TryConvert("25:00:00", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 14:30
        // True
        // True
        // False
    }

    /// <summary>Formats an optional TimeOnly for display in calendar events.</summary>
    public static void FormatOptionalAppointmentTime()
    {
        NullableTimeOnlyToStringTypeConverter converter = new NullableTimeOnlyToStringTypeConverter();

        bool success = converter.TryConvert((TimeOnly?)AppointmentStartTime, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 14:30
    }

    /// <summary>Parses an optional TimeSpan (project duration) from text input.</summary>
    public static void ParseOptionalProjectDuration()
    {
        StringToNullableTimeSpanTypeConverter converter = new StringToNullableTimeSpanTypeConverter();

        // Success case: valid duration
        bool success = converter.TryConvert("01:30:00", conversionHint: null, out var duration);
        Console.WriteLine(success);
        Console.WriteLine(duration);

        // Success case: not estimated yet
        bool notEstimated = converter.TryConvert(string.Empty, conversionHint: null, out var noDuration);
        Console.WriteLine(notEstimated);
        Console.WriteLine(noDuration is null);

        // Failure case: invalid format
        bool failure = converter.TryConvert("invalid-duration", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 01:30:00
        // True
        // True
        // False
    }

    /// <summary>Formats an optional TimeSpan for display in project management views.</summary>
    public static void FormatOptionalProjectDuration()
    {
        NullableTimeSpanToStringTypeConverter converter = new NullableTimeSpanToStringTypeConverter();

        bool success = converter.TryConvert((TimeSpan?)ProjectDuration, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 01:30:00
    }

    /// <summary>Parses an optional Guid (session ID) from text input.</summary>
    public static void ParseOptionalSessionCorrelationId()
    {
        StringToNullableGuidTypeConverter converter = new StringToNullableGuidTypeConverter();

        // Success case: valid GUID
        bool success = converter.TryConvert("550e8400-e29b-41d4-a716-446655440000", conversionHint: null, out var correlationId);
        Console.WriteLine(success);
        Console.WriteLine(correlationId);

        // Success case: not set
        bool notSet = converter.TryConvert(string.Empty, conversionHint: null, out var noCorrelationId);
        Console.WriteLine(notSet);
        Console.WriteLine(noCorrelationId is null);

        // Failure case: invalid GUID format
        bool failure = converter.TryConvert("not-a-guid", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 550e8400-e29b-41d4-a716-446655440000
        // True
        // True
        // False
    }

    /// <summary>Formats an optional Guid for display in logging and trace views.</summary>
    public static void FormatOptionalSessionCorrelationId()
    {
        NullableGuidToStringTypeConverter converter = new NullableGuidToStringTypeConverter();

        bool success = converter.TryConvert((Guid?)SessionCorrelationId, conversionHint: null, out var correlationIdText);
        Console.WriteLine(success);
        Console.WriteLine(correlationIdText);

        // Output:
        // True
        // 550e8400-e29b-41d4-a716-446655440000
    }

    /// <summary>Parses text into each optional number type; every converter reports the same affinity.</summary>
    public static void ParseEveryOptionalNumberType()
    {
        StringToNullableByteTypeConverter toByte = new StringToNullableByteTypeConverter();
        _ = toByte.TryConvert("255", conversionHint: null, out var colorChannel);
        Console.WriteLine($"{colorChannel} affinity {toByte.GetAffinityForObjects()}");

        StringToNullableShortTypeConverter toShort = new StringToNullableShortTypeConverter();
        _ = toShort.TryConvert("8080", conversionHint: null, out var portNumber);
        Console.WriteLine($"{portNumber} affinity {toShort.GetAffinityForObjects()}");

        StringToNullableIntegerTypeConverter toInteger = new StringToNullableIntegerTypeConverter();
        _ = toInteger.TryConvert("42", conversionHint: null, out var issueNumber);
        Console.WriteLine($"{issueNumber} affinity {toInteger.GetAffinityForObjects()}");

        StringToNullableLongTypeConverter toLong = new StringToNullableLongTypeConverter();
        _ = toLong.TryConvert("524288", conversionHint: null, out var fileSize);
        Console.WriteLine($"{fileSize} affinity {toLong.GetAffinityForObjects()}");

        StringToNullableSingleTypeConverter toSingle = new StringToNullableSingleTypeConverter();
        _ = toSingle.TryConvert("3.14", conversionHint: null, out var pi);
        Console.WriteLine($"{pi} affinity {toSingle.GetAffinityForObjects()}");

        StringToNullableDoubleTypeConverter toDouble = new StringToNullableDoubleTypeConverter();
        _ = toDouble.TryConvert("2.71828", conversionHint: null, out var eulersNumber);
        Console.WriteLine($"{eulersNumber} affinity {toDouble.GetAffinityForObjects()}");

        StringToNullableDecimalTypeConverter toDecimal = new StringToNullableDecimalTypeConverter();
        _ = toDecimal.TryConvert("250.50", conversionHint: null, out var transferAmount);
        Console.WriteLine($"{transferAmount} affinity {toDecimal.GetAffinityForObjects()}");

        // Output:
        // 255 affinity 2
        // 8080 affinity 2
        // 42 affinity 2
        // 524288 affinity 2
        // 3.14 affinity 2
        // 2.71828 affinity 2
        // 250.50 affinity 2
    }

    /// <summary>Formats each optional number type as text; every converter reports the same affinity.</summary>
    public static void FormatEveryOptionalNumberType()
    {
        NullableByteToStringTypeConverter fromByte = new NullableByteToStringTypeConverter();
        _ = fromByte.TryConvert((byte?)ColorChannelValue, conversionHint: null, out var colorChannel);
        Console.WriteLine($"{colorChannel} affinity {fromByte.GetAffinityForObjects()}");

        NullableShortToStringTypeConverter fromShort = new NullableShortToStringTypeConverter();
        _ = fromShort.TryConvert((short?)PortNumber, conversionHint: null, out var portNumber);
        Console.WriteLine($"{portNumber} affinity {fromShort.GetAffinityForObjects()}");

        NullableIntegerToStringTypeConverter fromInteger = new NullableIntegerToStringTypeConverter();
        _ = fromInteger.TryConvert((int?)IssueNumber, conversionHint: null, out var issueNumber);
        Console.WriteLine($"{issueNumber} affinity {fromInteger.GetAffinityForObjects()}");

        NullableLongToStringTypeConverter fromLong = new NullableLongToStringTypeConverter();
        _ = fromLong.TryConvert((long?)FileSize, conversionHint: null, out var fileSize);
        Console.WriteLine($"{fileSize} affinity {fromLong.GetAffinityForObjects()}");

        NullableSingleToStringTypeConverter fromSingle = new NullableSingleToStringTypeConverter();
        _ = fromSingle.TryConvert((float?)Pi, conversionHint: null, out var pi);
        Console.WriteLine($"{pi} affinity {fromSingle.GetAffinityForObjects()}");

        NullableDoubleToStringTypeConverter fromDouble = new NullableDoubleToStringTypeConverter();
        _ = fromDouble.TryConvert((double?)EulersNumber, conversionHint: null, out var eulersNumber);
        Console.WriteLine($"{eulersNumber} affinity {fromDouble.GetAffinityForObjects()}");

        NullableDecimalToStringTypeConverter fromDecimal = new NullableDecimalToStringTypeConverter();
        _ = fromDecimal.TryConvert((decimal?)TransferAmount, conversionHint: null, out var transferAmount);
        Console.WriteLine($"{transferAmount} affinity {fromDecimal.GetAffinityForObjects()}");

        // Output:
        // 255 affinity 2
        // 8080 affinity 2
        // 42 affinity 2
        // 524288 affinity 2
        // 3.14 affinity 2
        // 2.71828 affinity 2
        // 250.50 affinity 2
    }
}
