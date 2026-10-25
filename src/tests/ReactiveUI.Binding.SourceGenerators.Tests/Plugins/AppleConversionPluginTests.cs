// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Checks when the Foundation date conversions decline a pair.</summary>
public class AppleConversionPluginTests
{
    /// <summary>A Foundation date with casts to and from <see cref="DateTime"/>.</summary>
    private const string DateWithOperators = """
        namespace Foundation
        {
            public class NSDate
            {
                public static explicit operator NSDate(System.DateTime value) => new NSDate();
                public static explicit operator System.DateTime(NSDate value) => default;
            }
        }
        """;

    /// <summary>A Foundation date that declares no casts.</summary>
    private const string DateWithoutOperators = """
        namespace Foundation
        {
            public class NSDate
            {
            }
        }
        """;

    /// <summary>The metadata name of the Foundation date.</summary>
    private const string NativeDate = "Foundation.NSDate";

    /// <summary>A managed type that is not a date is not converted to a Foundation date.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToNative_ManagedTypeIsNotADate_Declines()
    {
        var compilation = TestHelper.CreateCompilation(DateWithOperators);

        var result = AppleConversionPlugin.ToNative(
            compilation.GetSpecialType(SpecialType.System_String),
            compilation.GetTypeByMetadataName(NativeDate)!,
            compilation);

        await Assert.That(result).IsNull();
    }

    /// <summary>A managed date is not converted when the Foundation date declares no cast from it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToNative_FoundationDateHasNoCast_Declines()
    {
        var compilation = TestHelper.CreateCompilation(DateWithoutOperators);

        var result = AppleConversionPlugin.ToNative(
            compilation.GetSpecialType(SpecialType.System_DateTime),
            compilation.GetTypeByMetadataName(NativeDate)!,
            compilation);

        await Assert.That(result).IsNull();
    }

    /// <summary>A Foundation date is not converted to a managed type that is not a date.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FromNative_ManagedTypeIsNotADate_Declines()
    {
        var compilation = TestHelper.CreateCompilation(DateWithOperators);

        var result = AppleConversionPlugin.FromNative(
            compilation.GetTypeByMetadataName(NativeDate)!,
            compilation.GetSpecialType(SpecialType.System_String),
            compilation);

        await Assert.That(result).IsNull();
    }

    /// <summary>A Foundation date is not converted when it declares no cast to a managed date.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FromNative_FoundationDateHasNoCast_Declines()
    {
        var compilation = TestHelper.CreateCompilation(DateWithoutOperators);

        var result = AppleConversionPlugin.FromNative(
            compilation.GetTypeByMetadataName(NativeDate)!,
            compilation.GetSpecialType(SpecialType.System_DateTime),
            compilation);

        await Assert.That(result).IsNull();
    }
}
