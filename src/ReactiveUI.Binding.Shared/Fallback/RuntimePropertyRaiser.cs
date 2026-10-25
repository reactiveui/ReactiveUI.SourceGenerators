// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>Raises one property's change notifications on one object, found by reflection.</summary>
/// <remarks>
/// The runtime twin of the generator's raise plugins, for <c>ToPropertyUnsafe</c>. It looks for members in the order
/// the generator does: ReactiveUI's raise extensions for an <c>IReactiveObject</c>, then a raise method, preferring one
/// that takes event args, then the field behind a field-like event. A type can only raise its own events, so the raise
/// method may be protected or private and the event field is always private; reflection reaches them where generated
/// code would add an accessor to a partial type.
/// <para>
/// The members found for a type are cached. A raise method is bound to its owner as a delegate once, when the raiser is
/// created, and each raiser caches its event args, so raising a value neither looks anything up nor allocates.
/// </para>
/// </remarks>
internal sealed class RuntimePropertyRaiser
{
    /// <summary>Why the raiser needs its members preserved.</summary>
    internal const string RequiresUnreferencedCodeMessage =
        "Finds the members that raise a property's change notifications by reflection; they may be trimmed.";

    /// <summary>Why the raiser needs dynamic code.</summary>
    internal const string RequiresDynamicCodeMessage =
        "Closes ReactiveUI's generic raise extension over IReactiveObject at run time.";

    /// <summary>The parameter count of ReactiveUI's raise extensions: the object and the property name.</summary>
    private const int ExtensionParameterCount = 2;

    /// <summary>The members found for each type, resolved once.</summary>
    private static readonly ConcurrentDictionary<Type, Members> MembersByType = new();

    /// <summary>The object whose notifications are raised.</summary>
    private readonly object _owner;

    /// <summary>The property name.</summary>
    private readonly string _propertyName;

    /// <summary>How the after-change notification is raised.</summary>
    private readonly RaiseMember _changed;

    /// <summary>The after-change raise method bound to the owner, or null when the member needs no binding.</summary>
    private readonly Delegate? _changedBound;

    /// <summary>How the before-change notification is raised, or null when the type raises none.</summary>
    private readonly RaiseMember? _changing;

    /// <summary>The before-change raise method bound to the owner, or null when the member needs no binding.</summary>
    private readonly Delegate? _changingBound;

    /// <summary>The after-change event args, created on the first raise.</summary>
    private PropertyChangedEventArgs? _changedArgs;

    /// <summary>The before-change event args, created on the first raise.</summary>
    private PropertyChangingEventArgs? _changingArgs;

    /// <summary>Initializes a new instance of the <see cref="RuntimePropertyRaiser"/> class.</summary>
    /// <param name="owner">The object whose notifications are raised.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="changed">How the after-change notification is raised.</param>
    /// <param name="changing">How the before-change notification is raised, or null for none.</param>
    private RuntimePropertyRaiser(object owner, string propertyName, RaiseMember changed, RaiseMember? changing)
    {
        _owner = owner;
        _propertyName = propertyName;
        _changed = changed;
        _changedBound = changed.Bind(owner);
        _changing = changing;
        _changingBound = changing?.Bind(owner);
    }

    /// <summary>Binds a raise member to one owner.</summary>
    /// <param name="owner">The object whose notification is raised.</param>
    /// <returns>The bound delegate, or null when the member needs no binding.</returns>
    private delegate Delegate? RaiseBinder(object owner);

    /// <summary>Raises one notification through a raise member.</summary>
    /// <param name="bound">The delegate the binder returned for the owner.</param>
    /// <param name="owner">The object whose notification is raised.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="args">The event args, which carry the property name.</param>
    private delegate void RaiseInvoker(Delegate? bound, object owner, string propertyName, EventArgs args);

    /// <summary>Gets a value indicating whether the owner's type raises a before-change notification.</summary>
    internal bool RaisesChanging => _changing is not null;

    /// <summary>Creates a raiser for one property of an object.</summary>
    /// <param name="owner">The object whose notifications are raised.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The raiser.</returns>
    /// <exception cref="InvalidOperationException">The object's type raises no after-change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    internal static RuntimePropertyRaiser Create(object owner, string propertyName)
    {
        var type = owner.GetType();
        var members = MembersByType.GetOrAdd(type, FindMembers);
        return members.Changed is null
            ? throw new InvalidOperationException(
                $"{type.FullName} raises no change notification ToPropertyUnsafe can reach. Implement ReactiveUI's IReactiveObject, "
                + "declare a RaisePropertyChanged or OnPropertyChanged method, or declare a field-like PropertyChanged event.")
            : new RuntimePropertyRaiser(owner, propertyName, members.Changed, members.Changing);
    }

    /// <summary>Raises the after-change notification.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void RaiseChanged() =>
        _changed.Invoke(_changedBound, _owner, _propertyName, _changedArgs ??= new PropertyChangedEventArgs(_propertyName));

