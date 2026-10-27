// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Demonstrates numeric type converters.</summary>
public static class NumberExamples
{
    /// <summary>A GitHub issue number for converter tests.</summary>
    private const int IssueNumber = 42;

    /// <summary>A file size in bytes (e.g., 512 KB for cloud storage).</summary>
    private const long FileSize = 524_288L;

    /// <summary>A port number that network applications use.</summary>
    private const short PortNumber = 8080;

    /// <summary>A port number that typically uses SSL/TLS.</summary>
    private const short SecurePortNumber = -32_768;

    /// <summary>A scientific constant (pi) for calculator examples.</summary>
    private const float Pi = 3.14F;

    /// <summary>A student score represented as a whole number.</summary>
    private const float StudentPassingScore = 70.0F;

    /// <summary>A mathematical constant (Euler's number).</summary>
    private const double EulersNumber = 2.71828D;

    /// <summary>A customer's transfer amount in a banking app.</summary>
    private const decimal TransferAmount = 250.50M;

    /// <summary>An RGB color value (0-255).</summary>
    private const byte ColorChannelValue = 255;

    /// <summary>Invalid text for testing parse failures.</summary>
    private const string InvalidNumberText = "not-a-number";

    /// <summary>Parses an issue number a user typed into the search field.</summary>
    public static void ParseGitHubIssueNumber()
    {
        StringToIntegerTypeConverter converter = new StringToIntegerTypeConverter();

        // Success case: valid issue number
        bool success = converter.TryConvert("42", conversionHint: null, out var issueNumber);
        Console.WriteLine(success);
        Console.WriteLine(issueNumber);

        // Failure case: non-numeric input
        bool failure = converter.TryConvert(InvalidNumberText, conversionHint: null, out var failResult);
        Console.WriteLine(failure);
        Console.WriteLine(failResult);

        // Output:
        // True
        // 42
        // False
        // 0
    }

    /// <summary>Formats a GitHub issue number for display in a label.</summary>
    public static void FormatGitHubIssueNumber()
    {
        IntegerToStringTypeConverter converter = new IntegerToStringTypeConverter();

        // Success case: display issue number
        bool success = converter.TryConvert(IssueNumber, conversionHint: null, out var issueText);
        Console.WriteLine(success);
        Console.WriteLine(issueText);

        // Success case: no issues
        _ = converter.TryConvert(0, conversionHint: null, out var noIssuesText);
        Console.WriteLine(noIssuesText);

        // Output:
        // True
        // 42
        // 0
    }

    /// <summary>Parses a file size in bytes that a user typed into a storage quota field.</summary>
    public static void ParseCloudStorageFileSize()
    {
        StringToLongTypeConverter converter = new StringToLongTypeConverter();

        // Success case: file size in bytes (512 KB)
        bool success = converter.TryConvert("524288", conversionHint: null, out var fileSize);
        Console.WriteLine(success);
        Console.WriteLine(fileSize);

        // Failure case: overflow
        bool overflow = converter.TryConvert("99999999999999999999999", conversionHint: null, out _);
        Console.WriteLine(overflow);

        // Output:
        // True
        // 524288
        // False
    }

    /// <summary>Formats a file size in bytes for display in a UI label.</summary>
    public static void FormatCloudStorageFileSize()
    {
        LongToStringTypeConverter converter = new LongToStringTypeConverter();

        // Success case: file size display
        bool success = converter.TryConvert(FileSize, conversionHint: null, out var sizeText);
        Console.WriteLine(success);
        Console.WriteLine(sizeText);

        // Success case: empty storage
        _ = converter.TryConvert(0L, conversionHint: null, out var emptyText);
        Console.WriteLine(emptyText);

        // Output:
        // True
        // 524288
        // 0
    }

    /// <summary>Parses a TCP port number that a user typed into the network settings.</summary>
    public static void ParseNetworkPortNumber()
    {
        StringToShortTypeConverter converter = new StringToShortTypeConverter();

        // Success case: valid port number
        bool success = converter.TryConvert("8080", conversionHint: null, out var portNumber);
        Console.WriteLine(success);
        Console.WriteLine(portNumber);

        // Failure case: port out of valid range
        bool overflow = converter.TryConvert("99999", conversionHint: null, out _);
        Console.WriteLine(overflow);

        // Output:
        // True
        // 8080
        // False
    }

