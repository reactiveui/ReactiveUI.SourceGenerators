// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="BindingEmitterHelpers"/>, shared by the four property-binding emitters.</summary>
public class BindingEmitterHelpersTests
{
    /// <summary>The fully qualified name of a source property type.</summary>
    private const string IntTypeName = "global::System.Int32";

    /// <summary>The fully qualified name of the view model a call site binds from.</summary>
    private const string ViewModelTypeName = "global::TestApp.MyViewModel";

    /// <summary>A type the view model derives from, which a view may expose it as.</summary>
    private const string ViewModelBaseTypeName = "global::TestApp.ViewModelBase";

    /// <summary>The name a view exposes its view model under.</summary>
    private const string ViewModelPropertyName = "ViewModel";

    /// <summary>The generated name of the view model a binding was handed.</summary>
    private const string ViewModelVariableName = "viewModel";

    /// <summary>The attribute that marks a selector's text parameter for expression-text dispatch.</summary>
    private const string CallerArgumentExpressionAttribute = "CallerArgumentExpression";

    /// <summary>The prefix every generated interceptor method's name starts with.</summary>
    private const string InterceptorMethodPrefix = "__Intercept_";

    /// <summary>A view exposing its view model as the concrete type is observed through it, unnarrowed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewModelObservation_ViewModelTypedExactly_ObservesTheViewWithoutNarrowing()
    {
        var observation = ResolveWithViewModelProperty(ViewModelTypeName);

        await Assert.That(observation.RootVariable).IsEqualTo("view");
        await Assert.That(observation.Path[0].ReadCastTypeFullName).IsNull();
    }

    /// <summary>A view exposing its view model as a base type is observed through that property, narrowed to the named type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewModelObservation_ViewModelTypedAsABase_ObservesTheViewAndNarrowsTheRead()
    {
        var observation = ResolveWithViewModelProperty(ViewModelBaseTypeName);

        await Assert.That(observation.RootVariable).IsEqualTo("view");
        await Assert.That(observation.Path[0].ReadCastTypeFullName).IsEqualTo(ViewModelTypeName);
    }

    /// <summary>The weakly typed view model the non-generic view interface declares is not followed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewModelObservation_ViewModelTypedAsObject_ObservesTheViewModelItWasHanded()
    {
        var observation = ResolveWithViewModelProperty("object");

        await Assert.That(observation.RootVariable).IsEqualTo(ViewModelVariableName);
    }

    /// <summary>A view exposing no view model at all leaves the binding on the one it was handed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewModelObservation_ViewDeclaresNoViewModel_ObservesTheViewModelItWasHanded()
    {
        var view = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var observation = BindingEmitterHelpers.ResolveViewModelObservation(
            ModelFactory.CreateBindingInvocationInfo(),
            ModelFactory.CreateClassBindingInfo(implementsINPC: true),
            view);

        await Assert.That(observation.RootVariable).IsEqualTo(ViewModelVariableName);
    }

    /// <summary>A supplied converter settles the conversion, so the registry is not asked for one.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RequiresRegistryConversion_ConverterSupplied_ReturnsFalse()
    {
        var invocation = ModelFactory.CreateBindingInvocationInfo(
            sourcePropertyTypeFullName: IntTypeName,
            hasConversion: true);

        await Assert.That(BindingEmitterHelpers.RequiresRegistryConversion(invocation)).IsFalse();
    }

    /// <summary>Two sides of the same type need no conversion at all.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RequiresRegistryConversion_SameTypeBothSides_ReturnsFalse()
    {
        var invocation = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(BindingEmitterHelpers.RequiresRegistryConversion(invocation)).IsFalse();
    }

    /// <summary>Differing sides with no supplied converter are what the registry exists to serve.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RequiresRegistryConversion_DifferingTypesNoConverter_ReturnsTrue()
    {
        var invocation = ModelFactory.CreateBindingInvocationInfo(sourcePropertyTypeFullName: IntTypeName);

        await Assert.That(BindingEmitterHelpers.RequiresRegistryConversion(invocation)).IsTrue();
    }

    /// <summary>A descriptor that sets neither a converter nor a scheduler emits nothing for them.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingDispatchApi_WithNoConversionOrSchedulerDeclared_EmitsNothingExtra()
    {
        var api = new BindingEmitterHelpers.BindingDispatchApi();
        var group = Group(false, IntTypeName, IntTypeName);
        var sb = new SourceWriter();

        api.AppendExtraParameters(sb, group, false);

        await Assert.That(sb.ToString()).IsEmpty();
        await Assert.That(api.FormatExtraArguments(group)).IsEmpty();
        await Assert.That(api.FormatWorkerParameters(ModelFactory.CreateBindingInvocationInfo())).IsEmpty();
    }

