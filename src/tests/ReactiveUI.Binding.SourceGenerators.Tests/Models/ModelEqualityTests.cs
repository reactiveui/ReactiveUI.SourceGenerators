// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Models;

/// <summary>Tests the auto-generated record equality members on the internal pipeline model types.</summary>
public class ModelEqualityTests
{
    /// <summary>The fully qualified name of the <c>Int32</c> type used by these tests.</summary>
    private const string Int32TypeName = "global::System.Int32";

    /// <summary>The fully qualified name of the <c>String</c> type used by these tests.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>The fully qualified name of the <c>MyViewModel</c> type used by these tests.</summary>
    private const string MyViewModelTypeName = "global::TestApp.MyViewModel";

    /// <summary>The <c>string</c> name these tests generate against.</summary>
    private const string StringName = "string";

    // ───────────────────────────────────────────────────────────────────────────
    // ObservablePropertyInfo
    // ───────────────────────────────────────────────────────────────────────────
    /// <summary>Verifies that two ObservablePropertyInfo instances with identical values are equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_Equals_SameValues_ReturnsTrue()
    {
        var left = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);
        var right = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);

        await Assert.That(left.Equals(right)).IsTrue();
    }

    /// <summary>Verifies that two ObservablePropertyInfo instances with different values are not equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_Equals_DifferentValues_ReturnsFalse()
    {
        var left = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);
        var right = new ObservablePropertyInfo("Age", Int32TypeName, true, false, false, false);

        await Assert.That(left.Equals(right)).IsFalse();
    }

    /// <summary>Verifies that ObservablePropertyInfo.Equals returns false when compared to null object.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_Equals_ObjectNull_ReturnsFalse()
    {
        var left = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);

        await Assert.That(left.Equals(NullReference())).IsFalse();
    }

    /// <summary>Verifies that ObservablePropertyInfo.Equals returns false when compared to a different type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_Equals_ObjectWrongType_ReturnsFalse()
    {
        var left = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);

        await Assert.That(left.Equals(StringName)).IsFalse();
    }

    /// <summary>Verifies that two ObservablePropertyInfo instances with same values produce the same hash code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_GetHashCode_SameValues_AreEqual()
    {
        var left = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);
        var right = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);

        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    /// <summary>Verifies that operator== returns true for ObservablePropertyInfo instances with same values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_OperatorEquals_SameValues_ReturnsTrue()
    {
        var left = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);
        var right = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);

        await Assert.That(left == right).IsTrue();
    }

    /// <summary>Verifies that operator!= returns true for ObservablePropertyInfo instances with different values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var left = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);
        var right = new ObservablePropertyInfo("Name", StringTypeName, false, false, false, false);

        await Assert.That(left != right).IsTrue();
    }

    /// <summary>Verifies that ObservablePropertyInfo.ToString contains the type name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_ToString_ContainsTypeName()
    {
        var left = new ObservablePropertyInfo("Name", StringTypeName, true, false, false, false);

        await Assert.That(left.ToString()).Contains("ObservablePropertyInfo");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // InvocationInfo
    // ───────────────────────────────────────────────────────────────────────────
    /// <summary>Verifies that two InvocationInfo instances with identical values are equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_Equals_SameValues_ReturnsTrue()
    {
        var left = ModelFactory.CreateInvocationInfo();
        var right = ModelFactory.CreateInvocationInfo();

        await Assert.That(left.Equals(right)).IsTrue();
    }

    /// <summary>Verifies that two InvocationInfo instances with different values are not equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_Equals_DifferentValues_ReturnsFalse()
    {
        var left = ModelFactory.CreateInvocationInfo();
        var right = ModelFactory.CreateInvocationInfo(callerLineNumber: 99);

        await Assert.That(left.Equals(right)).IsFalse();
    }

    /// <summary>Verifies that InvocationInfo.Equals returns false when compared to null object.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_Equals_ObjectNull_ReturnsFalse()
    {
        var left = ModelFactory.CreateInvocationInfo();

        await Assert.That(left.Equals(NullReference())).IsFalse();
    }

    /// <summary>Verifies that InvocationInfo.Equals returns false when compared to a different type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_Equals_ObjectWrongType_ReturnsFalse()
    {
        var left = ModelFactory.CreateInvocationInfo();

        await Assert.That(left.Equals(StringName)).IsFalse();
    }

    /// <summary>Verifies that two InvocationInfo instances with same values produce the same hash code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_GetHashCode_SameValues_AreEqual()
    {
        var left = ModelFactory.CreateInvocationInfo();
        var right = ModelFactory.CreateInvocationInfo();

        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    /// <summary>Verifies that operator== returns true for InvocationInfo instances with same values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_OperatorEquals_SameValues_ReturnsTrue()
    {
        var left = ModelFactory.CreateInvocationInfo();
        var right = ModelFactory.CreateInvocationInfo();

        await Assert.That(left == right).IsTrue();
    }

    /// <summary>Verifies that operator!= returns true for InvocationInfo instances with different values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var left = ModelFactory.CreateInvocationInfo();
        var right = ModelFactory.CreateInvocationInfo(methodName: "WhenChanging");

        await Assert.That(left != right).IsTrue();
    }

    /// <summary>Verifies that InvocationInfo.ToString contains the type name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_ToString_ContainsTypeName()
    {
        var left = ModelFactory.CreateInvocationInfo();

        await Assert.That(left.ToString()).Contains("InvocationInfo");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // BindingInvocationInfo
    // ───────────────────────────────────────────────────────────────────────────
    /// <summary>Verifies that two BindingInvocationInfo instances with identical values are equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_Equals_SameValues_ReturnsTrue()
    {
        var left = ModelFactory.CreateBindingInvocationInfo();
        var right = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(left.Equals(right)).IsTrue();
    }

    /// <summary>Verifies that two BindingInvocationInfo instances with different values are not equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_Equals_DifferentValues_ReturnsFalse()
    {
        var left = ModelFactory.CreateBindingInvocationInfo();
        var right = ModelFactory.CreateBindingInvocationInfo(isTwoWay: true);

        await Assert.That(left.Equals(right)).IsFalse();
    }

    /// <summary>Verifies that BindingInvocationInfo.Equals returns false when compared to null object.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_Equals_ObjectNull_ReturnsFalse()
    {
        var left = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(left.Equals(NullReference())).IsFalse();
    }

    /// <summary>Verifies that BindingInvocationInfo.Equals returns false when compared to a different type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_Equals_ObjectWrongType_ReturnsFalse()
    {
        var left = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(left.Equals(StringName)).IsFalse();
    }

    /// <summary>Verifies that two BindingInvocationInfo instances with same values produce the same hash code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_GetHashCode_SameValues_AreEqual()
    {
        var left = ModelFactory.CreateBindingInvocationInfo();
        var right = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    /// <summary>Verifies that operator== returns true for BindingInvocationInfo instances with same values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_OperatorEquals_SameValues_ReturnsTrue()
    {
        var left = ModelFactory.CreateBindingInvocationInfo();
        var right = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(left == right).IsTrue();
    }

    /// <summary>Verifies that operator!= returns true for BindingInvocationInfo instances with different values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var left = ModelFactory.CreateBindingInvocationInfo();
        var right = ModelFactory.CreateBindingInvocationInfo(methodName: "BindTwoWay");

        await Assert.That(left != right).IsTrue();
    }

    /// <summary>Verifies that BindingInvocationInfo.ToString contains the type name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_ToString_ContainsTypeName()
    {
        var left = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(left.ToString()).Contains("BindingInvocationInfo");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // PropertyPathSegment
    // ───────────────────────────────────────────────────────────────────────────
    /// <summary>Verifies that two PropertyPathSegment instances with identical values are equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_Equals_SameValues_ReturnsTrue()
    {
        var left = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);
        var right = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);

        await Assert.That(left.Equals(right)).IsTrue();
    }

    /// <summary>Verifies that two PropertyPathSegment instances with different values are not equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_Equals_DifferentValues_ReturnsFalse()
    {
        var left = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);
        var right = new PropertyPathSegment("Age", Int32TypeName, MyViewModelTypeName, false, null);

        await Assert.That(left.Equals(right)).IsFalse();
    }

    /// <summary>Verifies that PropertyPathSegment.Equals returns false when compared to null object.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_Equals_ObjectNull_ReturnsFalse()
    {
        var left = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);

        await Assert.That(left.Equals(NullReference())).IsFalse();
    }

    /// <summary>Verifies that PropertyPathSegment.Equals returns false when compared to a different type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_Equals_ObjectWrongType_ReturnsFalse()
    {
        var left = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);

        await Assert.That(left.Equals(StringName)).IsFalse();
    }

    /// <summary>Verifies that two PropertyPathSegment instances with same values produce the same hash code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_GetHashCode_SameValues_AreEqual()
    {
        var left = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);
        var right = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);

        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    /// <summary>Verifies that operator== returns true for PropertyPathSegment instances with same values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_OperatorEquals_SameValues_ReturnsTrue()
    {
        var left = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);
        var right = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);

        await Assert.That(left == right).IsTrue();
    }

    /// <summary>Verifies that operator!= returns true for PropertyPathSegment instances with different values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var left = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);
        var right = new PropertyPathSegment("Name", StringTypeName, "global::TestApp.OtherType", true, null);

        await Assert.That(left != right).IsTrue();
    }

    /// <summary>Verifies that PropertyPathSegment.ToString contains the type name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_ToString_ContainsTypeName()
    {
        var left = new PropertyPathSegment("Name", StringTypeName, MyViewModelTypeName, true, null);

        await Assert.That(left.ToString()).Contains("PropertyPathSegment");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // ObservationCodeGenerator.TypeGroup
    // ───────────────────────────────────────────────────────────────────────────
    /// <summary>Verifies that two TypeGroup instances with identical values are equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_Equals_SameValues_ReturnsTrue()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var invocations = new[] { inv };

        var left = new ObservationCodeGenerator.TypeGroup(inv, invocations);
        var right = new ObservationCodeGenerator.TypeGroup(inv, invocations);

        await Assert.That(left.Equals(right)).IsTrue();
    }

    /// <summary>Verifies that two TypeGroup instances with different First values are not equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_Equals_DifferentValues_ReturnsFalse()
    {
        var invA = ModelFactory.CreateInvocationInfo(callerLineNumber: 10);
        var invB = ModelFactory.CreateInvocationInfo(callerLineNumber: 20);

        var left = new ObservationCodeGenerator.TypeGroup(invA, [invA]);
        var right = new ObservationCodeGenerator.TypeGroup(invB, [invB]);

        await Assert.That(left.Equals(right)).IsFalse();
    }

    /// <summary>Verifies that TypeGroup.Equals returns false when compared to null object.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_Equals_ObjectNull_ReturnsFalse()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var left = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        await Assert.That(left.Equals(NullReference())).IsFalse();
    }

    /// <summary>Verifies that TypeGroup.Equals returns false when compared to a different type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_Equals_ObjectWrongType_ReturnsFalse()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var left = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        await Assert.That(left.Equals(StringName)).IsFalse();
    }

    /// <summary>Verifies that two TypeGroup instances with same values produce the same hash code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_GetHashCode_SameValues_AreEqual()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var invocations = new[] { inv };

        var left = new ObservationCodeGenerator.TypeGroup(inv, invocations);
        var right = new ObservationCodeGenerator.TypeGroup(inv, invocations);

        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    /// <summary>Verifies that operator== returns true for TypeGroup instances with same values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_OperatorEquals_SameValues_ReturnsTrue()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var invocations = new[] { inv };

        var left = new ObservationCodeGenerator.TypeGroup(inv, invocations);
        var right = new ObservationCodeGenerator.TypeGroup(inv, invocations);

        await Assert.That(left == right).IsTrue();
    }

    /// <summary>Verifies that operator!= returns true for TypeGroup instances with different values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var invA = ModelFactory.CreateInvocationInfo(callerLineNumber: 10);
        var invB = ModelFactory.CreateInvocationInfo(callerLineNumber: 20);

        var left = new ObservationCodeGenerator.TypeGroup(invA, [invA]);
        var right = new ObservationCodeGenerator.TypeGroup(invB, [invB]);

        await Assert.That(left != right).IsTrue();
    }

    /// <summary>Verifies that TypeGroup.ToString contains the type name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_ToString_ContainsTypeName()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var left = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        await Assert.That(left.ToString()).Contains("TypeGroup");
    }

    /// <summary>Verifies that TypeGroup.SourceTypeFullName delegates to First.SourceTypeFullName.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_SourceTypeFullName_DelegatesToFirst()
    {
        var inv = ModelFactory.CreateInvocationInfo(sourceTypeFullName: "global::TestApp.SpecialViewModel");
        var left = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        await Assert.That(left.SourceTypeFullName).IsEqualTo("global::TestApp.SpecialViewModel");
    }

    /// <summary>Verifies that TypeGroup.ReturnTypeFullName delegates to First.ReturnTypeFullName.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_ReturnTypeFullName_DelegatesToFirst()
    {
        var inv = ModelFactory.CreateInvocationInfo(returnTypeFullName: Int32TypeName);
        var left = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        await Assert.That(left.ReturnTypeFullName).IsEqualTo(Int32TypeName);
    }

    /// <summary>
    /// Returns a null object reference through a method call, so the equality-with-null assertions exercise
    /// the generated <c>Equals(object?)</c> override at runtime without flow analysis folding the comparison
    /// to a constant (CA1508).
    /// </summary>
    /// <returns>A null object reference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object? NullReference() => null;
}
