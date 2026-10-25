// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="ObservationCodeGenerator"/> — overload, method, runtime-fallback and affinity generation.</summary>
public partial class ObservationCodeGeneratorHelperTests
{
    /// <summary>The local a generated observation method assigns its first observed property to.</summary>
    private const string ObservedPropertyVariable = "__propObs0";

    /// <summary>Verifies GenerateConcreteOverload with CallerArgExpr mode.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesExpressionDispatch()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, false, true, WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__WhenChanged_");
    }

    /// <summary>Verifies GenerateConcreteOverload with CallerFilePath mode.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerFilePath_GeneratesFilePathDispatch()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, false, false, false, WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).Contains("callerLineNumber");
        await Assert.That(result).Contains("callerFilePath.EndsWith");
    }

    /// <summary>A call site the overload could not match reaches the stub rather than a throw.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateRuntimeFallback_CallsTheStubTheOverloadDisplaces()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, inv, WhenChangedName, inv.PropertyPaths.Length, false);

        var result = sb.ToString();
        await Assert.That(result).Contains($"{StubFallbackCallFragment}{WhenChangedName}");
        await Assert.That(result).DoesNotContain(ThrowNewGlobalSystemInvalidOperationExceptionFragment);
        await Assert.That(result).Contains("(objectToMonitor, property1);");
    }

    /// <summary>The stub the fallback names follows the API being generated.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateRuntimeFallback_WhenChanging_NamesTheWhenChangingStub()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, inv, WhenChangingName, inv.PropertyPaths.Length, false);

        await Assert.That(sb.ToString()).Contains($"{StubFallbackCallFragment}{WhenChangingName}");
    }

    /// <summary>The projected type is stated last, after the observed ones, where the overload takes a selector.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateRuntimeFallback_WithSelector_StatesTheProjectedTypeLastAndForwardsIt()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, inv, WhenChangedName, inv.PropertyPaths.Length, true);

        var result = sb.ToString();
        await Assert.That(result).Contains($"{inv.ReturnTypeFullName}>(");
        await Assert.That(result).Contains(", selector);");
    }

    /// <summary>Verifies GenerateObservationMethod with a deep chain and selector generates Switch and Select wrapping.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateObservationMethod_DeepChainWithSelector_GeneratesSwitchPattern()
    {
        var sb = new SourceWriter();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", StringTypeName, "global::TestApp.Address")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: Int32TypeName,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateObservationMethod(sb, inv, classInfo, "DEADBEEF", false, WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).Contains("SwitchMapSignal<");
        await Assert.That(result).Contains("__WhenChanged_DEADBEEF");
    }

    /// <summary>A single-property selector constructs a map with the observed and projected types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateObservationMethod_SinglePropertyWithSelector_GeneratesMapSignal()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo(
            returnTypeFullName: Int32TypeName,
            hasSelector: true);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateObservationMethod(sb, inv, classInfo, "CAFEBABE", false, WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).Contains("new global::ReactiveUI.Primitives.Signals.MapSignal<global::System.String, global::System.Int32>(");
        await Assert.That(result).Contains("selector");
    }

    /// <summary>Verifies GenerateObservationMethod with null classInfo generates code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateObservationMethod_NullClassInfo_GeneratesCode()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();

        ObservationCodeGenerator.GenerateObservationMethod(sb, inv, null, "DEADBEEF", false, WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).Contains("__WhenChanged_DEADBEEF");
    }

    /// <summary>Verifies GenerateObservationMethod single property without selector generates direct return.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateObservationMethod_SinglePropertyNoSelector_GeneratesDirectReturn()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateObservationMethod(sb, inv, classInfo, "ABC123", false, WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).Contains("__WhenChanged_ABC123");
        await Assert.That(result).Contains("PropertyObservable");
    }

    /// <summary>Verifies Generate with empty invocations returns null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_EmptyInvocations_ReturnsNull()
    {
        var result = ObservationCodeGenerator.Generate(
            [],
            [],
            new(true, true, true),
            WhenChangedName);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies Generate with default invocations returns null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_DefaultInvocations_ReturnsNull()
    {
        var result = ObservationCodeGenerator.Generate(
            default,
            [],
            new(true, true, true),
            WhenChangedName);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies Generate with valid invocations returns non-null source containing method prefix.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_WithInvocations_ReturnsNonNullSource()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        var result = ObservationCodeGenerator.Generate(
            [inv],
            [classInfo],
            new(true, true, true),
            WhenChangedName);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).Contains("__WhenChanged_");
    }

    /// <summary>A registered provider can observe a property even when no generated mechanism matches its type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_NoMatchingClassInfo_AllowsCustomProvider()
    {
        var inv = ModelFactory.CreateInvocationInfo();

        var result = ObservationCodeGenerator.Generate(
            [inv],
            [],
            new(true, true, true),
            WhenChangedName);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).Contains(ObservationAffinityCheckerName);
    }

    /// <summary>Verifies GenerateConcreteOverload with multiple invocations in a group generates else if branching.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_MultipleInvocationsInGroup_GeneratesElseIf()
    {
        var sb = new SourceWriter();
        var inv1 = ModelFactory.CreateInvocationInfo(callerLineNumber: 10, expressionTexts: new EquatableArray<string>([
            NameSelector
        ]));
        var inv2 = ModelFactory.CreateInvocationInfo(callerLineNumber: 20, expressionTexts: new EquatableArray<string>([
            AgeSelector
        ]));
        var group = new ObservationCodeGenerator.TypeGroup(inv1, [inv1, inv2]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, false, true, WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).Contains("if (");
        await Assert.That(result).Contains("else if (");
    }

    /// <summary>Verifies GenerateConcreteOverload with selector generates selector parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_WithSelector_GeneratesSelectorParameter()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo(returnTypeFullName: Int32TypeName, hasSelector: true);
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, false, true, WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).Contains("Func<");
        await Assert.That(result).Contains("selector");
    }

    /// <summary>Verifies GenerateConcreteOverload with CallerArgExpr and multi-property generates multiple expression checks with AND.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_MultiProperty_GeneratesMultipleExpressionChecks()
    {
        var sb = new SourceWriter();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment()
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Age", Int32TypeName)
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "(global::System.String, global::System.Int32)",
            hasSelector: false,
            expressionTexts: new EquatableArray<string>([NameSelector, AgeSelector]));
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, false, true, WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).Contains("property1Expression");
        await Assert.That(result).Contains("property2Expression");
        await Assert.That(result).Contains("&&");
    }

    /// <summary>Verifies GenerateObservationMethod generates method signature with correct prefix and suffix.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateObservationMethod_SingleProperty_GeneratesMethodSignature()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateObservationMethod(
            sb,
            inv,
            classInfo,
            "ABCDEF0123456789",
            false,
            WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).Contains("__WhenChanged_ABCDEF0123456789");
        await Assert.That(result).Contains("private static");
        await Assert.That(result).Contains("global::System.IObservable");
    }

    /// <summary>A registration that outranks the generated mechanism observes the property instead of it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_RegistrationOutranksTheMechanism_ObservesThroughTheRegistration()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([ModelFactory.CreatePropertyPathSegment()]);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, ModelFactory.CreateClassBindingInfo(implementsINPC: true), false, ObservedPropertyVariable);

        var result = sb.ToString();
        await Assert.That(result).Contains("FindHigherAffinityPlugin");
        await Assert.That(result).Contains("__propObs0Registration == null");
        await Assert.That(result).Contains("PluginPropertyObservable<");
    }

    /// <summary>The value is read through an emitted accessor rather than off the notification.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_RegistrationOutranksTheMechanism_ReadsThroughAnEmittedAccessor()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([ModelFactory.CreatePropertyPathSegment()]);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, ModelFactory.CreateClassBindingInfo(implementsINPC: true), false, ObservedPropertyVariable);

        await Assert.That(sb.ToString()).Contains("(object __o) => ((");
    }

    /// <summary>
    /// The expression handed to a registration is a lambda the compiler builds, so the member is a token
    /// rather than a name resolved at run time and an ahead-of-time consumer keeps nothing extra alive.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_RegistrationOutranksTheMechanism_HandsItACompiledExpression()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([ModelFactory.CreatePropertyPathSegment()]);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, ModelFactory.CreateClassBindingInfo(implementsINPC: true), false, ObservedPropertyVariable);

        await Assert.That(sb.ToString()).Contains("(__e => __e.Name)).Body");
    }

    /// <summary>Before-change observation asks the registration for before-change notifications.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_BeforeChange_AsksTheRegistrationForBeforeChange()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([ModelFactory.CreatePropertyPathSegment()]);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, ModelFactory.CreateClassBindingInfo(implementsINPChanging: true), true, ObservedPropertyVariable);

        await Assert.That(sb.ToString()).Contains("\", 5, true);");
    }

    /// <summary>A binding reads through the same choice, so its generated write is untouched by a registration.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_ShallowPath_ReadsThroughTheRegistrationChoice()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>([ModelFactory.CreatePropertyPathSegment()]);

        ObservationCodeGenerator.EmitInlineObservation(sb, "source", path, StringTypeName, ModelFactory.CreateClassBindingInfo(implementsINPC: true), "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("sourceObsRegistration");
        await Assert.That(result).Contains("PluginPropertyObservable<");
    }

    /// <summary>
    /// The dispatch decides nothing about registrations, so it never names the runtime engine and an
    /// ahead-of-time consumer carries no expression machinery for a branch it does not take.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_Always_LeavesTheRegistrationToTheObservation()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, false, true, WhenChangedName);

        var result = sb.ToString();
        await Assert.That(result).DoesNotContain(ObservationAffinityCheckerName);
        await Assert.That(result).DoesNotContain("RuntimeObservationFallback");
    }
}
