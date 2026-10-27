// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Checks allocation-free matching against nested, generic and nullable native symbols.</summary>
public class NativeTypeIdentityTests
{
    /// <summary>Names must match every namespace, nesting and arity segment.</summary>
    /// <param name="metadataName">The metadata lookup name.</param>
    /// <param name="expectedName">The proposed native name.</param>
    /// <param name="matches">Whether it identifies the native type.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Native.Control", "Native.Control", true)]
    [Arguments("Native.Control+Collection", "Native.Control.Collection", true)]
    [Arguments("Native.Control", "Other.Control", false)]
    [Arguments("Native.Control+Collection", "Native.Collection", false)]
    [Arguments("Native.Control`1", "Native.Control", false)]
    [Arguments("Native.Control", "Control", false)]
    public async Task Matches_UsesCompleteTypeIdentity(string metadataName, string expectedName, bool matches)
    {
        var compilation = TestHelper.CreateCompilation("namespace Native { public class Control { public class Collection {} } public class Control<T> {} }");
        var type = compilation.GetTypeByMetadataName(metadataName)!;
        await Assert.That(NativeTypeIdentity.Matches(type, expectedName)).IsEqualTo(matches);
        await Assert.That(NativeTypeIdentity.Matches(type.WithNullableAnnotation(NullableAnnotation.Annotated), expectedName)).IsEqualTo(matches);
    }
}
