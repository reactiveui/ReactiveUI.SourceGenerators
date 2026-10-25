// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="ObservationCodeGenerator"/> — deep-chain and inline observation.</summary>
public partial class ObservationCodeGeneratorHelperTests
{
    /// <summary>Verifies GenerateDeepChainObservation generates a switch-map stage per segment of a two-level chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainObservation_TwoLevelChain_GeneratesSwitchMapStages()
    {
        var sb = new SourceWriter();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
                ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            expressionTexts: new EquatableArray<string>([CitySelector]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateDeepChainObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("__obs0");
        await Assert.That(result).Contains("__obs1");
        await Assert.That(result).Contains(SwitchSinkFragment);
        await Assert.That(result).Contains(UniqueSignalFragment);
    }

    /// <summary>
    /// Before-change observation is distinct too. The runtime engine asks for it whichever way it observes, so
    /// a before-change stream that repeated a value would emit where the runtime engine stayed quiet.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainObservation_BeforeChange_SuppressesRepeatedValues()
    {
        var sb = new SourceWriter();
        var leafInfo = ModelFactory.CreateClassBindingInfo(fullyQualifiedName: AddressTypeName, implementsINPChanging: true);
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
                ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName) with { DeclaringTypeInfo = leafInfo }
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            isBeforeChange: true,
            expressionTexts: new EquatableArray<string>([CitySelector]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true, implementsINPChanging: true);

        ObservationCodeGenerator.GenerateDeepChainObservation(sb, inv, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains(UniqueSignalName);
    }

    /// <summary>Verifies GenerateDeepChainVariable generates variable declarations for deep chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainVariable_TwoLevelChain_GeneratesVariableDeclarations()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
            ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, false, PropObs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains("var __propObs0_s0");
        await Assert.That(result).Contains("var __propObs0_s1");
        await Assert.That(result).Contains("var __propObs0");
        await Assert.That(result).Contains(SwitchSinkFragment);
        await Assert.That(result).Contains(UniqueSignalFragment);
    }

    /// <summary>
    /// Past two levels the stages differ: an intermediate parent going null pushes the leaf's default value on so
    /// the stage below re-parents and drops its subscription, while the leaf itself suppresses instead.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainVariable_ThreeLevelChain_OnlyTheLeafSuppressesAMissingParent()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
            ModelFactory.CreatePropertyPathSegment("Country", CountryTypeName, AddressTypeName),
            ModelFactory.CreatePropertyPathSegment("Name", StringTypeName, CountryTypeName)
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, false, PropObs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains("var __propObs0_s2");
        await Assert.That(result).Contains($"{ImmediateReturnSignalName}<{CountryTypeName}>(default({CountryTypeName}))");
        await Assert.That(result).Contains($"{ImmutableEmptySignalName}<{StringTypeName}>.Instance");
    }

    /// <summary>A before-change chain observes PropertyChanging, and suppresses repeated values as the runtime engine does.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainVariable_BeforeChange_GeneratesPropertyChangingCode()
    {
        var sb = new SourceWriter();
        var leafInfo = ModelFactory.CreateClassBindingInfo(fullyQualifiedName: AddressTypeName, implementsINPChanging: true);
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
            ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName) with { DeclaringTypeInfo = leafInfo }
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true, implementsINPChanging: true);

        ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, true, PropObs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains(UniqueSignalName);
    }

    /// <summary>Verifies EmitInlineObservation with single property INPC generates PropertyObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_SingleProperty_INPC_GeneratesPropertyObservable()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            SourceName,
            path,
            StringTypeName,
            classInfo,
            SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(SourceObsDeclaration);
        await Assert.That(result).Contains(PropertyObservableName);
    }

    /// <summary>Verifies EmitInlineObservation with single property and no INPC reads through the runtime's deferred observable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_SingleProperty_NoINPC_GeneratesTheDeferredValue()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo();

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            SourceName,
            path,
            StringTypeName,
            classInfo,
            SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(SourceObsDeclaration);
        await Assert.That(result).Contains(DeferredPropertyObservableName);
    }

    /// <summary>Verifies EmitInlineObservation with a deep chain generates a switch-map stage.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_DeepChain_GeneratesSwitchMapStage()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
            ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            SourceName,
            path,
            StringTypeName,
            classInfo,
            SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(SwitchSinkFragment);
        await Assert.That(result).Contains(UniqueSignalFragment);
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    /// <summary>Verifies GenerateDeepChainVariable with null classInfo generates after-change code with ImmediateReturnSignal fallback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainVariable_NullClassInfo_GeneratesAfterChangeCode()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
            ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
        ]);

        ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, null, false, PropObs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains("var __propObs0_s0");
        await Assert.That(result).Contains(ImmediateReturnSignalName);
    }

    /// <summary>Verifies GenerateDeepChainVariable with IReactiveObject after-change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainVariable_ReactiveObjectAfterChange_GeneratesPropertyObservable()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
            ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, false, PropObs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyObservableName);
        await Assert.That(result).Contains(UniqueSignalName);
    }

    /// <summary>Verifies GenerateDeepChainObservation with null classInfo generates after-change code with ImmediateReturnSignal fallback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainObservation_NullClassInfo_GeneratesCode()
    {
        var sb = new SourceWriter();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
                ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            expressionTexts: new EquatableArray<string>([CitySelector]));

        ObservationCodeGenerator.GenerateDeepChainObservation(sb, inv, null, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(ImmediateReturnSignalName);
        await Assert.That(result).Contains("Switch");
    }

    /// <summary>Verifies GenerateDeepChainObservation with IReactiveObject after-change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainObservation_ReactiveObjectAfterChange_GeneratesPropertyObservable()
    {
        var sb = new SourceWriter();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
                ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            expressionTexts: new EquatableArray<string>([CitySelector]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateDeepChainObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyObservableName);
        await Assert.That(result).Contains(UniqueSignalName);
    }

    /// <summary>Verifies EmitInlineObservation with null classInfo generates PropertyObservable (uses null-safe path).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_NullClassInfo_SingleProperty_GeneratesPropertyObservable()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);

        ObservationCodeGenerator.EmitInlineObservation(sb, SourceName, path, StringTypeName, null, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    /// <summary>Verifies EmitInlineObservation with IReactiveObject generates PropertyObservable for single property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_ReactiveObject_SingleProperty_GeneratesPropertyObservable()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            SourceName,
            path,
            StringTypeName,
            classInfo,
            SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyObservableName);
    }

    /// <summary>Verifies EmitInlineObservation with IReactiveObject and deep chain generates Switch pattern.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_ReactiveObject_DeepChain_GeneratesSwitchPattern()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
            ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            SourceName,
            path,
            StringTypeName,
            classInfo,
            SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(SwitchSinkFragment);
        await Assert.That(result).Contains(PropertyObservableName);
    }

    /// <summary>Verifies EmitInlineObservation with deep chain and null classInfo generates ImmediateReturnSignal fallback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_DeepChain_NullClassInfo_GeneratesImmediateReturnSignal()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment(AddressName, AddressTypeName),
            ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
        ]);

        ObservationCodeGenerator.EmitInlineObservation(sb, SourceName, path, StringTypeName, null, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(ImmediateReturnSignalName);
        await Assert.That(result).Contains("__sourceObs_s0");
        await Assert.That(result).Contains("__sourceObs_s1");
        await Assert.That(result).Contains("Switch");
        await Assert.That(result).Contains(UniqueSignalName);
    }
}
