// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Verifies notification dispatch when ReactiveUI's interface and extensions ship separately.</summary>
public sealed class ToPropertyUnsafeSplitAssemblyTests
{
    /// <summary>The observable helper's initial value.</summary>
    private const string Initial = "initial";

    /// <summary>The first value delivered by the source.</summary>
    private const string First = "first";

    /// <summary>The two notifications raised for one value, in order.</summary>
    private const string NotificationPair = "changing:Name,changed:Name";

    /// <summary>The original framework's extension class.</summary>
    private const string ExtensionsName = "ReactiveUI.IReactiveObjectExtensions";

    /// <summary>The after-change raise method name.</summary>
    private const string ChangedMethod = "RaisePropertyChanged";

    /// <summary>The sender's generic parameter name.</summary>
    private const string SenderParameter = "TSender";

    /// <summary>Records notifications emitted by the separately loaded extensions.</summary>
    /// <param name="owner">The notification owner.</param>
    /// <param name="name">The property name.</param>
    /// <param name="changing">Whether the notification precedes the change.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Record(object owner, string name, bool changing) =>
        ReadNotifications(owner).Add($"{(changing ? "changing" : "changed")}:{name}");

    /// <summary>Both notification phases use the extension assembly belonging to the model's base type.</summary>
    /// <param name="extensionsName">The flavour's extension class name.</param>
    /// <param name="derived">Whether the consumer subclasses the ReactiveUI model in a third assembly.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("ReactiveUI.IReactiveObjectExtensions", false)]
    [Arguments("ReactiveUI.IReactiveObjectExtensions", true)]
    [Arguments("ReactiveUI.Reactive.IReactiveObjectExtensions", false)]
    [Arguments("ReactiveUI.Reactive.IReactiveObjectExtensions", true)]
    public async Task SplitAssembly_RaisesChangingThenChanged(string extensionsName, bool derived)
    {
        var model = (SplitModel)CreateModel(extensionsName, derived);
        using var source = new Subject<string>();
        using var helper = source.ToPropertyUnsafe(model, nameof(SplitModel.Name), Initial);
        model.Helper = helper;
        model.Raised.Clear();

        source.OnNext(First);
        source.OnNext("second");

        await Assert.That(model.Name).IsEqualTo("second");
        await Assert.That(string.Join(",", model.Raised)).IsEqualTo("changing:Name,changed:Name,changing:Name,changed:Name");
        await Assert.That(model.GetType().GetInterface("ReactiveUI.IReactiveObject")!.Assembly == model.GetType().Assembly).IsFalse();
    }

    /// <summary>An implementation without framework extensions falls back to its own notification methods.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MissingExtensions_RaisesThroughMethods()
    {
        var model = CreateModel(null, true);
        using var source = new Subject<string>();
        using var helper = source.ToPropertyUnsafe(model, nameof(SplitModel.Name), Initial);
        var raised = ReadNotifications(model);
        raised.Clear();
        source.OnNext(First);

        await Assert.That(helper.Value).IsEqualTo(First);
        await Assert.That(string.Join(",", raised)).IsEqualTo(NotificationPair);
    }

    /// <summary>The original layout still resolves extensions from the interface assembly.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InterfaceAssemblyExtensions_RaiseBothNotifications()
    {
        var model = CreateModel(ExtensionsName, true, true);
        using var source = new Subject<string>();
        using var helper = source.ToPropertyUnsafe(model, nameof(SplitModel.Name), Initial);
        var raised = ReadNotifications(model);
        raised.Clear();
        source.OnNext(First);

        await Assert.That(helper.Value).IsEqualTo(First);
        await Assert.That(string.Join(",", raised)).IsEqualTo(NotificationPair);
    }

    /// <summary>The System.Reactive runtime resolves the same split assembly notification layouts.</summary>
    /// <param name="extensionsName">The framework flavour's extension class.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("ReactiveUI.IReactiveObjectExtensions")]
    [Arguments("ReactiveUI.Reactive.IReactiveObjectExtensions")]
    public async Task ReactiveRuntime_SplitAssembly_RaisesBothNotifications(string extensionsName)
    {
        var model = (SplitModel)CreateModel(extensionsName, true);
        using var source = new Subject<string>();
        using var helper = ReactiveUI.Binding.Reactive.ReactiveUIBindingExtensions.ToPropertyUnsafe(source, model, nameof(SplitModel.Name), Initial);
        model.Raised.Clear();
        source.OnNext(First);

        await Assert.That(helper.Value).IsEqualTo(First);
        await Assert.That(string.Join(",", model.Raised)).IsEqualTo(NotificationPair);
    }

