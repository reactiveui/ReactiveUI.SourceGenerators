// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="ObservationCodeGenerator"/> helper methods.</summary>
public partial class ObservationCodeGeneratorHelperTests
{
    /// <summary>The fully qualified name of the <c>String</c> type used by these tests.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>The fully qualified name of the <c>Int32</c> type used by these tests.</summary>
    private const string Int32TypeName = "global::System.Int32";

    /// <summary>The fully qualified name of the <c>Address</c> type used by these tests.</summary>
    private const string AddressTypeName = "global::TestApp.Address";

    /// <summary>The <c>Address</c> name these tests generate against.</summary>
    private const string AddressName = "Address";

    /// <summary>The fully qualified name of the <c>Country</c> type used by these tests.</summary>
    private const string CountryTypeName = "global::TestApp.Country";

    /// <summary>The concrete distinct-value operator these tests generate against.</summary>
    private const string UniqueSignalName = "new global::ReactiveUI.Primitives.Advanced.UniqueSignal<";

    /// <summary>The <c>CombineLatest</c> name these tests generate against.</summary>
    private const string CombineLatestName = "CombineLatest";

    /// <summary>The <c>INotifyPropertyChanging</c> name these tests generate against.</summary>
    private const string INotifyPropertyChangingName = "INotifyPropertyChanging";

    /// <summary>The <c>ObservationAffinityChecker</c> name these tests generate against.</summary>
    private const string ObservationAffinityCheckerName = "ObservationAffinityChecker";

    /// <summary>The <c>PropertyObservable</c> name these tests generate against.</summary>
    private const string PropertyObservableName = "PropertyObservable";

    /// <summary>The <c>PropertyChangingObservable</c> name these tests generate against.</summary>
    private const string PropertyChangingObservableName = "PropertyChangingObservable";

    /// <summary>The <c>ImmediateReturnSignal</c> name these tests generate against.</summary>
    private const string ImmediateReturnSignalName = "ImmediateReturnSignal";

    /// <summary>The <c>ImmutableEmptySignal</c> name the leaf of a chain emits for a missing parent.</summary>
    private const string ImmutableEmptySignalName = "ImmutableEmptySignal";

    /// <summary>The observation a property with no mechanism for the asked timing gets: one value, without completing.</summary>
    private const string UnchangingPropertyObservableName = "UnchangingPropertyObservable";

    /// <summary>The observation a plain property gets: its value when a subscriber arrives, without completing.</summary>
    private const string DeferredPropertyObservableName = "global::ReactiveUI.Binding.Observables.DeferredPropertyObservable<";

    /// <summary>A property the bound type inherits rather than declares.</summary>
    private const string InheritedPropertyName = "Caption";

    /// <summary>The <c>WhenChanged</c> name these tests generate against.</summary>
    private const string WhenChangedName = "WhenChanged";

    /// <summary>The <c>WhenChanging</c> name these tests generate against.</summary>
    private const string WhenChangingName = "WhenChanging";

    /// <summary>The <c>source</c> name these tests generate against.</summary>
    private const string SourceName = "source";

    /// <summary>The <c>sourceObs</c> name these tests generate against.</summary>
    private const string SourceObsName = "sourceObs";

    /// <summary>The <c>var sourceObs</c> local the generated code is expected to emit.</summary>
    private const string SourceObsDeclaration = "var sourceObs";

    /// <summary>The <c>__obs0</c> local the generated code is expected to emit.</summary>
    private const string Obs0Local = "__obs0";

    /// <summary>The <c>__propObs0</c> local the generated code is expected to emit.</summary>
    private const string PropObs0Local = "__propObs0";

    /// <summary>The <c>var __propObs0</c> local the generated code is expected to emit.</summary>
    private const string PropObs0Declaration = "var __propObs0";

    /// <summary>The quoted <c>Name</c> property-name argument the generated code is expected to emit.</summary>
    private const string QuotedNameLiteral = "\"Name\"";

    /// <summary>The <c>obj.Name</c> property access the generated code is expected to emit.</summary>
    private const string ObjNameAccess = "obj.Name";

    /// <summary>The <c>x =&gt; x.Name</c> property selector these tests bind against.</summary>
    private const string NameSelector = "x => x.Name";

    /// <summary>The <c>x =&gt; x.Age</c> property selector these tests bind against.</summary>
    private const string AgeSelector = "x => x.Age";

    /// <summary>The <c>x =&gt; x.Address.City</c> property selector these tests bind against.</summary>
    private const string CitySelector = "x => x.Address.City";

    /// <summary>The typed distinct-value constructor these tests expect in the generated source.</summary>
    private const string UniqueSignalFragment = "new global::ReactiveUI.Primitives.Advanced.UniqueSignal<global::System.String>(";

    /// <summary>The projecting flattening sink these tests expect in the generated source.</summary>
    private const string SwitchSinkFragment = "SwitchMapSignal<";

    /// <summary>The <c>throw new global::System.InvalidOperationException</c> fragment these tests expect in the generated source.</summary>
    private const string ThrowNewGlobalSystemInvalidOperationExceptionFragment = "throw new global::System.InvalidOperationException";

