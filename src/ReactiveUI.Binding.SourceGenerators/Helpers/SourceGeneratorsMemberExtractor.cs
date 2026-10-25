// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Describes the members ReactiveUI.SourceGenerators adds to a type, following its naming and typing rules.</summary>
/// <remarks>
/// A generator never sees another generator's output, so a call site that binds a property ReactiveUI.SourceGenerators
/// writes has nothing to bind against. These rules repeat what that generator writes, so the call site can be read as
/// if its output were already there. They follow ReactiveUI.SourceGenerators 4.0.0.
/// </remarks>
internal static class SourceGeneratorsMemberExtractor
{
    /// <summary>The prefix a Hungarian-style member name starts with.</summary>
    private const string MemberPrefix = "m_";

    /// <summary>The suffix an asynchronous command method's name drops.</summary>
    private const string AsyncSuffix = "Async";

    /// <summary>The suffix a generated command property's name ends with.</summary>
    private const string CommandSuffix = "Command";

    /// <summary>The name of the attribute argument that sets the generated setter's accessibility.</summary>
    private const string SetModifierArgument = "SetModifier";

    /// <summary>The name of the attribute argument that sets the generated property's accessibility.</summary>
    private const string AccessModifierArgument = "AccessModifier";

    /// <summary>The metadata name of the command factory in ReactiveUI's System.Reactive flavour.</summary>
    private const string ReactiveCommandFactoryName = "ReactiveUI.Reactive.ReactiveCommand";

    /// <summary>The metadata name of ReactiveUI's command factory.</summary>
    private const string CommandFactoryName = "ReactiveUI.ReactiveCommand";

    /// <summary>The name of the void type a ReactiveUI.Primitives command carries.</summary>
    private const string RxVoidName = "ReactiveUI.Primitives.RxVoid";

    /// <summary>The void type a ReactiveUI.Primitives command carries, fully qualified.</summary>
    private const string RxVoidType = "global::ReactiveUI.Primitives.RxVoid";

    /// <summary>The accessibility of a generated property that takes no access modifier.</summary>
    private const string PublicAccessibility = "public";

    /// <summary>The void type a System.Reactive command carries.</summary>
    private const string UnitType = "global::System.Reactive.Unit";

    /// <summary>The metadata name of <c>CancellationToken</c>, which a command method may take last.</summary>
    private const string CancellationTokenName = "System.Threading.CancellationToken";

    /// <summary>The value of the <c>Protected</c> access modifier.</summary>
    private const int ProtectedModifier = 1;

    /// <summary>The value of the <c>Internal</c> access modifier.</summary>
    private const int InternalModifier = 2;

    /// <summary>The value of the <c>Private</c> access modifier.</summary>
    private const int PrivateModifier = 3;

    /// <summary>The value of the <c>InternalProtected</c> access modifier.</summary>
    private const int ProtectedInternalModifier = 4;

    /// <summary>The value of the <c>PrivateProtected</c> access modifier.</summary>
    private const int PrivateProtectedModifier = 5;

    /// <summary>The value of the <c>Init</c> setter modifier.</summary>
    private const int InitModifier = 6;

    /// <summary>The accessor body every generated accessor declaration carries. It never runs.</summary>
    private const string GetAccessor = "get { throw null; }";

    /// <summary>Describes the property <c>[Reactive]</c> generates from a field.</summary>
    /// <param name="context">The attribute context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The property, or null when the attribute is misplaced on an event field.</returns>
    /// <remarks>Only fields reach this: the attribute on a partial property marks a property the consumer declares.</remarks>
    internal static SourceGeneratorsMember? ExtractReactive(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (context.TargetSymbol is not IFieldSymbol field)
        {
            return null;
        }

        var setter = ReadInt(context.Attributes[0], SetModifierArgument) switch
        {
            ProtectedModifier => "protected set { }",
            InternalModifier => "internal set { }",
            PrivateModifier => "private set { }",
            ProtectedInternalModifier => "protected internal set { }",
            PrivateProtectedModifier => "private protected set { }",
            InitModifier => "init { }",
            _ => "set { }",
        };

        return FromField(field, PublicAccessibility, setter);
    }