    /// <summary>Raises the before-change notification, when the owner's type has one.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void RaiseChanging() =>
        _changing?.Invoke(_changingBound, _owner, _propertyName, _changingArgs ??= new PropertyChangingEventArgs(_propertyName));

    /// <summary>Finds the members that raise both notifications on a type.</summary>
    /// <param name="type">The type.</param>
    /// <returns>The members.</returns>
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    private static Members FindMembers(Type type) =>
        new(FindMember(type, false), FindMember(type, true));

    /// <summary>Finds the member that raises one notification on a type, strongest mechanism first.</summary>
    /// <param name="type">The type.</param>
    /// <param name="changing">Whether to find the before-change notification rather than the after-change one.</param>
    /// <returns>The member, or null when the type raises no such notification.</returns>
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    private static RaiseMember? FindMember(Type type, bool changing) =>
        FindReactiveObjectExtension(type, changing) ?? FindRaiseMethod(type, changing) ?? FindEventField(type, changing);

    /// <summary>Finds ReactiveUI's public raise extension, which honours suppressed and delayed notifications.</summary>
    /// <param name="type">The type.</param>
    /// <param name="changing">Whether to find the before-change extension.</param>
    /// <returns>The extension closed over <c>IReactiveObject</c>, or null when the type is not an <c>IReactiveObject</c>.</returns>
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    private static RaiseMember? FindReactiveObjectExtension(Type type, bool changing)
    {
        var reactiveObject = type.GetInterface("ReactiveUI.IReactiveObject");
        if (reactiveObject is null)
        {
            return null;
        }

        var extensions = FindReactiveObjectExtensions(type, reactiveObject);
        if (extensions is null)
        {
            return null;
        }

        var name = changing ? "RaisePropertyChanging" : "RaisePropertyChanged";
        foreach (var method in extensions.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (method.Name != name || !method.IsGenericMethodDefinition
                || method.GetParameters() is not { Length: ExtensionParameterCount } parameters
                || parameters[1].ParameterType != typeof(string))
            {
                continue;
            }

            // A static method closed over its first argument binds to Action<string> like an instance method.
            return ByName(method.MakeGenericMethod(reactiveObject));
        }

        return null;
    }

    /// <summary>Finds the extension class that owns the object's ReactiveUI notification state.</summary>
    /// <param name="type">The object's type.</param>
    /// <param name="reactiveObject">The ReactiveUI interface it implements.</param>
    /// <returns>The extension class, or null when the object's assemblies declare neither flavour.</returns>
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    private static Type? FindReactiveObjectExtensions(Type type, Type reactiveObject)
    {
        // The interface can live in ReactiveUI.Core; the base class identifies the flavour holding the state.
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (FindReactiveObjectExtensions(current.Assembly) is { } extensions)
            {
                return extensions;
            }
        }

