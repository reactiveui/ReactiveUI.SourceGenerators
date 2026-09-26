// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.WinForms;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Tests that the public attributes hold what they are given. The generators read the attributes from source, so these
/// are the only callers of their constructors and accessors.
/// </summary>
public class AttributeTests
{
    /// <summary>The base type named by the host attributes.</summary>
    private const string HostBaseType = "System.Windows.Forms.UserControl";

    /// <summary><c>[Reactive]</c> keeps its extra notifications and its options.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task ReactiveAttributeHoldsItsArguments()
    {
        var attribute = new ReactiveAttribute("First", "Second") { SetModifier = AccessModifier.Protected, Inheritance = InheritanceModifier.Virtual, UseRequired = true };

        await Assert.That(attribute.AlsoNotify).IsEquivalentTo(["First", "Second"]);
        await Assert.That(attribute.SetModifier).IsEqualTo(AccessModifier.Protected);
        await Assert.That(attribute.Inheritance).IsEqualTo(InheritanceModifier.Virtual);
        await Assert.That(attribute.UseRequired).IsTrue();
    }

    /// <summary><c>[Reactive]</c> without arguments notifies no other property.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task ReactiveAttributeWithoutArgumentsHasNoExtraNotifications() =>
        await Assert.That(new ReactiveAttribute().AlsoNotify).IsEmpty();

    /// <summary><c>[ReactiveCommand]</c> keeps its options.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task ReactiveCommandAttributeHoldsItsOptions()
    {
        var attribute = new ReactiveCommandAttribute
        {
            CanExecute = "CanRun",
            OutputScheduler = "Output",
            RunInBackground = true,
            BackgroundScheduler = "Background",
            AccessModifier = PropertyAccessModifier.Internal,
        };

        await Assert.That(attribute.CanExecute).IsEqualTo("CanRun");
        await Assert.That(attribute.OutputScheduler).IsEqualTo("Output");
        await Assert.That(attribute.RunInBackground).IsTrue();
        await Assert.That(attribute.BackgroundScheduler).IsEqualTo("Background");
        await Assert.That(attribute.AccessModifier).IsEqualTo(PropertyAccessModifier.Internal);
    }

    /// <summary><c>[BindableDerivedList]</c> keeps its access modifier.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task BindableDerivedListAttributeHoldsItsAccessModifier() =>
        await Assert.That(new BindableDerivedListAttribute { AccessModifier = PropertyAccessModifier.Private }.AccessModifier)
            .IsEqualTo(PropertyAccessModifier.Private);

    /// <summary>The attributes that take no arguments can be applied.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task ArgumentlessAttributesCanBeCreated()
    {
        await Assert.That(new ReactiveCollectionAttribute()).IsNotNull();
        await Assert.That(new IReactiveObjectAttribute()).IsNotNull();
    }

    /// <summary><c>[ViewModelControlHost]</c> keeps the base type it names.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task ViewModelControlHostAttributeHoldsItsBaseType() =>
        await Assert.That(new ViewModelControlHostAttribute(HostBaseType).BaseType).IsEqualTo(HostBaseType);

    /// <summary><c>[RoutedControlHost]</c> keeps the base type it names.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task RoutedControlHostAttributeHoldsItsBaseType() =>
        await Assert.That(new RoutedControlHostAttribute(HostBaseType).BaseType).IsEqualTo(HostBaseType);
}
