// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="CodeGeneratorHelpers"/> methods.</summary>
public class CodeGeneratorHelpersTests
{
    /// <summary>The <c>Address</c> name these tests generate against.</summary>
    private const string AddressName = "Address";

    /// <summary>The fully qualified name of the <c>MyViewModel</c> type used by these tests.</summary>
    private const string MyViewModelTypeName = "global::TestApp.MyViewModel";

    /// <summary>The <c>/src/Test.cs</c> name these tests generate against.</summary>
    private const string SrcTestcsName = "/src/Test.cs";

    /// <summary>The <c>x =&gt; x.Name</c> property selector these tests bind against.</summary>
    private const string NameSelector = "x => x.Name";

    /// <summary>Verifies BuildPropertyAccessChain with a single segment produces "root.Property".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyAccessChain_SingleSegment_ReturnsDottedPath()
    {
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);

        var result = CodeGeneratorHelpers.BuildPropertyAccessChain("obj", path);

        await Assert.That(result).IsEqualTo("obj.Name");
    }

    /// <summary>Verifies BuildPropertyAccessChain with multiple segments produces "root.A.B".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyAccessChain_MultiSegment_ReturnsDottedPath()
    {
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName, "global::TestApp.Address"),
            ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
        ]);

        var result = CodeGeneratorHelpers.BuildPropertyAccessChain("obj", path);

        await Assert.That(result).IsEqualTo("obj.Address.City");
    }

    /// <summary>Verifies BuildPropertyAccessChain with an empty path returns just the root.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyAccessChain_EmptyPath_ReturnsRoot()
    {
        var path = new EquatableArray<PropertyPathSegment>([]);

        var result = CodeGeneratorHelpers.BuildPropertyAccessChain("obj", path);

        await Assert.That(result).IsEqualTo("obj");
    }

    /// <summary>Verifies BuildPropertyPathString produces dotted property names for a single segment.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyPathString_SingleSegment_ReturnsPropertyName()
    {
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);

        var result = CodeGeneratorHelpers.BuildPropertyPathString(path);

        await Assert.That(result).IsEqualTo("Name");
    }

    /// <summary>Verifies BuildPropertyPathString produces dotted names for multiple segments.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyPathString_MultiSegment_ReturnsDottedPath()
    {
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName),
            ModelFactory.CreatePropertyPathSegment("City")
        ]);

        var result = CodeGeneratorHelpers.BuildPropertyPathString(path);

        await Assert.That(result).IsEqualTo("Address.City");
    }

    /// <summary>Verifies BuildPropertyPathString returns empty string for empty path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyPathString_EmptyPath_ReturnsEmptyString()
    {
        var path = new EquatableArray<PropertyPathSegment>([]);

        var result = CodeGeneratorHelpers.BuildPropertyPathString(path);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies ComputePathSuffix returns the last two path segments.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputePathSuffix_NormalPath_ReturnsLastTwoSegments()
    {
        var result = CodeGeneratorHelpers.ComputePathSuffix("/src/ViewModels/MyViewModel.cs");

        await Assert.That(result).IsEqualTo("ViewModels/MyViewModel.cs");
    }

    /// <summary>Verifies ComputePathSuffix returns just the filename for a single-segment path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputePathSuffix_SingleSegment_ReturnsFilename()
    {
        var result = CodeGeneratorHelpers.ComputePathSuffix("MyViewModel.cs");

        await Assert.That(result).IsEqualTo("MyViewModel.cs");
    }

    /// <summary>Verifies ComputePathSuffix returns empty for empty input.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputePathSuffix_EmptyString_ReturnsEmpty()
    {
        var result = CodeGeneratorHelpers.ComputePathSuffix(string.Empty);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies ComputePathSuffix normalizes backslashes to forward slashes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputePathSuffix_Backslashes_NormalizesToForwardSlashes()
    {
        var result = CodeGeneratorHelpers.ComputePathSuffix(@"C:\src\ViewModels\MyViewModel.cs");

        await Assert.That(result).IsEqualTo("ViewModels/MyViewModel.cs");
    }

    /// <summary>Verifies EscapeString escapes double quotes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EscapeString_WithQuotes_EscapesQuotes()
    {
        var result = CodeGeneratorHelpers.EscapeString("say \"hello\"");

        await Assert.That(result).IsEqualTo("say \\\"hello\\\"");
    }

    /// <summary>Verifies EscapeString escapes backslashes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EscapeString_WithBackslashes_EscapesBackslashes()
    {
        var result = CodeGeneratorHelpers.EscapeString(@"path\to\file");

        await Assert.That(result).IsEqualTo("path\\\\to\\\\file");
    }

    /// <summary>Verifies EscapeString leaves clean strings unchanged.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EscapeString_CleanString_ReturnsUnchanged()
    {
        var result = CodeGeneratorHelpers.EscapeString(NameSelector);

        await Assert.That(result).IsEqualTo(NameSelector);
    }

    /// <summary>Verifies FindClassInfo returns matching class info when found.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindClassInfo_Found_ReturnsMatchingInfo()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo();
        var allClasses = ImmutableArray.Create(classInfo);

        var result = CodeGeneratorHelpers.FindClassInfo(allClasses, MyViewModelTypeName);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.FullyQualifiedName).IsEqualTo(MyViewModelTypeName);
    }

    /// <summary>Verifies FindClassInfo returns null when class info is not found.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindClassInfo_NotFound_ReturnsNull()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo("global::TestApp.OtherType");
        var allClasses = ImmutableArray.Create(classInfo);

        var result = CodeGeneratorHelpers.FindClassInfo(allClasses, MyViewModelTypeName);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies FindClassInfo returns null for empty array.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindClassInfo_EmptyArray_ReturnsNull()
    {
        var allClasses = ImmutableArray<ClassBindingInfo>.Empty;

        var result = CodeGeneratorHelpers.FindClassInfo(allClasses, MyViewModelTypeName);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies AppendExtensionClassHeader produces expected structure.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtensionClassHeader_ProducesExpectedStructure()
    {
        var sb = new SourceWriter();

        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, new(false, true, true));

        var result = sb.ToString();
        await Assert.That(result).Contains("// <auto-generated/>");
        await Assert.That(result).Contains("#pragma warning disable");
        await Assert.That(result).Contains("namespace ReactiveUI.Binding");
        await Assert.That(result).Contains("__ReactiveUIGeneratedBindings");
    }

    /// <summary>Verifies AppendExtensionClassFooter closes the class and its namespace, returning to the file level.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtensionClassFooter_ProducesClosingBraces()
    {
        var sb = new SourceWriter().Indent().Indent();

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);

        await Assert.That(sb.ToString()).IsEqualTo("    }\n}\n");
        await Assert.That(sb.Level).IsEqualTo(0);
    }

    /// <summary>Verifies BuildPropertyAccessLambda delegates to BuildPropertyAccessChain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyAccessLambda_ReturnsParameterDotPath()
    {
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName),
            ModelFactory.CreatePropertyPathSegment("City")
        ]);

        var result = CodeGeneratorHelpers.BuildPropertyAccessLambda("x", path);

        await Assert.That(result).IsEqualTo("x.Address.City");
    }

    /// <summary>Verifies BuildPropertySetterChain delegates to BuildPropertyAccessChain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertySetterChain_ReturnsRootDotPath()
    {
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Text")]);

        var result = CodeGeneratorHelpers.BuildPropertySetterChain("target", path);

        await Assert.That(result).IsEqualTo("target.Text");
    }

    /// <summary>Verifies StableStringHash is deterministic across multiple calls.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StableStringHash_SameInput_ReturnsSameHash()
    {
        var hash1 = CodeGeneratorHelpers.StableStringHash(MyViewModelTypeName);
        var hash2 = CodeGeneratorHelpers.StableStringHash(MyViewModelTypeName);

        await Assert.That(hash1).IsEqualTo(hash2);
    }

    /// <summary>Verifies StableStringHash returns different hashes for different strings.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StableStringHash_DifferentInput_ReturnsDifferentHash()
    {
        var hash1 = CodeGeneratorHelpers.StableStringHash("TypeA");
        var hash2 = CodeGeneratorHelpers.StableStringHash("TypeB");

        await Assert.That(hash1).IsNotEqualTo(hash2);
    }

    /// <summary>Verifies StableStringHash returns 0 for null input.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StableStringHash_NullInput_ReturnsZero()
    {
        var result = CodeGeneratorHelpers.StableStringHash(null!);

        await Assert.That(result).IsEqualTo(0);
    }

    /// <summary>Verifies ComputeStableMethodSuffix produces a 16-character hex string.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeStableMethodSuffix_ReturnsHexString()
    {
        const int CallerLineNumber = 42;
        const int ExpectedResultCount = 16;
        var result = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            MyViewModelTypeName,
            SrcTestcsName,
            CallerLineNumber,
            NameSelector);

        await Assert.That(result.Length).IsEqualTo(ExpectedResultCount);
        await Assert.That(result.All("0123456789ABCDEF".Contains)).IsTrue();
    }

    /// <summary>Verifies ComputeStableMethodSuffix is deterministic.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeStableMethodSuffix_SameInput_ReturnsSameResult()
    {
        const int CallerLineNumber = 42;
        var result1 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            MyViewModelTypeName,
            SrcTestcsName,
            CallerLineNumber,
            NameSelector);
        var result2 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            MyViewModelTypeName,
            SrcTestcsName,
            CallerLineNumber,
            NameSelector);

        await Assert.That(result1).IsEqualTo(result2);
    }

    /// <summary>Verifies ComputeStableMethodSuffix produces different results for different discriminators.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeStableMethodSuffix_DifferentDiscriminator_ReturnsDifferentResult()
    {
        const int CallerLineNumber = 42;
        var result1 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            MyViewModelTypeName,
            SrcTestcsName,
            CallerLineNumber,
            NameSelector);
        var result2 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            MyViewModelTypeName,
            SrcTestcsName,
            CallerLineNumber,
            "x => x.Age");

        await Assert.That(result1).IsNotEqualTo(result2);
    }

    /// <summary>Verifies ComputeStableMethodSuffix produces different results for different line numbers.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeStableMethodSuffix_DifferentLineNumber_ReturnsDifferentResult()
    {
        const int CallerLineNumber = 10;
        const int CallerLineNumber2 = 20;
        var result1 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            MyViewModelTypeName,
            SrcTestcsName,
            CallerLineNumber);
        var result2 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            MyViewModelTypeName,
            SrcTestcsName,
            CallerLineNumber2);

        await Assert.That(result1).IsNotEqualTo(result2);
    }

    /// <summary>
    /// Verifies ComputePathSuffix returns the full path when only one slash is present
    /// (secondLastSlash is less than zero). Covers line 119.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputePathSuffix_OneSlash_ReturnsFullPath()
    {
        var result = CodeGeneratorHelpers.ComputePathSuffix("folder/MyFile.cs");

        await Assert.That(result).IsEqualTo("folder/MyFile.cs");
    }

    /// <summary>Verifies EscapeString handles strings with both quotes and backslashes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EscapeString_MixedSpecialChars_EscapesAll()
    {
        var result = CodeGeneratorHelpers.EscapeString("a\\\"b");

        await Assert.That(result).IsEqualTo("a\\\\\\\"b");
    }

    /// <summary>Verifies ConditionKeyword returns "if" for index 0.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ConditionKeyword_IndexZero_ReturnsIf() =>
        await Assert.That(CodeGeneratorHelpers.ConditionKeyword(0)).IsEqualTo("if");

    /// <summary>Verifies ConditionKeyword returns "else if" for index 1.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ConditionKeyword_IndexOne_ReturnsElseIf() =>
        await Assert.That(CodeGeneratorHelpers.ConditionKeyword(1)).IsEqualTo("else if");

    /// <summary>Verifies ConditionKeyword returns "else if" for index greater than 1.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ConditionKeyword_IndexGreaterThanOne_ReturnsElseIf()
    {
        const int LaterBranchIndex = 5;
        await Assert.That(CodeGeneratorHelpers.ConditionKeyword(LaterBranchIndex)).IsEqualTo("else if");
    }

    /// <summary>The declaration scan answers first when it saw the observed type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveObservedTypeInfo_TypeWasScanned_ReturnsTheScannedInfo()
    {
        var scanned = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        var result = CodeGeneratorHelpers.ResolveObservedTypeInfo(
            [scanned],
            scanned.FullyQualifiedName,
            new([]));

        await Assert.That(result).IsSameReferenceAs(scanned);
    }

    /// <summary>A type from a referenced assembly is absent from the scan, so the path answers instead.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveObservedTypeInfo_TypeNotScanned_ReadsThePathSegment()
    {
        var declaring = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var path = new EquatableArray<PropertyPathSegment>(
            [new("Name", "global::System.String", declaring.FullyQualifiedName, true, declaring)]);

        var result = CodeGeneratorHelpers.ResolveObservedTypeInfo(
            [],
            "global::Absent.Type",
            path);

        await Assert.That(result).IsSameReferenceAs(declaring);
    }

    /// <summary>With nothing scanned and no path to read, there is nothing to resolve.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveObservedTypeInfo_TypeNotScannedAndPathEmpty_ReturnsNull()
    {
        var result = CodeGeneratorHelpers.ResolveObservedTypeInfo(
            [],
            "global::Absent.Type",
            new([]));

        await Assert.That(result).IsNull();
    }

    /// <summary>A segment declared as the type the call site named is read straight off its parent.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendSegmentRead_SegmentNeedsNoNarrowing_ReadsThePropertyDirectly()
    {
        var segment = ModelFactory.CreatePropertyPathSegment(AddressName, MyViewModelTypeName);

        var result = CodeGeneratorHelpers.AppendSegmentRead("viewModel", segment);

        await Assert.That(result).IsEqualTo("viewModel.Address");
    }

    /// <summary>
    /// A view exposing its view model as a base still holds the view model the call site named, so the read
    /// narrows to it and the rest of the path continues from there.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendSegmentRead_SegmentCarriesAReadCast_NarrowsTheRead()
    {
        var segment = new PropertyPathSegment(
            "ViewModel",
            MyViewModelTypeName,
            "global::TestApp.MyView",
            true,
            null,
            MyViewModelTypeName);

        var result = CodeGeneratorHelpers.AppendSegmentRead("view", segment);

        await Assert.That(result).IsEqualTo("((global::TestApp.MyViewModel)(object)view.ViewModel)");
    }

    /// <summary>
    /// A file-and-line branch is the call site's condition followed by the return of its worker. The first branch of
    /// an overload opens with <c>if</c> and every later one with <c>else if</c>.
    /// </summary>
    /// <param name="index">The branch's position in the overload.</param>
    /// <param name="keyword">The keyword the branch is expected to open with.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(0, "if")]
    [Arguments(1, "else if")]
    [Arguments(2, "else if")]
    public async Task AppendCallerInfoDispatchBranch_WritesConditionThenWorkerReturn(int index, string keyword)
    {
        const int line = 42;
        const string worker = "__Worker_1";
        const string arguments = "view, viewModel";
        var expected = new SourceWriter().Indent();
        CodeGeneratorHelpers.AppendCallerInfoDispatchCondition(
            expected,
            index,
            line,
            CodeGeneratorHelpers.ComputePathSuffix(SrcTestcsName));
        CodeGeneratorHelpers.AppendDispatchReturn(expected, worker, arguments);
        var actual = new SourceWriter().Indent();

        CodeGeneratorHelpers.AppendCallerInfoDispatchBranch(actual, index, line, SrcTestcsName, worker, arguments);

        await Assert.That(actual.ToString()).IsEqualTo(expected.ToString());
        await Assert.That(actual.ToString()).Contains($"{keyword} (callerLineNumber == {line}");
        await Assert.That(actual.ToString()).Contains($"return {worker}({arguments});");
    }
}