        return FindReactiveObjectExtensions(reactiveObject.Assembly);
    }

    /// <summary>Finds either ReactiveUI flavour's raise extensions in an assembly.</summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>The extension class, or null when the assembly declares neither flavour.</returns>
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    private static Type? FindReactiveObjectExtensions(Assembly assembly) =>
        assembly.GetType("ReactiveUI.IReactiveObjectExtensions")
        ?? assembly.GetType("ReactiveUI.Reactive.IReactiveObjectExtensions");

    /// <summary>Finds a raise method, preferring an overload that takes event args over one that takes a name.</summary>
    /// <param name="type">The type.</param>
    /// <param name="changing">Whether to find a before-change raise method.</param>
    /// <returns>The raise method, or null when the type has none.</returns>
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    [SuppressMessage("Security", "SES1406", Justification = "reflection-based API that must reach a non-public member; there is no public route")]
    private static RaiseMember? FindRaiseMethod(Type type, bool changing)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var names = changing ? RaiseMethodNames.Changing : RaiseMethodNames.Changed;
        var argsType = changing ? typeof(PropertyChangingEventArgs) : typeof(PropertyChangedEventArgs);
        for (var i = 0; i < names.Length; i++)
        {
            if (type.GetMethod(names[i], flags, null, [argsType], null) is { } byArgs)
            {
                return ByArgs(byArgs, changing);
            }

            if (type.GetMethod(names[i], flags, null, [typeof(string)], null) is { } byName)
            {
                return ByName(byName);
            }
        }

        return null;
    }

    /// <summary>Finds the field behind a field-like event, walking up to the type that declares it.</summary>
    /// <param name="type">The type.</param>
    /// <param name="changing">Whether to find the <c>PropertyChanging</c> event rather than <c>PropertyChanged</c>.</param>
    /// <returns>The field, or null when no type in the hierarchy declares the event as a field.</returns>
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    [SuppressMessage("Security", "SES1406", Justification = "reflection-based API that must reach a non-public member; there is no public route")]
    private static RaiseMember? FindEventField(Type type, bool changing)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        var name = changing ? nameof(INotifyPropertyChanging.PropertyChanging) : nameof(INotifyPropertyChanged.PropertyChanged);
        var handlerType = changing ? typeof(PropertyChangingEventHandler) : typeof(PropertyChangedEventHandler);
        for (var declaring = type; declaring is not null; declaring = declaring.BaseType)
        {
            if (declaring.GetField(name, flags) is { } field && field.FieldType == handlerType)
            {
                return new(static _ => null, (_, owner, _, args) => RaiseThroughEventField(field.GetValue(owner), owner, args));
            }
        }

        return null;
    }

    /// <summary>Describes a raise method that takes event args, bound to its owner when it returns nothing.</summary>
    /// <param name="method">The raise method.</param>
    /// <param name="changing">Whether the method takes <see cref="PropertyChangingEventArgs"/>.</param>
    /// <returns>The raise member.</returns>
    private static RaiseMember ByArgs(MethodInfo method, bool changing)
    {
        if (method.ReturnType != typeof(void))
        {
            return new(static _ => null, (_, owner, _, args) => method.Invoke(owner, [args]));
        }

        return changing
            ? new(
                owner => BindTo<Action<PropertyChangingEventArgs>>(method, owner),
                static (bound, _, _, args) => ((Action<PropertyChangingEventArgs>)bound!)((PropertyChangingEventArgs)args))
            : new(
                owner => BindTo<Action<PropertyChangedEventArgs>>(method, owner),
                static (bound, _, _, args) => ((Action<PropertyChangedEventArgs>)bound!)((PropertyChangedEventArgs)args));
    }

    /// <summary>Describes a raise member that takes the property name, bound to its owner when it returns nothing.</summary>
    /// <param name="method">An instance raise method, or a static one whose first parameter takes the owner.</param>
    /// <returns>The raise member.</returns>
    private static RaiseMember ByName(MethodInfo method)
    {
        if (method.ReturnType != typeof(void))
        {
            return method.IsStatic
                ? new(static _ => null, (_, owner, propertyName, _) => method.Invoke(null, [owner, propertyName]))
                : new(static _ => null, (_, owner, propertyName, _) => method.Invoke(owner, [propertyName]));
        }

        return new(
            owner => BindTo<Action<string>>(method, owner),
            static (bound, _, propertyName, _) => ((Action<string>)bound!)(propertyName));
    }

    /// <summary>Binds a method to an owner as a delegate: an instance method's target, or a static method's first argument.</summary>
    /// <typeparam name="TDelegate">The delegate type.</typeparam>
    /// <param name="method">The method.</param>
    /// <param name="owner">The object the delegate is closed over.</param>
    /// <returns>The bound delegate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TDelegate BindTo<TDelegate>(MethodInfo method, object owner)
        where TDelegate : Delegate =>
#if NET5_0_OR_GREATER
        method.CreateDelegate<TDelegate>(owner);
#else
        (TDelegate)method.CreateDelegate(typeof(TDelegate), owner);
#endif

    /// <summary>Invokes the delegate held by a field-like event, typed so no dynamic invoke is needed.</summary>
    /// <param name="handler">The field's current value, or null when nothing is subscribed.</param>
    /// <param name="owner">The object whose notification is raised.</param>
    /// <param name="args">The event args.</param>
    private static void RaiseThroughEventField(object? handler, object owner, EventArgs args)
    {
        if (handler is PropertyChangedEventHandler changed)
        {
            changed(owner, (PropertyChangedEventArgs)args);
        }
        else if (handler is PropertyChangingEventHandler changing)
        {
            changing(owner, (PropertyChangingEventArgs)args);
        }
    }

    /// <summary>The members that raise a type's two notifications.</summary>
    /// <param name="Changed">The after-change member, or null when the type has none.</param>
    /// <param name="Changing">The before-change member, or null when the type has none.</param>
    private readonly record struct Members(RaiseMember? Changed, RaiseMember? Changing);

    /// <summary>The raise method names, in the order the generator tries them.</summary>
    private static class RaiseMethodNames
    {
        /// <summary>The after-change raise method names.</summary>
        internal static readonly string[] Changed = ["RaisePropertyChanged", "OnPropertyChanged", "NotifyPropertyChanged", "NotifyOfPropertyChange"];

        /// <summary>The before-change raise method names.</summary>
        internal static readonly string[] Changing = ["RaisePropertyChanging", "OnPropertyChanging", "NotifyPropertyChanging"];
    }

    /// <summary>One member that raises a notification: how to bind it to an owner, and how to call it.</summary>
    /// <param name="Bind">Binds the member to an owner, once per raiser.</param>
    /// <param name="Invoke">Raises the notification, once per value.</param>
    private sealed record RaiseMember(RaiseBinder Bind, RaiseInvoker Invoke);
}