    /// <summary>The opening of the call a generated overload ends on, up to the API name.</summary>
    private const string StubFallbackCallFragment = "return global::ReactiveUI.Binding.ReactiveUIBindingExtensions.";

    /// <summary>Verifies GetSelectorType returns correct Func type for single-property invocation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSelectorType_SingleProperty_ReturnsFuncType()
    {
        var inv = ModelFactory.CreateInvocationInfo(
            returnTypeFullName: StringTypeName,
            hasSelector: true);

        var result = ObservationCodeGenerator.GetSelectorType(inv);

        await Assert.That(result).IsEqualTo("global::System.Func<global::System.String, global::System.String>");
    }

    /// <summary>Verifies GetSelectorType returns correct Func type for multi-property invocation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSelectorType_MultiProperty_ReturnsFuncType()
    {
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment()
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Age", "global::System.Int32")
            ])
        ]);

        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: StringTypeName,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age"]));

        var result = ObservationCodeGenerator.GetSelectorType(inv);

        await Assert.That(result)
            .IsEqualTo("global::System.Func<global::System.String, global::System.Int32, global::System.String>");
    }

    /// <summary>Verifies GroupByTypeSignature groups invocations with the same source and return types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        const int ExpectedInvocationCount = 2;
        var inv1 = ModelFactory.CreateInvocationInfo(callerLineNumber: 10);
        var inv2 = ModelFactory.CreateInvocationInfo(callerLineNumber: 20);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = ObservationCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(1);
        await Assert.That(groups[0].Invocations.Length).IsEqualTo(ExpectedInvocationCount);
    }

    /// <summary>Verifies GroupByTypeSignature separates invocations with different return types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentReturnTypes_SeparateGroups()
    {
        const int ExpectedGroupCount = 2;
        var inv1 = ModelFactory.CreateInvocationInfo();
        var inv2 = ModelFactory.CreateInvocationInfo(returnTypeFullName: "global::System.Int32");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = ObservationCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(ExpectedGroupCount);
    }

    /// <summary>Verifies GenerateMultiPropertyObservation with deep chain and IReactiveObject before-change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task
        GenerateMultiPropertyObservation_ReactiveObjectBeforeChange_WithDeepChain_GeneratesPropertyChangingVariables()
    {
        var sb = new SourceWriter();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment()
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", StringTypeName, "global::TestApp.Address")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: StringTypeName,
            isBeforeChange: true,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains("CombineLatest");
    }

    /// <summary>Verifies IsINPC returns true when classInfo implements IReactiveObject.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPC_ReactiveObject_ReturnsTrue()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);
        await Assert.That(ObservationCodeGenerator.IsINPC(classInfo)).IsTrue();
    }

    /// <summary>Verifies IsINPC returns true when classInfo implements INPC directly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPC_DirectINPC_ReturnsTrue()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        await Assert.That(ObservationCodeGenerator.IsINPC(classInfo)).IsTrue();
    }

    /// <summary>Verifies IsINPC returns false when classInfo has no INPC support.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPC_NoINPC_ReturnsFalse()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo();
        await Assert.That(ObservationCodeGenerator.IsINPC(classInfo)).IsFalse();
    }

    /// <summary>Verifies IsINPC returns false when classInfo is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPC_NullClassInfo_ReturnsFalse() =>
        await Assert.That(ObservationCodeGenerator.IsINPC(null)).IsFalse();

    /// <summary>Verifies IsINPChanging returns true when classInfo implements IReactiveObject.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPChanging_ReactiveObject_ReturnsTrue()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);
        await Assert.That(ObservationCodeGenerator.IsINPChanging(classInfo)).IsTrue();
    }

    /// <summary>Verifies IsINPChanging returns true when classInfo implements INPChanging directly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPChanging_DirectINPChanging_ReturnsTrue()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);
        await Assert.That(ObservationCodeGenerator.IsINPChanging(classInfo)).IsTrue();
    }

    /// <summary>Verifies IsINPChanging returns false when classInfo has no INPChanging support.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPChanging_NoINPChanging_ReturnsFalse()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo();
        await Assert.That(ObservationCodeGenerator.IsINPChanging(classInfo)).IsFalse();
    }

    /// <summary>Verifies IsINPChanging returns false when classInfo is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPChanging_NullClassInfo_ReturnsFalse() =>
        await Assert.That(ObservationCodeGenerator.IsINPChanging(null)).IsFalse();

    /// <summary>Verifies GetTypeCastName returns the fully qualified name when classInfo is provided.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetTypeCastName_WithClassInfo_ReturnsFullyQualifiedName()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var result = ObservationCodeGenerator.GetTypeCastName(classInfo);
        await Assert.That(result).IsEqualTo(classInfo.FullyQualifiedName);
    }

    /// <summary>Verifies GetTypeCastName returns "object" when classInfo is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetTypeCastName_NullClassInfo_ReturnsObject()
    {
        var result = ObservationCodeGenerator.GetTypeCastName(null);
        await Assert.That(result).IsEqualTo("object");
    }
}
