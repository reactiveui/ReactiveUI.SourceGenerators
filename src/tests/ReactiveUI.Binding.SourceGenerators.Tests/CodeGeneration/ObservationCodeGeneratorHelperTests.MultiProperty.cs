// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="ObservationCodeGenerator"/> — multi-property observation.</summary>
public partial class ObservationCodeGeneratorHelperTests
{
    /// <summary>Verifies GenerateMultiPropertyObservation produces CombineLatest with multiple paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_MultiplePaths_GeneratesCombineLatest()
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
            returnTypeFullName: StringTypeName,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>([NameSelector, AgeSelector]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(CombineLatestName);
        await Assert.That(result).Contains("selector");
    }

    /// <summary>Verifies GenerateMultiPropertyObservation with a mix of shallow and deep chain paths generates both variable styles.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_WithDeepChainPath_GeneratesDeepChainVariables()
    {
        var sb = new SourceWriter();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment()
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Address", AddressTypeName),
                ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
            ])
        ]);

        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: StringTypeName,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>([NameSelector, "x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("__propObs0");
        await Assert.That(result).Contains("__propObs1");
        await Assert.That(result).Contains("SwitchMapSignal<");
        await Assert.That(result).Contains(CombineLatestName);
    }

    /// <summary>Verifies GenerateMultiPropertyObservation with isBeforeChange=true generates PropertyChanging code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_BeforeChange_GeneratesPropertyChangingCode()
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
            returnTypeFullName: StringTypeName,
            isBeforeChange: true,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>([NameSelector, AgeSelector]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains(CombineLatestName);
    }

    /// <summary>
    /// Verifies GenerateMultiPropertyObservation before-change with deep chain generates PropertyChanging variable declarations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_BeforeChange_WithDeepChain_GeneratesPropertyChangingVariables()
    {
        var sb = new SourceWriter();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment()
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Address", AddressTypeName),
                ModelFactory.CreatePropertyPathSegment("City", StringTypeName, AddressTypeName)
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: StringTypeName,
            isBeforeChange: true,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>([NameSelector, "x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains("__propObs1_s0");
        await Assert.That(result).Contains(CombineLatestName);
    }

    /// <summary>Verifies GenerateMultiPropertyObservation with IReactiveObject after-change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_ReactiveObjectAfterChange_GeneratesPropertyObservable()
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
            returnTypeFullName: StringTypeName,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>([NameSelector, AgeSelector]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains(CombineLatestName);
    }

    /// <summary>Verifies GenerateMultiPropertyObservation with null classInfo generates code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_NullClassInfo_GeneratesImmediateReturnSignal()
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
            returnTypeFullName: StringTypeName,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>([NameSelector, AgeSelector]));

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, null, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(CombineLatestName);
    }

    /// <summary>Verifies GenerateMultiPropertyObservation with no selector generates tuple lambda.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_NoSelector_GathersValuesIntoOneEmission()
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
            returnTypeFullName: "global::ReactiveUI.Binding.PropertyValues<global::System.String, global::System.Int32>",
            hasSelector: false,
            expressionTexts: new EquatableArray<string>([NameSelector, AgeSelector]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(CombineLatestName);
        await Assert.That(result).Contains(
            "(p1, p2) => new global::ReactiveUI.Binding.PropertyValues<global::System.String, global::System.Int32>(p1, p2)");
    }
}
