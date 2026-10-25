// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="ObservationCodeGenerator"/> — single-property and shallow-path observation.</summary>
public partial class ObservationCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies GenerateSinglePropertyObservation generates INPC after-change code for ReactiveObject
    /// (ReactiveObject implements INPC, so the INPC path is used).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_ReactiveObjectAfterChange_GeneratesPropertyChangedHandler()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateSinglePropertyObservation(
            sb,
            inv,
            classInfo,
            ObjNameAccess,
            "Name",
            false);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyObservableName);
        await Assert.That(result).Contains(QuotedNameLiteral);
        await Assert.That(result).Contains("INotifyPropertyChanged");
    }

    /// <summary>
    /// Verifies GenerateSinglePropertyObservation generates INPChanging before-change code for ReactiveObject
    /// (ReactiveObject implements INPChanging, so the INPChanging path is used).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_ReactiveObjectBeforeChange_GeneratesPropertyChangingHandler()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateSinglePropertyObservation(
            sb,
            inv,
            classInfo,
            ObjNameAccess,
            "Name",
            true);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyChangingObservableName);
        await Assert.That(result).Contains(QuotedNameLiteral);
    }

    /// <summary>Verifies GenerateSinglePropertyObservation generates INPC after-change code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_INPCAfterChange_GeneratesPropertyChangedHandler()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateSinglePropertyObservation(
            sb,
            inv,
            classInfo,
            ObjNameAccess,
            "Name",
            false);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyObservableName);
        await Assert.That(result).Contains("INotifyPropertyChanged");
    }

    /// <summary>Verifies GenerateSinglePropertyObservation generates INPChanging before-change code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_INPChangingBeforeChange_GeneratesPropertyChangingHandler()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateSinglePropertyObservation(
            sb,
            inv,
            classInfo,
            ObjNameAccess,
            "Name",
            true);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyChangingObservableName);
        await Assert.That(result).Contains(INotifyPropertyChangingName);
    }

    /// <summary>Verifies GenerateSinglePropertyObservation reads a plain property through the runtime's deferred observable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_NoInterface_GeneratesObservableReturn()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo();

        ObservationCodeGenerator.GenerateSinglePropertyObservation(
            sb,
            inv,
            classInfo,
            ObjNameAccess,
            "Name",
            false);

        var result = sb.ToString();
        await Assert.That(result).Contains(DeferredPropertyObservableName);
    }

    /// <summary>Verifies GenerateShallowPathObservation for single segment delegates to single property logic.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_SingleSegment_GeneratesInlineObservation()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyObservableName);
        await Assert.That(result).Contains(QuotedNameLiteral);
    }

    /// <summary>Verifies GenerateShallowObservableVariable generates INPC variable for after-change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_INPCAfterChange_GeneratesVariableDeclaration()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, PropObs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropObs0Declaration);
        await Assert.That(result).Contains(PropertyObservableName);
    }

    /// <summary>Verifies GenerateShallowObservableVariable generates INPChanging variable for before-change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_INPChangingBeforeChange_GeneratesVariableDeclaration()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, true, PropObs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropObs0Declaration);
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains(INotifyPropertyChangingName);
    }

    /// <summary>Verifies GenerateShallowObservableVariable reads a plain property through the runtime's deferred observable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_NoInterface_GeneratesObservableReturn()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo();

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, PropObs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropObs0Declaration);
        await Assert.That(result).Contains(DeferredPropertyObservableName);
    }

    /// <summary>Verifies GenerateShallowPathObservation for before-change produces PropertyChanging code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_BeforeChange_GeneratesPropertyChangingCode()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains(INotifyPropertyChangingName);
    }

    /// <summary>
    /// A dependency object advertises the mechanism, but an inherited plain property takes no part in it. The
    /// type that declares the property is what knows that, and answering from the bound type alone would emit
    /// a companion field the property does not have - which only the consumer's build would discover.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_InheritedPlainProperty_DoesNotTakeTheDependencyPropertyPath()
    {
        var sb = new SourceWriter();
        var declaringType = ModelFactory.CreateClassBindingInfo(
            inheritsWpfDependencyObject: true,
            properties: new EquatableArray<ObservablePropertyInfo>(
                [ModelFactory.CreateObservablePropertyInfo(InheritedPropertyName)]));
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment(InheritedPropertyName, declaringTypeInfo: declaringType)]);

        // The bound type declares nothing of its own, so it can say only that it is a dependency object.
        var boundType = ModelFactory.CreateClassBindingInfo(inheritsWpfDependencyObject: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, boundType, false);

        await Assert.That(sb.ToString()).DoesNotContain($"{InheritedPropertyName}Property");
    }

    /// <summary>An inherited dependency property does take that path, because the companion field is inherited too.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_InheritedDependencyProperty_TakesTheDependencyPropertyPath()
    {
        var sb = new SourceWriter();
        var declaringType = ModelFactory.CreateClassBindingInfo(
            inheritsWpfDependencyObject: true,
            properties: new EquatableArray<ObservablePropertyInfo>(
                [ModelFactory.CreateObservablePropertyInfo(InheritedPropertyName, isDependencyProperty: true)]));
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment(InheritedPropertyName, declaringTypeInfo: declaringType)]);
        var boundType = ModelFactory.CreateClassBindingInfo(inheritsWpfDependencyObject: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, boundType, false);

        await Assert.That(sb.ToString()).Contains($"{InheritedPropertyName}Property");
    }

    /// <summary>Verifies GenerateShallowPathObservation with no interface reads through the runtime's deferred observable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_NoInterface_GeneratesObservableReturn()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo();

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(DeferredPropertyObservableName);
    }

    /// <summary>
    /// Verifies GenerateShallowPathObservation with null classInfo generates Observable.Return fallback.
    /// Covers the <c>classInfo?.ImplementsIReactiveObject ?? false</c> null-propagation branch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_NullClassInfo_GeneratesObservableReturn()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, null, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>
    /// Verifies GenerateShallowPathObservation with IReactiveObject after-change generates PropertyObservable.
    /// Covers the short-circuit OR path where ImplementsIReactiveObject is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_ReactiveObjectAfterChange_GeneratesPropertyObservable()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyObservableName);
    }

    /// <summary>Verifies GenerateShallowPathObservation with IReactiveObject before-change generates PropertyChangingObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_ReactiveObjectBeforeChange_GeneratesPropertyChangingObservable()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyChangingObservableName);
    }

    /// <summary>Verifies GenerateShallowObservableVariable with null classInfo generates ImmediateReturnSignal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_NullClassInfo_GeneratesTheUnchangingValue()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, null, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>Verifies GenerateShallowObservableVariable with IReactiveObject after-change generates PropertyObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_ReactiveObjectAfterChange_GeneratesPropertyObservable()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyObservableName);
    }

    /// <summary>Verifies GenerateShallowObservableVariable with IReactiveObject before-change generates PropertyChangingObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_ReactiveObjectBeforeChange_GeneratesPropertyChangingObservable()
    {
        var sb = new SourceWriter();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment()]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, true, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(PropertyChangingObservableName);
    }
}
