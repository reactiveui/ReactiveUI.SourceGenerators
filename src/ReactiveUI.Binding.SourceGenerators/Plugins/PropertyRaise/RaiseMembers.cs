// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

/// <summary>Finds the members a type raises its change notifications through.</summary>
internal static class RaiseMembers
{
    /// <summary>The name of the after-change event.</summary>
    internal const string ChangedEventName = "PropertyChanged";

    /// <summary>The name of the before-change event.</summary>
    internal const string ChangingEventName = "PropertyChanging";

    /// <summary>The conventional names of a method that raises <c>PropertyChanged</c>.</summary>
    /// <remarks>
    /// Covers hand-written view models and the common base classes: CommunityToolkit.Mvvm and Prism use
    /// <c>OnPropertyChanged</c> and <c>RaisePropertyChanged</c>, Caliburn.Micro uses <c>NotifyOfPropertyChange</c>.
    /// </remarks>
    private static readonly string[] ChangedMethodNames =
        ["RaisePropertyChanged", "OnPropertyChanged", "NotifyPropertyChanged", "NotifyOfPropertyChange"];

    /// <summary>The conventional names of a method that raises <c>PropertyChanging</c>.</summary>
    private static readonly string[] ChangingMethodNames =
        ["RaisePropertyChanging", "OnPropertyChanging", "NotifyPropertyChanging"];

    /// <summary>Finds a raise method on the type or a base type that is callable from a given place.</summary>
    /// <param name="type">The type that declares the property.</param>
    /// <param name="changing">Whether to look for a before-change raise method rather than an after-change one.</param>
    /// <param name="eventArgsType">The event-args type a raise method may take instead of a name.</param>
    /// <param name="within">Where the call is made from: the consumer assembly, or the type itself.</param>
    /// <param name="compilation">The consumer compilation, which judges accessibility.</param>
    /// <returns>The call, preferring an event-args overload; or null when there is none.</returns>
    /// <remarks>
    /// An overload that takes event args lets generated code pass one cached instance per property, so raising
    /// allocates nothing; a name overload allocates event args on every change. Both are looked for across the
    /// whole hierarchy before a name overload is settled for.
    /// </remarks>
    internal static PropertyRaiseCall? FindMethod(
        INamedTypeSymbol type,
        bool changing,
        INamedTypeSymbol? eventArgsType,
        ISymbol within,
        Compilation compilation)
    {
        var search = new MethodSearch(type, eventArgsType, within, compilation);
        var names = changing ? ChangingMethodNames : ChangedMethodNames;
        PropertyRaiseCall? byName = null;

        for (var current = type; current is not null; current = current.BaseType)
        {
            for (var n = 0; n < names.Length; n++)
            {
                if (search.Match(current, names[n], ref byName) is { } byArgs)
                {
                    return byArgs;
                }
            }
        }

        return byName;
    }

    /// <summary>Finds a field-like event the type itself declares, which only code inside the type can invoke.</summary>
    /// <param name="type">The type that declares the property.</param>
    /// <param name="name">The event name.</param>
    /// <param name="handlerType">The delegate type the event must have.</param>
    /// <returns>The invocation, or null when the type declares no such event.</returns>
    /// <remarks>
    /// A field-like event's accessors are synthesized by the compiler, which is how it is told apart from an event
    /// with hand-written accessors: only the former has a delegate field behind it that can be invoked.
    /// </remarks>
    internal static PropertyRaiseCall? FindFieldLikeEvent(INamedTypeSymbol type, string name, INamedTypeSymbol? handlerType)
    {
        if (handlerType is null)
        {
            return null;
        }

        var members = type.GetMembers(name);
        for (var i = 0; i < members.Length; i++)
        {
            if (members[i] is IEventSymbol { IsStatic: false, IsAbstract: false, AddMethod.IsImplicitlyDeclared: true } evt
                && SymbolEqualityComparer.Default.Equals(evt.Type, handlerType))
            {
                return new(PropertyRaiseCallKind.EventInvoke, name, PropertyRaiseArgumentKind.EventArgs);
            }
        }

        return null;
    }

    /// <summary>What one raise-method search looks for, and from where.</summary>
    /// <param name="Type">The type that declares the property, which a protected call is made through.</param>
    /// <param name="EventArgsType">The event-args type a raise method may take instead of a name.</param>
    /// <param name="Within">Where the call is made from.</param>
    /// <param name="Compilation">The consumer compilation, which judges accessibility.</param>
    private readonly record struct MethodSearch(
        INamedTypeSymbol Type,
        INamedTypeSymbol? EventArgsType,
        ISymbol Within,
        Compilation Compilation)
    {
        /// <summary>Looks at one type's methods of one name.</summary>
        /// <param name="declaring">The type whose members are read.</param>
        /// <param name="name">The method name.</param>
        /// <param name="byName">The first name overload found so far, kept in case no event-args overload exists.</param>
        /// <returns>An event-args overload, which ends the search; otherwise null.</returns>
        public PropertyRaiseCall? Match(INamedTypeSymbol declaring, string name, ref PropertyRaiseCall? byName)
        {
            var members = declaring.GetMembers(name);
            for (var m = 0; m < members.Length; m++)
            {
                if (AsRaiseMethod(members[m]) is not { } method || !Compilation.IsSymbolAccessibleWithin(method, Within, Type))
                {
                    continue;
                }

                var first = method.Parameters[0].Type;
                if (EventArgsType is not null && SymbolEqualityComparer.Default.Equals(first, EventArgsType))
                {
                    return new(PropertyRaiseCallKind.InstanceMethod, name, PropertyRaiseArgumentKind.EventArgs);
                }

                if (byName is null && first.SpecialType == SpecialType.System_String)
                {
                    byName = new(PropertyRaiseCallKind.InstanceMethod, name, PropertyRaiseArgumentKind.PropertyName);
                }
            }

            return null;
        }

        /// <summary>Determines whether a member is an instance method callable with one name or event-args argument.</summary>
        /// <param name="member">The member to judge.</param>
        /// <returns>The member as a method when one argument is enough to call it; otherwise null.</returns>
        private static IMethodSymbol? AsRaiseMethod(ISymbol member)
        {
            if (member is not IMethodSymbol { IsStatic: false, IsGenericMethod: false, MethodKind: MethodKind.Ordinary } method
                || method.Parameters.IsEmpty
                || method.Parameters[0].RefKind != RefKind.None)
            {
                return null;
            }

            for (var i = 1; i < method.Parameters.Length; i++)
            {
                if (!method.Parameters[i].IsOptional)
                {
                    return null;
                }
            }

            return method;
        }
    }
}
