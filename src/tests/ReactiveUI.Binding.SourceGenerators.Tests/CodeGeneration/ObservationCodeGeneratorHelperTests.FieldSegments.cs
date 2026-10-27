// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Tests for <see cref="ObservationCodeGenerator"/> — paths through fields. A field resolves no mechanism, so a
/// field on a type that notifies before a change is observed through the type's before-change event, and a field
/// along a chain is read from its parent.
/// </summary>
public partial class ObservationCodeGeneratorHelperTests
{
    /// <summary>The name of the field every test here observes.</summary>
    private const string LabelFieldName = "Label";

    /// <summary>The name of the intermediate field in the chain test.</summary>
    private const string LineFieldName = "Line";

    /// <summary>The type the intermediate field holds.</summary>
    private const string LineTypeName = "global::TestApp.Line";

    /// <summary>Verifies GenerateShallowPathObservation observes a field before the change through the owner's event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_FieldOnBeforeChangeType_UsesPropertyChanging()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([LabelField()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, true);

        await Assert.That(sb.ToString()).Contains(PropertyChangingObservableName);
    }

    /// <summary>Verifies GenerateShallowObservableVariable observes a field before the change through the owner's event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_FieldOnBeforeChangeType_UsesPropertyChanging()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([LabelField()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, true, PropObs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropObs0Declaration);
        await Assert.That(result).Contains(PropertyChangingObservableName);
    }

    /// <summary>Verifies GenerateSinglePropertyObservation observes a field before the change through the owner's event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_FieldOnBeforeChangeType_UsesPropertyChanging()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: new EquatableArray<EquatableArray<PropertyPathSegment>>([new([LabelField()])]),
            isBeforeChange: true,
            methodName: WhenChangingName);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateSinglePropertyObservation(sb, inv, classInfo, "obj.Label", LabelFieldName, true);

        await Assert.That(sb.ToString()).Contains(PropertyChangingObservableName);
    }

    /// <summary>
    /// Verifies EmitInlineObservation reads fields along a chain: an intermediate field passes its parent's absence
    /// on as a default, while a leaf field follows the requested broken-chain behavior.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_FieldsAlongAChain_EmitDefaultThenFollowTheLeafBehavior()
    {
        var sb = new SourceWriter();
        var root = ModelFactory.CreatePropertyPathSegment(
            AddressName,
            AddressTypeName,
            declaringTypeInfo: ModelFactory.CreateClassBindingInfo(implementsINPC: true));
        var line = ModelFactory.CreatePropertyPathSegment(LineFieldName, LineTypeName, AddressTypeName) with { IsField = true };
        var label = ModelFactory.CreatePropertyPathSegment(LabelFieldName, declaringType: LineTypeName) with { IsField = true };
        var path = new EquatableArray<PropertyPathSegment>([root, line, label]);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            SourceName,
            path,
            StringTypeName,
            ModelFactory.CreateClassBindingInfo(implementsINPC: true),
            SourceObsName,
            NullParentObservationBehavior.SuppressEmission);

        var result = sb.ToString();
        await Assert.That(result).Contains($"{ImmediateReturnSignalName}<{LineTypeName}>(default({LineTypeName}))");
        await Assert.That(result).Contains($"{ImmutableEmptySignalName}<{StringTypeName}>.Instance");
    }

    /// <summary>Creates the <c>Label</c> field segment declared on the default view model.</summary>
    /// <returns>The field segment.</returns>
    private static PropertyPathSegment LabelField() =>
        ModelFactory.CreatePropertyPathSegment(LabelFieldName) with { IsField = true };
}