    /// <summary>Describes the property <c>[ReactiveCollection]</c> generates from a field.</summary>
    /// <param name="context">The attribute context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The property, or null when the attribute is misplaced on an event field.</returns>
    internal static SourceGeneratorsMember? ExtractReactiveCollection(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return context.TargetSymbol is IFieldSymbol field ? FromField(field, PublicAccessibility, "set { }") : null;
    }

    /// <summary>Describes the read-only property <c>[BindableDerivedList]</c> generates from a field.</summary>
    /// <param name="context">The attribute context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The property.</returns>
    internal static SourceGeneratorsMember? ExtractBindableDerivedList(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return FromField((IFieldSymbol)context.TargetSymbol, ReadAccessibility(context.Attributes[0]), null);
    }

    /// <summary>Describes the command property <c>[ReactiveCommand]</c> generates from a method.</summary>
    /// <param name="context">The attribute context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The property, or null when the method takes more than one parameter the command could carry.</returns>
    internal static SourceGeneratorsMember? ExtractReactiveCommand(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var method = (IMethodSymbol)context.TargetSymbol;
        if (PartialTypeRaisePlugin.DescribeDeclaration(method.ContainingType) is not { } declaration)
        {
            return null;
        }

        var returnType = method.ReturnType;
        var isTask = IsTask(returnType);
        var isObservable = IsObservable(returnType);
        var output = isTask || isObservable ? Unwrap(returnType) : returnType;

        if (CommandInput(method, isTask, out var input) is false)
        {
            return null;
        }

        var (commandNamespace, voidType) = ReadCommandApi(context.SemanticModel.Compilation);
        var outputName = output is null or { SpecialType: SpecialType.System_Void } ? voidType : TypeName(output);
        var inputName = input is null ? voidType : TypeName(input);

        var property = new PooledStringBuilder()
            .Append(ReadAccessibility(context.Attributes[0]))
            .Append(' ')
            .Append(commandNamespace)
            .Append(".ReactiveCommand<")
            .Append(inputName)
            .Append(", ")
            .Append(outputName)
            .Append("> ")
            .Append(CommandName(method.Name, isTask))
            .Append(" { ")
            .Append(GetAccessor)
            .Append(" }")
            .ToStringAndReturn();

        return new(declaration, null, property);
    }

    /// <summary>Describes the <c>IReactiveObject</c> implementation <c>[IReactiveObject]</c> adds to a class.</summary>
    /// <param name="context">The attribute context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The interface, or null when the compilation has no <c>IReactiveObject</c> to implement.</returns>
    internal static SourceGeneratorsMember? ExtractIReactiveObject(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var reactiveObject = context.SemanticModel.Compilation.GetTypeByMetadataName(Constants.IReactiveObjectMetadataName);
        return reactiveObject is not null
            && PartialTypeRaisePlugin.DescribeDeclaration((INamedTypeSymbol)context.TargetSymbol) is { } declaration
            ? new(declaration, TypeName(reactiveObject), null)
            : null;
    }

    /// <summary>Gets the name ReactiveUI.SourceGenerators gives the property it generates from a field.</summary>
    /// <param name="fieldName">The field's name.</param>
    /// <returns>The name without an <c>m_</c> or leading-underscore prefix, its first letter upper case.</returns>
    internal static string PropertyName(string fieldName)
    {
        var start = PrefixLength(fieldName);
        return start >= fieldName.Length
            ? fieldName
            : char.ToUpperInvariant(fieldName[start]) + fieldName.Substring(start + 1);
    }