    /// <summary>A binding API that hands back a plain disposable says so by leaving both return types unset.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingDispatchApi_WithNoReturnTypeDeclared_HandsBackADisposable()
    {
        var api = new BindingEmitterHelpers.BindingDispatchApi();

        await Assert.That(api.FormatReturnType(Group(false, IntTypeName, IntTypeName))).IsEqualTo("global::System.IDisposable");
        await Assert.That(api.FormatWorkerReturnType(ModelFactory.CreateBindingInvocationInfo())).IsEqualTo("global::System.IDisposable");
    }

    /// <summary>The two objects a worker binds are named from the two parameters it declares them as.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingDispatchApi_WorkerArguments_NamesBothParametersInTheirDeclaredOrder()
    {
        var api = new BindingEmitterHelpers.BindingDispatchApi { WorkerSourceParameterName = ViewModelVariableName, WorkerTargetParameterName = "view" };

        await Assert.That(api.WorkerArguments).IsEqualTo($"{ViewModelVariableName}, view");
    }

    /// <summary>An interceptor for a compiler without caller-argument expressions declares no expression-text parameters.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateInterceptors_WithoutCallerArgumentExpressions_DeclaresNoExpressionTextParameters()
    {
        var sb = new SourceWriter();

        BindingEmitterHelpers.GenerateInterceptors(
            sb,
            InterceptedGroup(false),
            OneWayBindCodeGenerator.DispatchApi,
            new(SupportsCallerArgExpr: false, SupportsNullable: true, EmitGeneratedCodeMarkers: false, SupportsInterceptors: true));

        var output = sb.ToString();
        await Assert.That(output).Contains(InterceptorMethodPrefix);
        await Assert.That(output).DoesNotContain(CallerArgumentExpressionAttribute);
    }

    /// <summary>An interceptor for a call site that passes a converter object declares no expression-text parameters.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateInterceptors_WithConverterOverride_DeclaresNoExpressionTextParameters()
    {
        var sb = new SourceWriter();

        BindingEmitterHelpers.GenerateInterceptors(
            sb,
            InterceptedGroup(true),
            OneWayBindCodeGenerator.DispatchApi,
            new(SupportsCallerArgExpr: true, SupportsNullable: true, EmitGeneratedCodeMarkers: false, SupportsInterceptors: true));

        var output = sb.ToString();
        await Assert.That(output).Contains(InterceptorMethodPrefix);
        await Assert.That(output).DoesNotContain(CallerArgumentExpressionAttribute);
    }

    /// <summary>A two-way stage on an API that names only one converter hands that converter to both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitDualStreamStages_ApiWithOnlyAForwardConverter_ConvertsBothDirectionsWithIt()
    {
        var api = OneWayBindCodeGenerator.DispatchApi;
        var sb = new SourceWriter();

        var observables = BindingEmitterHelpers.EmitDualStreamStages(
            sb,
            api,
            ModelFactory.CreateBindingInvocationInfo(sourcePropertyTypeFullName: IntTypeName, isTwoWay: true, hasConverterOverride: true));

        await Assert.That(observables.TargetVar).IsNotEqualTo(api.TargetObservableName);
        await Assert.That(sb.ToString()).Contains(api.OverrideForwardName);
    }

    /// <summary>Resolves the observation for a view declaring its view model as the given type.</summary>
    /// <param name="declaredType">The type the view declares its view model property as.</param>
    /// <returns>The resolved observation.</returns>
    private static BindingEmitterHelpers.ViewModelObservation ResolveWithViewModelProperty(string declaredType)
    {
        var view = ModelFactory.CreateClassBindingInfo(
            implementsINPC: true,
            properties: new EquatableArray<ObservablePropertyInfo>(
                [ModelFactory.CreateObservablePropertyInfo(ViewModelPropertyName, declaredType)]));

        return BindingEmitterHelpers.ResolveViewModelObservation(
            ModelFactory.CreateBindingInvocationInfo(),
            ModelFactory.CreateClassBindingInfo(implementsINPC: true),
            view);
    }

    /// <summary>Builds a group fixing both property types and whether a converter was supplied.</summary>
    /// <param name="hasConversion">Whether the call site supplied a converter.</param>
    /// <param name="sourceType">The fully qualified source property type.</param>
    /// <param name="targetType">The fully qualified target property type.</param>
    /// <returns>The binding type group.</returns>
    private static BindingTypeGroup Group(bool hasConversion, string sourceType, string targetType) =>
        new(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            sourceType,
            targetType,
            hasConversion,
            false,
            [ModelFactory.CreateBindingInvocationInfo()]);

    /// <summary>Builds a group whose one call site the compiler can intercept.</summary>
    /// <param name="hasConverterOverride">Whether the call site passed a converter object.</param>
    /// <returns>The binding type group.</returns>
    private static BindingTypeGroup InterceptedGroup(bool hasConverterOverride) =>
        new(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            IntTypeName,
            IntTypeName,
            false,
            false,
            [ModelFactory.CreateBindingInvocationInfo(hasConverterOverride: hasConverterOverride) with { Interceptor = new(1, "location") }])
        { HasConverterOverride = hasConverterOverride };
}
