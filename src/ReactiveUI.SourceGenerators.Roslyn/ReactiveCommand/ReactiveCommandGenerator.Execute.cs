// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates properties that expose methods as ReactiveUI commands.</summary>
/// <seealso cref="IIncrementalGenerator" />
public partial class ReactiveCommandGenerator
{
    /// <summary>Gets the fully-qualified name emitted in generated-code attributes.</summary>
    internal static readonly string GeneratorName = typeof(ReactiveCommandGenerator).FullName!;

    /// <summary>Gets the generator assembly version emitted in generated-code attributes.</summary>
    internal static readonly string GeneratorVersion = typeof(ReactiveCommandGenerator).Assembly.GetName().Version.ToString();

    /// <summary>The ReactiveUI command type name.</summary>
    private const string ReactiveCommand = "ReactiveCommand";

    /// <summary>The factory method used for synchronous commands.</summary>
    private const string Create = ".Create";

    /// <summary>The factory method used for observable commands.</summary>
    private const string CreateO = ".CreateFromObservable";

    /// <summary>The factory method used for task-backed commands.</summary>
    private const string CreateT = ".CreateFromTask";

    /// <summary>The factory method used for synchronous commands that execute on a background scheduler.</summary>
    private const string CreateB = ".CreateRunInBackground";

    /// <summary>The attribute property used to specify the can-execute member.</summary>
    private const string CanExecute = "CanExecute";

    /// <summary>The attribute property used to request background execution.</summary>
    private const string RunInBackground = "RunInBackground";

    /// <summary>The method that starts a task-returning command on the thread pool.</summary>
    private const string TaskRun = "global::System.Threading.Tasks.Task.Run";

    /// <summary>The cancellation token type a task-returning command method may take last.</summary>
    private const string CancellationTokenType = "global::System.Threading.CancellationToken";

    /// <summary>The access-modifier value for internal generated commands.</summary>
    private const int InternalAccessibility = 2;

    /// <summary>The access-modifier value for private generated commands.</summary>
    private const int PrivateAccessibility = 3;

    /// <summary>The access-modifier value for protected-internal generated commands.</summary>
    private const int ProtectedInternalAccessibility = 4;

    /// <summary>The access-modifier value for private-protected generated commands.</summary>
    private const int PrivateProtectedAccessibility = 5;

    /// <summary>The length of the conventional <c>m_</c> field prefix.</summary>
    private const int CommandNamePrefixLength = 2;

    /// <summary>The suffix an asynchronous method's name drops in its command's name.</summary>
    private const string AsyncSuffix = "Async";

    /// <summary>The suffix every generated command property and field name ends with.</summary>
    private const string CommandSuffix = "Command";

    /// <summary>The <c>GeneratedCode</c> attribute stamped on every generated command, built once.</summary>
    private static readonly string GeneratedCodeAttribute = SourceWriterExtensions.GeneratedCodeAttribute(GeneratorName, GeneratorVersion);