    /// <summary>Gets the name ReactiveUI.SourceGenerators gives the command property it generates from a method.</summary>
    /// <param name="methodName">The method's name.</param>
    /// <param name="isAsync">Whether the method returns a task, which drops an <c>Async</c> suffix.</param>
    /// <returns>The method name's stem, its first letter upper case, followed by <c>Command</c>.</returns>
    internal static string CommandName(string methodName, bool isAsync)
    {
        var start = PrefixLength(methodName);
        var length = methodName.Length - start;
        if (isAsync && length >= AsyncSuffix.Length && methodName.EndsWith(AsyncSuffix, StringComparison.Ordinal))
        {
            length -= AsyncSuffix.Length;
        }

        return length == 0
            ? CommandSuffix
            : char.ToUpperInvariant(methodName[start]) + methodName.Substring(start + 1, length - 1) + CommandSuffix;
    }

    /// <summary>Describes the property generated from a field.</summary>
    /// <param name="field">The field.</param>
    /// <param name="accessibility">The property's accessibility.</param>
    /// <param name="setter">The setter declaration, or null for a read-only property.</param>
    /// <returns>The property, or null when its name would repeat the field's or its type is not partial.</returns>
    private static SourceGeneratorsMember? FromField(IFieldSymbol field, string accessibility, string? setter)
    {
        var name = PropertyName(field.Name);
        if (string.Equals(name, field.Name, StringComparison.Ordinal)
            || PartialTypeRaisePlugin.DescribeDeclaration(field.ContainingType) is not { } declaration)
        {
            return null;
        }

        var property = new PooledStringBuilder()
            .Append(accessibility)
            .Append(' ')
            .Append(TypeName(field.Type))
            .Append(' ')
            .Append(name)
            .Append(" { ")
            .Append(GetAccessor);
        if (setter is not null)
        {
            _ = property.Append(' ').Append(setter);
        }

        return new(declaration, null, property.Append(" }").ToStringAndReturn());
    }

    /// <summary>Finds the parameter a command carries, the way ReactiveUI.SourceGenerators does.</summary>
    /// <param name="method">The command method.</param>
    /// <param name="isTask">Whether the method returns a task.</param>
    /// <param name="input">The parameter's type, or null when the command takes none.</param>
    /// <returns><see langword="false"/> when the method has more than one parameter to carry, and no command is generated.</returns>
    /// <remarks>
    /// A task method may take a <c>CancellationToken</c> after its one parameter. Any other method that takes a
    /// <c>CancellationToken</c> carries no parameter.
    /// </remarks>
    private static bool CommandInput(IMethodSymbol method, bool isTask, out ITypeSymbol? input)
    {
        input = null;
        var parameters = method.Parameters;
        if (TakesCancellationToken(parameters))
        {
            if (isTask && parameters.Length == 2)
            {
                input = parameters[0].Type;
            }

            return true;
        }

        if (parameters.Length > 1)
        {
            return false;
        }

        input = parameters.IsEmpty ? null : parameters[0].Type;
        return true;
    }

    /// <summary>Determines whether any of a method's parameters is a <c>CancellationToken</c>.</summary>
    /// <param name="parameters">The method's parameters.</param>
    /// <returns><see langword="true"/> when one of them is.</returns>
    private static bool TakesCancellationToken(ImmutableArray<IParameterSymbol> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.Type.ToDisplayString() == CancellationTokenName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reads which ReactiveUI declares the command type, and the type a command without a value carries.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The command type's namespace and the void type, both fully qualified.</returns>
    private static (string Namespace, string VoidType) ReadCommandApi(Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName(ReactiveCommandFactoryName) is not null)
        {
            return ("global::ReactiveUI.Reactive", UnitType);
        }

        var command = compilation.GetTypeByMetadataName(CommandFactoryName);
        return ("global::ReactiveUI", command is not null && CreatesRxVoid(command) ? RxVoidType : UnitType);
    }