    /// <summary>Extensions returning a value remain callable by the reflection fallback.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ReturningExtensions_RaiseBothNotifications()
    {
        var model = (SplitModel)CreateModel(ExtensionsName, true, returnsValue: true);
        using var source = new Subject<string>();
        using var helper = source.ToPropertyUnsafe(model, nameof(SplitModel.Name), Initial);
        model.Raised.Clear();
        source.OnNext(First);

        await Assert.That(helper.Value).IsEqualTo(First);
        await Assert.That(string.Join(",", model.Raised)).IsEqualTo(NotificationPair);
    }

    /// <summary>A framework without a before-change extension still raises its after-change notification.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ChangedOnlyExtensions_RaiseAfterChange()
    {
        var model = (SplitModel)CreateModel(ExtensionsName, true, changedOnly: true);
        using var source = new Subject<string>();
        using var helper = source.ToPropertyUnsafe(model, nameof(SplitModel.Name), Initial);
        model.Raised.Clear();
        source.OnNext(First);

        await Assert.That(helper.Value).IsEqualTo(First);
        await Assert.That(string.Join(",", model.Raised)).IsEqualTo("changed:Name");
    }

    /// <summary>Creates an interface assembly, a framework assembly, and optionally a consumer assembly.</summary>
    /// <param name="extensionsName">The extensions' full name.</param>
    /// <param name="derived">Whether to add a consumer subclass.</param>
    /// <param name="interfaceExtensions">Whether extensions use the legacy interface assembly layout.</param>
    /// <param name="returnsValue">Whether extensions return the property name.</param>
    /// <param name="changedOnly">Whether the framework supplies only after-change extensions.</param>
    /// <returns>The independently compiled model.</returns>
    private static object CreateModel(string? extensionsName, bool derived, bool interfaceExtensions = false, bool returnsValue = false, bool changedOnly = false)
    {
        var core = CreateModule();
        var interfaceBuilder = core.DefineType("ReactiveUI.IReactiveObject", TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
        var reactiveObject = interfaceBuilder.CreateType()!;
        var framework = CreateModule();
        if (extensionsName is not null)
        {
            DefineExtensions(interfaceExtensions ? core : framework, extensionsName, reactiveObject, returnsValue, changedOnly);
        }

        var isolated = extensionsName is null || interfaceExtensions;
        var modelBuilder = framework.DefineType("ReactiveUI.ReactiveObject", TypeAttributes.Public, isolated ? typeof(object) : typeof(SplitModel), [reactiveObject]);
        if (isolated)
        {
            _ = modelBuilder.DefineField(nameof(SplitModel.Raised), typeof(List<string>), FieldAttributes.Public);
            if (extensionsName is null)
            {
                DefineRaiseMethods(modelBuilder);
            }
        }

        _ = modelBuilder.DefineDefaultConstructor(MethodAttributes.Public);
        var modelType = modelBuilder.CreateType()!;
        if (derived)
        {
            var consumer = CreateModule().DefineType("Consumer.ViewModel", TypeAttributes.Public | TypeAttributes.Sealed, modelType);
            _ = consumer.DefineDefaultConstructor(MethodAttributes.Public);
            modelType = consumer.CreateType()!;
        }

        var model = Activator.CreateInstance(modelType)!;
        if (isolated)
        {
            modelType.GetField(nameof(SplitModel.Raised))!.SetValue(model, new List<string>());
        }

        return model;
    }