    /// <summary>Gets the metadata needed to generate a command for an attributed method.</summary>
    /// <param name="context">The generator attribute context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The command metadata, or <see langword="null"/> when the method is unsupported.</returns>
    private static CommandInfo? GetMethodInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token) =>
        context.TargetSymbol is IMethodSymbol methodSymbol
            ? CreateCommandInfo(context, methodSymbol, context.Attributes[0], token)
            : default;

    /// <summary>Creates the metadata for a supported command method.</summary>
    /// <param name="context">The generator attribute context.</param>
    /// <param name="methodSymbol">The attributed method symbol.</param>
    /// <param name="attributeData">The command attribute.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The command metadata, or <see langword="null"/> when the method has unsupported parameters.</returns>
    private static CommandInfo? CreateCommandInfo(
        in GeneratorAttributeSyntaxContext context,
        IMethodSymbol methodSymbol,
        AttributeData attributeData,
        CancellationToken token)
    {
        var (isTask, isObservable, realReturnType, isReturnTypeVoid) = GetCommandReturnInfo(methodSymbol, context.SemanticModel.Compilation);
        var methodParameters = GetCommandParameters(methodSymbol, isTask);
        if (methodParameters.Length > 1)
        {
            return default;
        }

        token.ThrowIfCancellationRequested();
        TryGetCanExecuteExpressionType(methodSymbol, attributeData, out var canExecuteObservableName, out var canExecuteTypeInfo);
        token.ThrowIfCancellationRequested();
        var outputScheduler = GetScheduler(context, methodSymbol, attributeData, ReactiveCommandRules.OutputSchedulerArgument);
        token.ThrowIfCancellationRequested();
        var backgroundScheduler = GetScheduler(context, methodSymbol, attributeData, ReactiveCommandRules.BackgroundSchedulerArgument);
        var runInBackground = backgroundScheduler is not null || attributeData.GetNamedArgument<bool>(RunInBackground);
        token.ThrowIfCancellationRequested();
        var accessModifier = GetAccessModifier(attributeData);
        token.ThrowIfCancellationRequested();
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        context.GetForwardedAttributes(
            builder,
            methodSymbol,
            methodSyntax.AttributeLists,
            token,
            out var forwardedPropertyAttributes);
        token.ThrowIfCancellationRequested();
        var targetInfo = TargetInfo.From(methodSymbol.ContainingType);
        token.ThrowIfCancellationRequested();
        var argumentTypeString = methodParameters.IsEmpty
            ? null
            : methodParameters[0].Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        token.ThrowIfCancellationRequested();

        return new(
            targetInfo,
            methodSymbol.Name,
            realReturnType.GetFullyQualifiedNameWithNullabilityAnnotations(),
            argumentTypeString,
            isTask,
            isReturnTypeVoid,
            isObservable,
            canExecuteObservableName,
            canExecuteTypeInfo,
            outputScheduler,
            runInBackground,
            backgroundScheduler,
            HasTrailingCancellationToken(methodSymbol),
            forwardedPropertyAttributes,
            accessModifier,
            context.TargetNode.HasDocumentationComment() ? GetXmlDocumentation(methodSymbol, token) : string.Empty);
    }

    /// <summary>Gets the relevant return-type details for a command method.</summary>
    /// <param name="methodSymbol">The command method.</param>
    /// <param name="compilation">The active compilation.</param>
    /// <returns>The async shape, unwrapped return type, and void status.</returns>
    private static (bool IsTask, bool IsObservable, ITypeSymbol ReturnType, bool IsVoid) GetCommandReturnInfo(IMethodSymbol methodSymbol, Compilation compilation)
    {
        var isTask = methodSymbol.ReturnType.IsTaskReturnType();
        var isObservable = methodSymbol.ReturnType.IsObservableReturnType();
        var returnType = isTask || isObservable
            ? methodSymbol.ReturnType.GetTaskReturnType(compilation)
            : methodSymbol.ReturnType;
        var isVoid = SymbolEqualityComparer.Default.Equals(returnType, compilation.GetSpecialType(SpecialType.System_Void));
        return (isTask, isObservable, returnType, isVoid);
    }

    /// <summary>Gets the supported input parameters for a command method.</summary>
    /// <param name="methodSymbol">The command method.</param>
    /// <param name="isTask">Whether the method is task-backed.</param>
    /// <returns>The parameters exposed by the generated command.</returns>
    private static ImmutableArray<IParameterSymbol> GetCommandParameters(IMethodSymbol methodSymbol, bool isTask)
    {
        using var builder = ImmutableArrayBuilder<IParameterSymbol>.Rent();
        var hasCancellationToken = false;
        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Type.ToDisplayString() == "System.Threading.CancellationToken")
            {
                hasCancellationToken = true;
                break;
            }
        }

        if (isTask && hasCancellationToken && methodSymbol.Parameters.Length == 2)
        {
            builder.Add(methodSymbol.Parameters[0]);
        }
        else if (!hasCancellationToken)
        {
            foreach (var parameter in methodSymbol.Parameters)
            {
                builder.Add(parameter);
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>Determines whether a command method takes a <c>CancellationToken</c> as its last parameter.</summary>
    /// <param name="methodSymbol">The command method.</param>
    /// <returns><see langword="true"/> when the last parameter is a <c>CancellationToken</c>.</returns>
    private static bool HasTrailingCancellationToken(IMethodSymbol methodSymbol) =>
        !methodSymbol.Parameters.IsEmpty
        && methodSymbol.Parameters[methodSymbol.Parameters.Length - 1].Type.ToDisplayString() == "System.Threading.CancellationToken";

    /// <summary>Gets the generated command property's access modifier.</summary>
    /// <param name="attributeData">The command attribute.</param>
    /// <returns>The C# access-modifier text.</returns>
    private static string GetAccessModifier(AttributeData attributeData) =>
        attributeData.GetNamedArgument<int>("AccessModifier") switch
        {
            1 => "protected",
            InternalAccessibility => "internal",
            PrivateAccessibility => "private",
            ProtectedInternalAccessibility => "protected internal",
            PrivateProtectedAccessibility => "private protected",
            _ => "public",
        };

    /// <summary>Formats a method's XML documentation for insertion into generated source.</summary>
    /// <param name="methodSymbol">The documented method.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The formatted documentation, or an empty string.</returns>
    private static string GetXmlDocumentation(IMethodSymbol methodSymbol, CancellationToken token)
    {
        var xmlDocumentation = methodSymbol.GetDocumentationCommentXml(cancellationToken: token) ?? string.Empty;
        if (xmlDocumentation.Length == 0)
        {
            return string.Empty;
        }

        var lines = xmlDocumentation.Split('\n');
        if (lines.Length < 3)
        {
            return string.Empty;
        }

        // The first and the last two lines are the <member> envelope around the documentation.
        const int XmlMemberEnvelopeLineCount = 2;
        var builder = PooledBuilder.Rent(xmlDocumentation.Length);
        for (var index = 1; index < lines.Length - XmlMemberEnvelopeLineCount; index++)
        {
            if (index > 1)
            {
                _ = builder.Append('\n');
            }

            _ = builder.Append("/// ").Append(lines[index].Trim());
        }

        return PooledBuilder.ToStringAndReturn(builder);
    }

    /// <summary>Generates the partial declaration holding one type's commands.</summary>
    /// <param name="commands">The commands the type declares, all sharing one <see cref="TargetInfo"/>.</param>
    /// <param name="integration">The selected ReactiveUI API surface.</param>
    /// <returns>The file's text.</returns>
    private static string GenerateSource(EquatableArray<CommandInfo> commands, ReactiveUiIntegration integration)
    {
        var target = commands[0].TargetInfo;
        var writer = SourceWriter.Rent()
            .AutoGenerated()
            .BlankLine()
            .DisableWarningsEnableNullable()
            .BlankLine();

        var depth = writer.OpenNamespace(target.TargetNamespace) + writer.OpenContainingTypes(target.ParentInfo);
        _ = writer.OpenPartialType(target);
        for (var i = 0; i < commands.Count; i++)
        {
            if (i > 0)
            {
                _ = writer.BlankLine();
            }

            WriteCommand(writer, commands[i], integration);
        }

        return writer.CloseBlock()
            .CloseBlocks(depth)
            .RestoreNullableAndWarnings()
            .ToStringAndReturn();
    }

    /// <summary>Writes one command: its lazily-created backing field and the property exposing it.</summary>
    /// <param name="writer">The writer, at the level of the type's members.</param>
    /// <param name="commandInfo">The command.</param>
    /// <param name="integration">The selected ReactiveUI API surface.</param>
    private static void WriteCommand(SourceWriter writer, CommandInfo commandInfo, ReactiveUiIntegration integration)
    {
        var outputType = commandInfo.GetOutputTypeText(integration.VoidTypeName);
        var inputType = commandInfo.GetInputTypeText(integration.VoidTypeName);
        var methodName = commandInfo.MethodName;
        var stemStart = GetCommandNameStem(methodName, commandInfo.IsTask, out var stemLength);

        _ = writer.Append("private ");
        WriteCommandType(writer, integration, inputType, outputType);
        _ = writer.Append("? ");
        WriteFieldName(writer, methodName, stemStart, stemLength);
        _ = writer.EndStatement().BlankLine();

        if (!string.IsNullOrEmpty(commandInfo.XmlComment))
        {
            _ = writer.Lines(commandInfo.XmlComment!);
        }

        _ = writer.Line(GeneratedCodeAttribute).ExcludeFromCodeCoverage();
        foreach (var attribute in commandInfo.ForwardedPropertyAttributes.AsImmutableArray())
        {
            _ = writer.Line(attribute);
        }

        _ = writer.Append(commandInfo.AccessModifier).Append(' ');
        WriteCommandType(writer, integration, inputType, outputType);
        _ = writer.Append(' ');
        WriteCommandName(writer, methodName, stemStart, stemLength);
        _ = writer.Append(" { get => ");
        WriteFieldName(writer, methodName, stemStart, stemLength);
        _ = writer.Append(" ??= ").Append(integration.Namespace).Append('.').Append(ReactiveCommand).Append(GetCommandFactoryMethod(commandInfo));
        WriteGenericTypeArguments(writer, commandInfo, inputType, outputType);
        _ = writer.Append('(');
        WriteExecuteArgument(writer, commandInfo);
        WriteCanExecuteArgument(writer, commandInfo);
        WriteSchedulerArguments(writer, commandInfo);
        _ = writer.Line("); }");
    }

    /// <summary>Writes the execute argument: the method group, or for a background task a lambda starting it on the thread pool.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="commandInfo">The command.</param>
    /// <remarks>
    /// The lambda's parameters are typed, so it selects the same factory overload the method group would:
    /// <c>(T p, CancellationToken ct) =&gt; Task.Run(() =&gt; M(p, ct), ct)</c>.
    /// </remarks>
    private static void WriteExecuteArgument(SourceWriter writer, CommandInfo commandInfo)
    {
        if (!commandInfo.IsTask || !commandInfo.RunInBackground)
        {
            _ = writer.Append(commandInfo.MethodName);
            return;
        }

        var hasArgument = commandInfo.ArgumentType is not null;
        var hasToken = commandInfo.HasCancellationToken;
        _ = writer.Append('(');
        if (hasArgument)
        {
            _ = writer.Append(commandInfo.ArgumentType).Append(" p");
        }

        if (hasToken)
        {
            _ = writer.Append(hasArgument ? ", " : null).Append(CancellationTokenType).Append(" ct");
        }

        _ = writer.Append(") => ").Append(TaskRun).Append("(() => ").Append(commandInfo.MethodName).Append('(');
        if (hasArgument)
        {
            _ = writer.Append('p');
        }

        if (hasToken)
        {
            _ = writer.Append(hasArgument ? ", " : null).Append("ct");
        }

        _ = writer.Append(hasToken ? "), ct)" : "))");
    }

    /// <summary>Writes the closed command type, <c>{namespace}.ReactiveCommand&lt;{input}, {output}&gt;</c>.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="integration">The selected ReactiveUI API surface.</param>
    /// <param name="inputType">The command input type.</param>
    /// <param name="outputType">The command output type.</param>
    private static void WriteCommandType(SourceWriter writer, ReactiveUiIntegration integration, string inputType, string outputType) =>
        _ = writer.Append(integration.Namespace).Append('.').Append(ReactiveCommand)
            .Append('<').Append(inputType).Append(", ").Append(outputType).Append('>');

    /// <summary>Writes the factory call's type arguments, which only a command taking a parameter needs.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="commandInfo">The command.</param>
    /// <param name="inputType">The command input type.</param>
    /// <param name="outputType">The command output type.</param>
    private static void WriteGenericTypeArguments(SourceWriter writer, CommandInfo commandInfo, string inputType, string outputType)
    {
        if (commandInfo.ArgumentType is null)
        {
            return;
        }

        _ = writer.Append('<').Append(inputType);
        if (!commandInfo.IsReturnTypeVoid)
        {
            _ = writer.Append(", ").Append(outputType);
        }

        _ = writer.Append('>');
    }

    /// <summary>Writes the optional background- and output-scheduler arguments of a command factory call.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="commandInfo">The command.</param>
    /// <remarks>
    /// <c>CreateRunInBackground</c> takes the background scheduler before the output scheduler, so a synchronous
    /// background command names it, as <see langword="null"/> for ReactiveUI's default, whenever either is set.
    /// ReactiveUI 24 has no overload taking only the execute delegate and a background scheduler, so that call also
    /// names a <see langword="null"/> can-execute, which ReactiveUI 23's all-optional overload accepts too.
    /// </remarks>
    private static void WriteSchedulerArguments(SourceWriter writer, CommandInfo commandInfo)
    {
        var hasOutputScheduler = !string.IsNullOrEmpty(commandInfo.OutputScheduler);
        if (commandInfo.RunInBackground && !commandInfo.IsTask && !commandInfo.IsObservable
            && (hasOutputScheduler || !string.IsNullOrEmpty(commandInfo.BackgroundScheduler)))
        {
            if (!hasOutputScheduler && string.IsNullOrEmpty(commandInfo.CanExecuteObservableName))
            {
                _ = writer.Append(", canExecute: null");
            }

            _ = writer.Append(", backgroundScheduler: ").Append(commandInfo.BackgroundScheduler ?? "null");
        }

        if (!hasOutputScheduler)
        {
            return;
        }

        _ = writer.Append(", outputScheduler: ").Append(commandInfo.OutputScheduler);
    }

    /// <summary>Gets the ReactiveCommand factory method for the command shape.</summary>
    /// <param name="commandInfo">The command metadata.</param>
    /// <returns>The factory method name.</returns>
    private static string GetCommandFactoryMethod(CommandInfo commandInfo)
    {
        if (commandInfo.IsObservable)
        {
            return CreateO;
        }

        if (commandInfo.IsTask)
        {
            return CreateT;
        }

        return commandInfo.RunInBackground ? CreateB : Create;
    }

    /// <summary>Writes the optional can-execute argument of a command factory call.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="commandInfo">The command.</param>
    private static void WriteCanExecuteArgument(SourceWriter writer, CommandInfo commandInfo)
    {
        if (string.IsNullOrEmpty(commandInfo.CanExecuteObservableName))
        {
            return;
        }

        _ = writer.Append(", ")
            .Append(commandInfo.CanExecuteObservableName)
            .Append(commandInfo.CanExecuteTypeInfo == CanExecuteTypeInfo.MethodObservable ? "()" : null);
    }

    /// <summary>Tries to get the expression type for the "CanExecute" property, if available.</summary>
    /// <param name="methodSymbol">The input <see cref="IMethodSymbol"/> instance to process.</param>
    /// <param name="attributeData">The <see cref="AttributeData"/> instance for <paramref name="methodSymbol"/>.</param>
    /// <param name="canExecuteMemberName">The resulting can execute member name, if available.</param>
    /// <param name="canExecuteTypeInfo">The resulting expression type, if available.</param>
    private static void TryGetCanExecuteExpressionType(
        IMethodSymbol methodSymbol,
        AttributeData attributeData,
        out string? canExecuteMemberName,
        out CanExecuteTypeInfo? canExecuteTypeInfo)
    {
        // Get the can execute member, if any
        if (!attributeData.TryGetNamedArgument(CanExecute, out string? memberName))
        {
            canExecuteMemberName = null;
            canExecuteTypeInfo = null;

            return;
        }

        if (memberName is null)
        {
            canExecuteMemberName = null;
            canExecuteTypeInfo = null;
            return;
        }

        var canExecuteSymbols = methodSymbol.ContainingType!.GetAllMembers(memberName);
        var symbolCount = 0;
        ISymbol? canExecuteSymbol = null;
        foreach (var symbol in canExecuteSymbols)
        {
            symbolCount++;
            canExecuteSymbol = symbol;
            if (symbolCount > 1)
            {
                break;
            }
        }

        if (symbolCount == 0)
        {
            // Special case for when the target member is a generated property from [ObservableProperty]
            if (TryGetCanExecuteMemberFromGeneratedProperty(memberName, methodSymbol.ContainingType, out canExecuteTypeInfo))
            {
                canExecuteMemberName = memberName;

                return;
            }
        }
        else if (symbolCount == 1
                 && TryGetCanExecuteExpressionFromSymbol(canExecuteSymbol!, out canExecuteTypeInfo))
        {
            canExecuteMemberName = memberName;

            return;
        }

        canExecuteMemberName = null;
        canExecuteTypeInfo = null;
    }

    /// <summary>Gets the scheduler an attribute property names, as an expression generated code can use.</summary>
    /// <param name="context">The generator attribute context.</param>
    /// <param name="methodSymbol">The attributed command method.</param>
    /// <param name="attributeData">The command attribute.</param>
    /// <param name="argumentName">The attribute property naming the scheduler.</param>
    /// <returns>The scheduler expression, or <see langword="null"/> when none is named or the name does not resolve.</returns>
    /// <remarks>A name that does not resolve is reported by the command analyzer, which applies the same rules.</remarks>
    private static string? GetScheduler(
        in GeneratorAttributeSyntaxContext context,
        IMethodSymbol methodSymbol,
        AttributeData attributeData,
        string argumentName) =>
        attributeData.TryGetNamedArgument(argumentName, out string? name)
        && name is not null
        && ReactiveCommandRules.TryResolveScheduler(
            context.SemanticModel,
            attributeData.ApplicationSyntaxReference?.Span.Start ?? context.TargetNode.SpanStart,
            methodSymbol.ContainingType,
            name,
            out var expression)
            ? expression
            : null;

    /// <summary>Gets the expression type for the can execute logic, if possible.</summary>
    /// <param name="canExecuteSymbol">The can execute member symbol (either a method or a property).</param>
    /// <param name="canExecuteTypeInfo">The resulting can execute expression type, if available.</param>
    /// <returns>Whether or not <paramref name="canExecuteTypeInfo"/> was set and the input symbol was valid.</returns>
    private static bool TryGetCanExecuteExpressionFromSymbol(
        ISymbol canExecuteSymbol,
        [NotNullWhen(true)] out CanExecuteTypeInfo? canExecuteTypeInfo)
    {
        switch (canExecuteSymbol)
        {
            case IMethodSymbol methodSymbol when methodSymbol.ReturnType.IsObservableBoolType() && methodSymbol.Parameters.IsEmpty:
            {
                canExecuteTypeInfo = CanExecuteTypeInfo.MethodObservable;
                return true;
            }

            case IPropertySymbol { GetMethod: not null } propertySymbol when propertySymbol.Type.IsObservableBoolType():
            {
                canExecuteTypeInfo = CanExecuteTypeInfo.PropertyObservable;
                return true;
            }

            case IFieldSymbol fieldSymbol when fieldSymbol.Type.IsObservableBoolType():
            {
                canExecuteTypeInfo = CanExecuteTypeInfo.FieldObservable;
                return true;
            }

            default:
            {
                canExecuteTypeInfo = null;
                return false;
            }
        }
    }

    /// <summary>Gets the expression type for the can execute logic, if possible.</summary>
    /// <param name="memberName">The member name passed to <c>[ReactiveCommand(CanExecute = ...)]</c>.</param>
    /// <param name="containingType">The containing type for the method annotated with <c>[ReactiveCommand]</c>.</param>
    /// <param name="canExecuteTypeInfo">The resulting can execute expression type, if available.</param>
    /// <returns>Whether or not <paramref name="canExecuteTypeInfo"/> was set and the input symbol was valid.</returns>
    private static bool TryGetCanExecuteMemberFromGeneratedProperty(
        string memberName,
        INamedTypeSymbol containingType,
        [NotNullWhen(true)] out CanExecuteTypeInfo? canExecuteTypeInfo)
    {
        foreach (var memberSymbol in containingType.GetAllMembers())
        {
            // Only look for instance fields of Observable bool type
            if (memberSymbol is not IFieldSymbol fieldSymbol || !fieldSymbol.Type.IsObservableBoolType())
            {
                continue;
            }

            // Only filter fields with the [Reactive] attribute
            if (!HasReactiveAttribute(memberSymbol))
            {
                continue;
            }

            // Get the target property name either directly or matching the generated one
            var propertyName = fieldSymbol.GetGeneratedPropertyName();

            // If the generated property name matches, get the right expression type
            if (memberName == propertyName)
            {
                canExecuteTypeInfo = CanExecuteTypeInfo.PropertyObservable;

                return true;
            }
        }

        canExecuteTypeInfo = null;

        return false;
    }

    /// <summary>Determines whether a symbol has the <c>ReactiveAttribute</c>.</summary>
    /// <param name="symbol">The symbol to inspect.</param>
    /// <returns><see langword="true"/> when the attribute is present.</returns>
    private static bool HasReactiveAttribute(ISymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.HasFullyQualifiedMetadataName(AttributeDefinitions.ReactiveAttributeType) == true)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Locates the stem of a method name that the command's property and field names are built from.</summary>
    /// <param name="methodName">The source method name.</param>
    /// <param name="isAsync">Whether the method is asynchronous.</param>
    /// <param name="length">The stem's length.</param>
    /// <returns>The index the stem starts at.</returns>
    /// <remarks>The stem drops an <c>m_</c> or leading-underscore prefix, and an <c>Async</c> suffix on an asynchronous method.</remarks>
    private static int GetCommandNameStem(string methodName, bool isAsync, out int length)
    {
        var start = 0;
        if (methodName.StartsWith("m_", StringComparison.Ordinal))
        {
            start = CommandNamePrefixLength;
        }
        else
        {
            while (start < methodName.Length && methodName[start] == '_')
            {
                start++;
            }
        }

        length = methodName.Length - start;
        if (isAsync
            && length >= AsyncSuffix.Length
            && string.CompareOrdinal(methodName, methodName.Length - AsyncSuffix.Length, AsyncSuffix, 0, AsyncSuffix.Length) == 0)
        {
            length -= AsyncSuffix.Length;
        }

        return start;
    }

    /// <summary>Writes the generated command property's name: the stem, capitalized, and <c>Command</c>.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="methodName">The source method name.</param>
    /// <param name="stemStart">The index the stem starts at.</param>
    /// <param name="stemLength">The stem's length.</param>
    private static void WriteCommandName(SourceWriter writer, string methodName, int stemStart, int stemLength) =>
        _ = writer.Append(char.ToUpper(methodName[stemStart], CultureInfo.InvariantCulture))
            .Append(methodName, stemStart + 1, stemLength - 1)
            .Append(CommandSuffix);

    /// <summary>Writes the generated backing field's name: the command property's name, camel-cased behind an underscore.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="methodName">The source method name.</param>
    /// <param name="stemStart">The index the stem starts at.</param>
    /// <param name="stemLength">The stem's length.</param>
    private static void WriteFieldName(SourceWriter writer, string methodName, int stemStart, int stemLength)
    {
        var first = char.ToUpper(methodName[stemStart], CultureInfo.InvariantCulture);
        _ = writer.Append('_')
            .Append(char.ToLower(first, CultureInfo.InvariantCulture))
            .Append(methodName, stemStart + 1, stemLength - 1)
            .Append(CommandSuffix);
    }
}