    /// <summary>Formats a TCP port number for display in network configuration labels.</summary>
    public static void FormatNetworkPortNumber()
    {
        ShortToStringTypeConverter converter = new ShortToStringTypeConverter();

        // Success case: standard HTTP port
        bool success = converter.TryConvert(PortNumber, conversionHint: null, out var portText);
        Console.WriteLine(success);
        Console.WriteLine(portText);

        // Success case: reserved port range
        bool reserved = converter.TryConvert(SecurePortNumber, conversionHint: null, out var reservedText);
        Console.WriteLine(reserved);
        Console.WriteLine(reservedText);

        // Output:
        // True
        // 8080
        // True
        // -32768
    }

    /// <summary>Parses a scientific constant (pi) that a calculator user typed.</summary>
    public static void ParseScientificConstant()
    {
        StringToSingleTypeConverter converter = new StringToSingleTypeConverter();

        // Success case: pi value
        bool success = converter.TryConvert("3.14", conversionHint: null, out var piValue);
        Console.WriteLine(success);
        Console.WriteLine(piValue);

        // Failure case: invalid text input
        bool failure = converter.TryConvert(InvalidNumberText, conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 3.14
        // False
    }

    /// <summary>Formats a score for display in an educational grade report.</summary>
    public static void FormatStudentScore()
    {
        SingleToStringTypeConverter converter = new SingleToStringTypeConverter();

        // Success case: pi constant
        bool success = converter.TryConvert(Pi, conversionHint: null, out var piText);
        Console.WriteLine(success);
        Console.WriteLine(piText);

        // Success case: whole number score
        bool whole = converter.TryConvert(StudentPassingScore, conversionHint: null, out var scoreText);
        Console.WriteLine(whole);
        Console.WriteLine(scoreText);

        // Output:
        // True
        // 3.14
        // True
        // 70
    }

    /// <summary>Parses Euler's mathematical constant from text input.</summary>
    public static void ParseEulersNumber()
    {
        StringToDoubleTypeConverter converter = new StringToDoubleTypeConverter();

        // Success case: Euler's number
        bool success = converter.TryConvert("2.71828", conversionHint: null, out var eValue);
        Console.WriteLine(success);
        Console.WriteLine(eValue);

        // Failure case: non-numeric input
        bool failure = converter.TryConvert(InvalidNumberText, conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 2.71828
        // False
    }

    /// <summary>Formats Euler's number for display in a scientific calculation UI.</summary>
    public static void FormatEulersNumber()
    {
        DoubleToStringTypeConverter converter = new DoubleToStringTypeConverter();

        // Success case: Euler's number
        bool success = converter.TryConvert(EulersNumber, conversionHint: null, out var eText);
        Console.WriteLine(success);
        Console.WriteLine(eText);

        // Success case: large scientific value
        const double largeValue = 1_234_567.89D;
        bool large = converter.TryConvert(largeValue, conversionHint: null, out var largeText);
        Console.WriteLine(large);
        Console.WriteLine(largeText);

        // Output:
        // True
        // 2.71828
        // True
        // 1234567.89
    }

    /// <summary>Parses a transfer amount that a user typed into the banking app.</summary>
    public static void ParseBankTransferAmount()
    {
        StringToDecimalTypeConverter converter = new StringToDecimalTypeConverter();

        // Success case: transfer amount
        bool success = converter.TryConvert("250.50", conversionHint: null, out var amount);
        Console.WriteLine(success);
        Console.WriteLine(amount);

        // Failure case: invalid currency input
        bool failure = converter.TryConvert("not-a-number", conversionHint: null, out var failResult);
        Console.WriteLine(failure);
        Console.WriteLine(failResult);

        // Output:
        // True
        // 250.50
        // False
        // 0
    }

    /// <summary>Formats a transfer amount for display in a banking receipt.</summary>
    public static void FormatBankTransferAmount()
    {
        DecimalToStringTypeConverter converter = new DecimalToStringTypeConverter();

        // Success case: transfer amount display
        bool success = converter.TryConvert(TransferAmount, conversionHint: null, out var amountText);
        Console.WriteLine(success);
        Console.WriteLine(amountText);

        // Success case: zero balance
        _ = converter.TryConvert(0M, conversionHint: null, out var zeroText);
        Console.WriteLine(zeroText);

        // Output:
        // True
        // 250.50
        // 0
    }

    /// <summary>Parses an RGB color channel value that a user typed into a color picker.</summary>
    public static void ParseRGBColorChannel()
    {
        StringToByteTypeConverter converter = new StringToByteTypeConverter();

        // Success case: maximum color intensity
        bool success = converter.TryConvert("255", conversionHint: null, out var channelValue);
        Console.WriteLine(success);
        Console.WriteLine(channelValue);

        // Failure case: color value out of range
        bool overflow = converter.TryConvert("256", conversionHint: null, out _);
        Console.WriteLine(overflow);

        // Output:
        // True
        // 255
        // False
    }

    /// <summary>Formats an RGB color channel value for display in color picker labels.</summary>
    public static void FormatRGBColorChannel()
    {
        ByteToStringTypeConverter converter = new ByteToStringTypeConverter();

        // Success case: maximum color intensity
        bool success = converter.TryConvert(ColorChannelValue, conversionHint: null, out var channelText);
        Console.WriteLine(success);
        Console.WriteLine(channelText);

        // Success case: no color
        _ = converter.TryConvert((byte)0, conversionHint: null, out var noColorText);
        Console.WriteLine(noColorText);

        // Output:
        // True
        // 255
        // 0
    }

    /// <summary>Parses text into each number type; every text-to-number converter reports the same affinity.</summary>
    public static void ParseEveryNumberType()
    {
        StringToByteTypeConverter toByte = new StringToByteTypeConverter();
        _ = toByte.TryConvert("255", conversionHint: null, out var colorChannel);
        Console.WriteLine($"{colorChannel} affinity {toByte.GetAffinityForObjects()}");

        StringToShortTypeConverter toShort = new StringToShortTypeConverter();
        _ = toShort.TryConvert("8080", conversionHint: null, out var portNumber);
        Console.WriteLine($"{portNumber} affinity {toShort.GetAffinityForObjects()}");

        StringToIntegerTypeConverter toInteger = new StringToIntegerTypeConverter();
        _ = toInteger.TryConvert("42", conversionHint: null, out var issueNumber);
        Console.WriteLine($"{issueNumber} affinity {toInteger.GetAffinityForObjects()}");

        StringToLongTypeConverter toLong = new StringToLongTypeConverter();
        _ = toLong.TryConvert("524288", conversionHint: null, out var fileSize);
        Console.WriteLine($"{fileSize} affinity {toLong.GetAffinityForObjects()}");

        StringToSingleTypeConverter toSingle = new StringToSingleTypeConverter();
        _ = toSingle.TryConvert("3.14", conversionHint: null, out var pi);
        Console.WriteLine($"{pi} affinity {toSingle.GetAffinityForObjects()}");

        StringToDoubleTypeConverter toDouble = new StringToDoubleTypeConverter();
        _ = toDouble.TryConvert("2.71828", conversionHint: null, out var eulersNumber);
        Console.WriteLine($"{eulersNumber} affinity {toDouble.GetAffinityForObjects()}");

        StringToDecimalTypeConverter toDecimal = new StringToDecimalTypeConverter();
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

    /// <summary>Formats each number type as text; every number-to-text converter reports the same affinity.</summary>
    public static void FormatEveryNumberType()
    {
        ByteToStringTypeConverter fromByte = new ByteToStringTypeConverter();
        _ = fromByte.TryConvert(ColorChannelValue, conversionHint: null, out var colorChannel);
        Console.WriteLine($"{colorChannel} affinity {fromByte.GetAffinityForObjects()}");

        ShortToStringTypeConverter fromShort = new ShortToStringTypeConverter();
        _ = fromShort.TryConvert(PortNumber, conversionHint: null, out var portNumber);
        Console.WriteLine($"{portNumber} affinity {fromShort.GetAffinityForObjects()}");

        IntegerToStringTypeConverter fromInteger = new IntegerToStringTypeConverter();
        _ = fromInteger.TryConvert(IssueNumber, conversionHint: null, out var issueNumber);
        Console.WriteLine($"{issueNumber} affinity {fromInteger.GetAffinityForObjects()}");

        LongToStringTypeConverter fromLong = new LongToStringTypeConverter();
        _ = fromLong.TryConvert(FileSize, conversionHint: null, out var fileSize);
        Console.WriteLine($"{fileSize} affinity {fromLong.GetAffinityForObjects()}");

        SingleToStringTypeConverter fromSingle = new SingleToStringTypeConverter();
        _ = fromSingle.TryConvert(Pi, conversionHint: null, out var pi);
        Console.WriteLine($"{pi} affinity {fromSingle.GetAffinityForObjects()}");

        DoubleToStringTypeConverter fromDouble = new DoubleToStringTypeConverter();
        _ = fromDouble.TryConvert(EulersNumber, conversionHint: null, out var eulersNumber);
        Console.WriteLine($"{eulersNumber} affinity {fromDouble.GetAffinityForObjects()}");

        DecimalToStringTypeConverter fromDecimal = new DecimalToStringTypeConverter();
        _ = fromDecimal.TryConvert(TransferAmount, conversionHint: null, out var transferAmount);
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
