// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Tests for <see cref="ObservedProperties"/>, which decides what a mechanism reaches on one property.</summary>
public class ObservedPropertiesTests
{
    /// <summary>The property these tests ask about.</summary>
    private const string ObservedName = "Name";

    /// <summary>A property name the type under test never declares.</summary>
    private const string UndeclaredName = "Absent";

    /// <summary>A property carrying a dependency-property field participates in the mechanism.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsDependencyProperty_DeclaredWithField_ReturnsTrue()
    {
        var classInfo = WithProperty(isDependencyProperty: true, hasChangeEvent: false);

        await Assert.That(ObservedProperties.IsDependencyProperty(classInfo, ObservedName)).IsTrue();
    }

    /// <summary>A plain CLR property on a dependency object does not participate.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsDependencyProperty_DeclaredWithoutField_ReturnsFalse()
    {
        var classInfo = WithProperty(isDependencyProperty: false, hasChangeEvent: false);

        await Assert.That(ObservedProperties.IsDependencyProperty(classInfo, ObservedName)).IsFalse();
    }

    /// <summary>A property the type does not declare is unknown rather than absent, so it stays observable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsDependencyProperty_Undeclared_ReturnsTrue()
    {
        var classInfo = WithProperty(isDependencyProperty: false, hasChangeEvent: false);

        await Assert.That(ObservedProperties.IsDependencyProperty(classInfo, UndeclaredName)).IsTrue();
    }

    /// <summary>A property carrying a companion change event participates in the mechanism.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasChangeEvent_DeclaredWithEvent_ReturnsTrue()
    {
        var classInfo = WithProperty(isDependencyProperty: false, hasChangeEvent: true);

        await Assert.That(ObservedProperties.HasChangeEvent(classInfo, ObservedName)).IsTrue();
    }

    /// <summary>A component property with no companion event does not participate.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasChangeEvent_DeclaredWithoutEvent_ReturnsFalse()
    {
        var classInfo = WithProperty(isDependencyProperty: false, hasChangeEvent: false);

        await Assert.That(ObservedProperties.HasChangeEvent(classInfo, ObservedName)).IsFalse();
    }

    /// <summary>A property the type does not declare is unknown rather than absent, so it stays observable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasChangeEvent_Undeclared_ReturnsTrue()
    {
        var classInfo = WithProperty(isDependencyProperty: false, hasChangeEvent: false);

        await Assert.That(ObservedProperties.HasChangeEvent(classInfo, UndeclaredName)).IsTrue();
    }

    /// <summary>The declared property is found past earlier entries that do not match.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasChangeEvent_DeclaredAfterAnotherProperty_ReadsTheMatchingOne()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(
            properties: new EquatableArray<ObservablePropertyInfo>(
            [
                Property("Other", false, false),
                Property(ObservedName, false, true),
            ]));

        await Assert.That(ObservedProperties.HasChangeEvent(classInfo, ObservedName)).IsTrue();
    }

    /// <summary>A type declaring no properties at all answers unknown for anything asked of it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasChangeEvent_NoDeclaredProperties_ReturnsTrue()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(
            properties: new EquatableArray<ObservablePropertyInfo>([]));

        await Assert.That(ObservedProperties.HasChangeEvent(classInfo, ObservedName)).IsTrue();
    }

    /// <summary>Builds one declared property with the given participation.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="isDependencyProperty">Whether the property has a companion dependency-property field.</param>
    /// <param name="hasChangeEvent">Whether the property has a companion change event.</param>
    /// <returns>The property info.</returns>
    private static ObservablePropertyInfo Property(string name, bool isDependencyProperty, bool hasChangeEvent) =>
        new(name, "global::System.String", true, false, isDependencyProperty, hasChangeEvent);

    /// <summary>Builds a type declaring one property with the given participation.</summary>
    /// <param name="isDependencyProperty">Whether the property has a companion dependency-property field.</param>
    /// <param name="hasChangeEvent">Whether the property has a companion change event.</param>
    /// <returns>The class binding info.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ClassBindingInfo WithProperty(bool isDependencyProperty, bool hasChangeEvent) =>
        ModelFactory.CreateClassBindingInfo(
            properties: new EquatableArray<ObservablePropertyInfo>(
                [Property(ObservedName, isDependencyProperty, hasChangeEvent)]));
}