    /// <summary>Declares the framework's generic raise extensions against the separate interface.</summary>
    /// <param name="module">The framework or core module.</param>
    /// <param name="name">The extension class name.</param>
    /// <param name="reactiveObject">The interface constraint.</param>
    /// <param name="returnsValue">Whether the methods return the name after raising.</param>
    /// <param name="changedOnly">Whether to omit the before-change method.</param>
    private static void DefineExtensions(ModuleBuilder module, string name, Type reactiveObject, bool returnsValue, bool changedOnly)
    {
        var extensions = module.DefineType(name, TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
        DefineUnrelatedOverloads(extensions);
        foreach (var changing in changedOnly ? new[] { false } : new[] { false, true })
        {
            var method = extensions.DefineMethod(changing ? "RaisePropertyChanging" : ChangedMethod, MethodAttributes.Public | MethodAttributes.Static);
            var sender = method.DefineGenericParameters(SenderParameter)[0];
            sender.SetInterfaceConstraints(reactiveObject);
            method.SetReturnType(returnsValue ? typeof(string) : typeof(void));
            method.SetParameters(sender, typeof(string));
            var body = method.GetILGenerator();
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Box, sender);
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(changing ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            body.Emit(OpCodes.Call, typeof(ToPropertyUnsafeSplitAssemblyTests).GetMethod(nameof(Record))!);
            if (returnsValue)
            {
                body.Emit(OpCodes.Ldarg_1);
            }

            body.Emit(OpCodes.Ret);
        }

        _ = extensions.CreateType();
    }

    /// <summary>Adds overloads that do not match the generic sender and string name API.</summary>
    /// <param name="extensions">The extension class being built.</param>
    private static void DefineUnrelatedOverloads(TypeBuilder extensions)
    {
        var ordinary = extensions.DefineMethod(ChangedMethod, MethodAttributes.Public | MethodAttributes.Static, typeof(void), [typeof(object), typeof(string)]);
        ordinary.GetILGenerator().Emit(OpCodes.Ret);
        var missingName = extensions.DefineMethod(ChangedMethod, MethodAttributes.Public | MethodAttributes.Static);
        var missingSender = missingName.DefineGenericParameters(SenderParameter)[0];
        missingName.SetReturnType(typeof(void));
        missingName.SetParameters(missingSender);
        missingName.GetILGenerator().Emit(OpCodes.Ret);
        var numericName = extensions.DefineMethod(ChangedMethod, MethodAttributes.Public | MethodAttributes.Static);
        var numericSender = numericName.DefineGenericParameters(SenderParameter)[0];
        numericName.SetReturnType(typeof(void));
        numericName.SetParameters(numericSender, typeof(int));
        numericName.GetILGenerator().Emit(OpCodes.Ret);
    }

    /// <summary>Declares ordinary model notification methods for the extension-free layout.</summary>
    /// <param name="model">The model being built.</param>
    private static void DefineRaiseMethods(TypeBuilder model)
    {
        foreach (var changing in new[] { false, true })
        {
            var method = model.DefineMethod(changing ? "RaisePropertyChanging" : ChangedMethod, MethodAttributes.Public, typeof(void), [typeof(string)]);
            var body = method.GetILGenerator();
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(changing ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            body.Emit(OpCodes.Call, typeof(ToPropertyUnsafeSplitAssemblyTests).GetMethod(nameof(Record))!);
            body.Emit(OpCodes.Ret);
        }
    }

    /// <summary>Creates a unique assembly so fixtures do not share interface identities or cached raisers.</summary>
    /// <returns>The assembly's module.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ModuleBuilder CreateModule() =>
        AssemblyBuilder.DefineDynamicAssembly(new($"SplitReactiveUI_{Guid.NewGuid():N}"), AssemblyBuilderAccess.Run).DefineDynamicModule("Main");

    /// <summary>Reads each independent model's notification record.</summary>
    /// <param name="model">The notification owner.</param>
    /// <returns>The notifications recorded for that model.</returns>
    private static List<string> ReadNotifications(object model) => model is SplitModel shared
        ? shared.Raised
        : (List<string>)model.GetType().GetField(nameof(SplitModel.Raised))!.GetValue(model)!;

    /// <summary>The property and observable helper shared by the independent framework models.</summary>
    public class SplitModel
    {
        /// <summary>Gets or sets the observable helper.</summary>
        public ObservableAsPropertyHelper<string>? Helper { get; set; }

        /// <summary>Gets the observable value.</summary>
        public string? Name => Helper?.Value;

        /// <summary>Gets the notifications, in dispatch order.</summary>
        public List<string> Raised { get; } = [];
    }
}