    /// <summary>Determines whether ReactiveUI's command factory creates commands over <c>RxVoid</c>.</summary>
    /// <param name="command">The <c>ReactiveCommand</c> factory class.</param>
    /// <returns><see langword="true"/> when a <c>Create</c> overload returns a command typed with <c>RxVoid</c>.</returns>
    private static bool CreatesRxVoid(INamedTypeSymbol command)
    {
        foreach (var member in command.GetMembers("Create"))
        {
            if (member is not IMethodSymbol { ReturnType: INamedTypeSymbol returnType })
            {
                continue;
            }

            foreach (var argument in returnType.TypeArguments)
            {
                if (argument.ToDisplayString() == RxVoidName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Determines whether a type is, or derives from, <c>System.Threading.Tasks.Task</c>.</summary>
    /// <param name="type">The method's return type.</param>
    /// <returns><see langword="true"/> for <c>Task</c>, <c>Task&lt;T&gt;</c> and their subclasses.</returns>
    private static bool IsTask(ITypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current is INamedTypeSymbol { Name: "Task", Arity: 0, ContainingType: null } task
                && task.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type, or a type it derives from, mentions <c>System.IObservable</c>.</summary>
    /// <param name="type">The method's return type.</param>
    /// <returns><see langword="true"/> when the type, a containing type or a type argument is an <c>IObservable</c>.</returns>
    private static bool IsObservable(ITypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (MentionsObservable(current))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type, its containing type or any type argument is <c>System.IObservable</c>.</summary>
    /// <param name="type">The type.</param>
    /// <returns><see langword="true"/> when an <c>IObservable</c> appears anywhere in the type's name.</returns>
    private static bool MentionsObservable(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array)
        {
            return MentionsObservable(array.ElementType);
        }

        if (type is not INamedTypeSymbol named)
        {
            return false;
        }

        if ((named.Name.StartsWith("IObservable", StringComparison.Ordinal) && named.ContainingNamespace.ToDisplayString() == "System")
            || (named.ContainingType is { } containing && MentionsObservable(containing)))
        {
            return true;
        }

        foreach (var argument in named.TypeArguments)
        {
            if (MentionsObservable(argument))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reads the generated property's accessibility from a <c>PropertyAccessModifier</c> argument.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <returns>The accessibility keywords.</returns>
    private static string ReadAccessibility(AttributeData attribute) =>
        ReadInt(attribute, AccessModifierArgument) switch
        {
            ProtectedModifier => "protected",
            InternalModifier => "internal",
            PrivateModifier => "private",
            ProtectedInternalModifier => "protected internal",
            PrivateProtectedModifier => "private protected",
            _ => PublicAccessibility,
        };

    /// <summary>Reads an enum-valued named argument of an attribute.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <param name="name">The argument's name.</param>
    /// <returns>The argument's value, or zero when it is not set.</returns>
    private static int ReadInt(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (argument.Key == name && argument.Value.Value is int value)
            {
                return value;
            }
        }

        return 0;
    }

    /// <summary>Gets the length of the prefix a generated name drops: <c>m_</c>, or any leading underscores.</summary>
    /// <param name="name">The member name.</param>
    /// <returns>The number of characters to skip.</returns>
    private static int PrefixLength(string name)
    {
        if (name.StartsWith(MemberPrefix, StringComparison.Ordinal))
        {
            return MemberPrefix.Length;
        }

        var start = 0;
        while (start < name.Length && name[start] == '_')
        {
            start++;
        }

        return start;
    }

    /// <summary>Writes a type's fully qualified name.</summary>
    /// <param name="type">The type.</param>
    /// <returns>The name, starting with <c>global::</c> where the type has a namespace.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string TypeName(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    /// <summary>Gets the value a task or observable produces.</summary>
    /// <param name="type">The method's return type.</param>
    /// <returns>Its one type argument, or null when it has none, as a plain <c>Task</c> does.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ITypeSymbol? Unwrap(ITypeSymbol type) =>
        type is INamedTypeSymbol { TypeArguments.Length: 1 } wrapper ? wrapper.TypeArguments[0] : null;
}
