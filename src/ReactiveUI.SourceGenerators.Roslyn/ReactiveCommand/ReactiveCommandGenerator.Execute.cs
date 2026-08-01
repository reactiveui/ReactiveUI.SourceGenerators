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

    /// <summary>The attribute property used to specify the output scheduler.</summary>
    private const string OutputScheduler = "OutputScheduler";

    /// <summary>The attribute property used to request background execution.</summary>
    private const string RunInBackground = "RunInBackground";

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
        TryGetOutputScheduler(methodSymbol, attributeData, context.SemanticModel.Compilation.GetReactiveUiIntegration(), out var outputScheduler);
        token.ThrowIfCancellationRequested();
        var runInBackground = attributeData.GetNamedArgument<bool>(RunInBackground);
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
            forwardedPropertyAttributes,
            accessModifier,
            GetXmlDocumentation(methodSymbol, token));
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
        if (string.IsNullOrEmpty(xmlDocumentation))
        {
            return string.Empty;
        }

        var lines = xmlDocumentation.Split('\n');
        if (lines.Length < 3)
        {
            return string.Empty;
        }

        var formattedDocumentation = new System.Text.StringBuilder();
        const int XmlMemberEnvelopeLineCount = 2;
        for (var index = 1; index < lines.Length - XmlMemberEnvelopeLineCount; index++)
        {
            _ = formattedDocumentation.Append("        /// ")
                .AppendLine(lines[index].TrimStart());
        }

        return formattedDocumentation.ToString().TrimEnd();
    }

    /// <summary>Generates the complete source file for a target type's commands.</summary>
    /// <param name="containingTypeName">The containing type name.</param>
    /// <param name="containingNamespace">The containing namespace.</param>
    /// <param name="containingClassVisibility">The containing type visibility.</param>
    /// <param name="containingType">The containing type keyword.</param>
    /// <param name="commands">The command metadata.</param>
    /// <param name="integration">The selected ReactiveUI API surface.</param>
    /// <returns>The generated source file.</returns>
    private static string GenerateSource(
        string containingTypeName,
        string containingNamespace,
        string containingClassVisibility,
        string containingType,
        CommandInfo[] commands,
        ReactiveUiIntegration integration)
    {
        // Get Parent class details from properties.ParentInfo
        var parentTypes = new TargetInfo?[commands.Length];
        for (var index = 0; index < commands.Length; index++)
        {
            parentTypes[index] = commands[index].TargetInfo.ParentInfo;
        }

        var (parentClassDeclarationsString, closingBrackets) = TargetInfo.GenerateParentClassDeclarations(parentTypes);

        var classes = GenerateClassWithCommands(containingTypeName, containingClassVisibility, containingType, commands, integration);

        return
$$"""
// <auto-generated/>

#pragma warning disable
#nullable enable

namespace {{containingNamespace}}
{
    {{parentClassDeclarationsString}}{{classes}}{{closingBrackets}}
}
#nullable restore
#pragma warning restore
""";
    }

    /// <summary>Generates the source code.</summary>
    /// <param name="containingTypeName">The contain type name.</param>
    /// <param name="containingClassVisibility">The containing class visibility.</param>
    /// <param name="containingType">The containing type.</param>
    /// <param name="commands">The commands.</param>
    /// <param name="integration">The selected ReactiveUI API surface.</param>
    /// <returns>The value.</returns>
    private static string GenerateClassWithCommands(
        string containingTypeName,
        string containingClassVisibility,
        string containingType,
        CommandInfo[] commands,
        ReactiveUiIntegration integration)
    {
        // Includes 2 tabs from the property declarations so no need to add them here.
        var commandDeclarationsBuilder = new System.Text.StringBuilder();
        foreach (var command in commands)
        {
            if (commandDeclarationsBuilder.Length > 0)
            {
                _ = commandDeclarationsBuilder.AppendLine();
            }

            _ = commandDeclarationsBuilder.Append(GetCommandSyntax(command, integration));
        }

        var commandDeclarations = commandDeclarationsBuilder.ToString();

        return
$$"""

    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
{{commandDeclarations}}
    }
""";
    }

    /// <summary>Generates the property declaration for a command.</summary>
    /// <param name="commandExtensionInfo">The command metadata.</param>
    /// <param name="integration">The selected ReactiveUI API surface.</param>
    /// <returns>The generated property declaration.</returns>
    private static string GetCommandSyntax(CommandInfo commandExtensionInfo, ReactiveUiIntegration integration)
    {
        var outputType = commandExtensionInfo.GetOutputTypeText(integration.VoidTypeName);
        var inputType = commandExtensionInfo.GetInputTypeText(integration.VoidTypeName);
        var rxCmd = $"{integration.Namespace}.{ReactiveCommand}";
        var commandName = GetGeneratedCommandName(commandExtensionInfo.MethodName, commandExtensionInfo.IsTask);
        var fieldName = GetGeneratedFieldName(commandName);

        var initializer = GetCommandInitializer(commandExtensionInfo, fieldName, outputType, inputType, rxCmd);

        // Prepare any forwarded property attributes
        var forwardedPropertyAttributesString = GetForwardedPropertyAttributes(commandExtensionInfo);

        return
$$"""
        private {{rxCmd}}<{{inputType}}, {{outputType}}>? {{fieldName}};

{{commandExtensionInfo.XmlComment}}
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
        {{forwardedPropertyAttributesString}}
        {{commandExtensionInfo.AccessModifier}} {{rxCmd}}<{{inputType}}, {{outputType}}> {{commandName}} { get => {{initializer}} }
""";
    }

    /// <summary>Creates the initializer expression for a generated command.</summary>
    /// <param name="commandInfo">The command metadata.</param>
    /// <param name="fieldName">The backing-field name.</param>
    /// <param name="outputType">The generated command output type.</param>
    /// <param name="inputType">The generated command input type.</param>
    /// <param name="commandTypeName">The fully-qualified reactive command type name.</param>
    /// <returns>The lazy-initializer expression.</returns>
    private static string GetCommandInitializer(CommandInfo commandInfo, string fieldName, string outputType, string inputType, string commandTypeName)
    {
        var genericTypeArguments = string.Empty;
        if (commandInfo.ArgumentType is not null)
        {
            genericTypeArguments = commandInfo.IsReturnTypeVoid
                ? $"<{inputType}>"
                : $"<{inputType}, {outputType}>";
        }

        var commandType = GetCommandFactoryMethod(commandInfo);
        var canExecuteArgument = GetCanExecuteArgument(commandInfo);
        var schedulerArgument = GetOutputSchedulerArgument(commandInfo);
        return $"{fieldName} ??= {commandTypeName}{commandType}{genericTypeArguments}({commandInfo.MethodName}{canExecuteArgument}{schedulerArgument});";
    }

    /// <summary>Gets the optional output-scheduler arguments for a command factory call.</summary>
    /// <param name="commandInfo">The command metadata.</param>
    /// <returns>The formatted scheduler arguments, or an empty string.</returns>
    private static string GetOutputSchedulerArgument(CommandInfo commandInfo)
    {
        if (string.IsNullOrEmpty(commandInfo.OutputScheduler))
        {
            return string.Empty;
        }

        return commandInfo.RunInBackground && !commandInfo.IsTask && !commandInfo.IsObservable
            ? $", backgroundScheduler: null, outputScheduler: {commandInfo.OutputScheduler}"
            : $", outputScheduler: {commandInfo.OutputScheduler}";
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

    /// <summary>Gets the optional can-execute argument for a command factory call.</summary>
    /// <param name="commandInfo">The command metadata.</param>
    /// <returns>The formatted can-execute argument, or an empty string.</returns>
    private static string GetCanExecuteArgument(CommandInfo commandInfo)
    {
        if (string.IsNullOrEmpty(commandInfo.CanExecuteObservableName))
        {
            return string.Empty;
        }

        var invocationSuffix = commandInfo.CanExecuteTypeInfo == CanExecuteTypeInfo.MethodObservable ? "()" : string.Empty;
        return $", {commandInfo.CanExecuteObservableName}{invocationSuffix}";
    }

    /// <summary>Formats the attributes forwarded to a generated command property.</summary>
    /// <param name="commandInfo">The command metadata.</param>
    /// <returns>The formatted attribute list.</returns>
    private static string GetForwardedPropertyAttributes(CommandInfo commandInfo)
    {
        var builder = new System.Text.StringBuilder();
        foreach (var attribute in AttributeDefinitions.ExcludeFromCodeCoverage)
        {
            AppendForwardedAttribute(builder, attribute);
        }

        foreach (var attribute in commandInfo.ForwardedPropertyAttributes.AsImmutableArray())
        {
            AppendForwardedAttribute(builder, attribute);
        }

        return builder.ToString();
    }

    /// <summary>Appends an indented forwarded attribute to a source builder.</summary>
    /// <param name="builder">The target builder.</param>
    /// <param name="attribute">The attribute text.</param>
    private static void AppendForwardedAttribute(System.Text.StringBuilder builder, string attribute)
    {
        if (builder.Length > 0)
        {
                _ = builder.Append("\n        ");
        }

        _ = builder.Append(attribute);
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

    /// <summary>Gets the configured output scheduler, when its member is valid.</summary>
    /// <param name="methodSymbol">The attributed command method.</param>
    /// <param name="attributeData">The command attribute.</param>
    /// <param name="integration">The selected ReactiveUI API surface.</param>
    /// <param name="outputScheduler">The scheduler expression, when valid.</param>
    private static void TryGetOutputScheduler(
        IMethodSymbol methodSymbol,
        AttributeData attributeData,
        ReactiveUiIntegration integration,
        out string? outputScheduler)
    {
        if (!attributeData.TryGetNamedArgument(OutputScheduler, out string? scheduler) || scheduler is null)
        {
            outputScheduler = null;
            return;
        }

        if (IsReactiveUiScheduler(scheduler))
        {
            outputScheduler = scheduler;
            return;
        }

        if (!TryGetSingleMember(methodSymbol.ContainingType!.GetAllMembers(scheduler), out var outputSchedulerSymbol))
        {
            outputScheduler = null;
            return;
        }

        _ = TryGetOutputSchedulerFromSymbol(outputSchedulerSymbol, integration.Api, out outputScheduler);
    }

    /// <summary>Determines whether a scheduler expression names a built-in ReactiveUI scheduler.</summary>
    /// <param name="scheduler">The scheduler expression.</param>
    /// <returns><see langword="true"/> when the expression is a built-in scheduler.</returns>
    private static bool IsReactiveUiScheduler(string scheduler) =>
        scheduler is "global::ReactiveUI.RxSchedulers.MainThreadScheduler"
            or "global::ReactiveUI.RxSchedulers.TaskpoolScheduler"
            or "global::ReactiveUI.Reactive.RxSchedulers.MainThreadScheduler"
            or "global::ReactiveUI.Reactive.RxSchedulers.TaskpoolScheduler";

    /// <summary>Gets the only symbol from a candidate sequence.</summary>
    /// <param name="symbols">The candidate symbols.</param>
    /// <param name="symbol">The only symbol, when present.</param>
    /// <returns><see langword="true"/> when exactly one symbol exists.</returns>
    private static bool TryGetSingleMember(IEnumerable<ISymbol> symbols, [NotNullWhen(true)] out ISymbol? symbol)
    {
        symbol = null;
        foreach (var candidate in symbols)
        {
            if (symbol is not null)
            {
                return false;
            }

            symbol = candidate;
        }

        return symbol is not null;
    }

    /// <summary>Validates a candidate scheduler symbol and gets its expression.</summary>
    /// <param name="outputSchedulerSymbol">The candidate scheduler symbol.</param>
    /// <param name="api">The selected ReactiveUI API.</param>
    /// <param name="outputScheduler">The scheduler expression, when valid.</param>
    /// <returns><see langword="true"/> when the symbol is a supported scheduler.</returns>
    private static bool TryGetOutputSchedulerFromSymbol(
        ISymbol outputSchedulerSymbol,
        ReactiveUiApi api,
        [NotNullWhen(true)] out string? outputScheduler)
    {
        switch (outputSchedulerSymbol)
        {
            case IFieldSymbol fieldSymbol when fieldSymbol.Type.IsSchedulerType(api):
            {
                outputScheduler = fieldSymbol.Name;
                return true;
            }

            case IPropertySymbol { GetMethod: not null } propertySymbol when propertySymbol.Type.IsSchedulerType(api):
            {
                outputScheduler = propertySymbol.Name;
                return true;
            }

            case IMethodSymbol methodSymbol when methodSymbol.ReturnType.IsSchedulerType(api):
            {
                outputScheduler = $"{methodSymbol.Name}()";
                return true;
            }

            default:
            {
                outputScheduler = null;
                return false;
            }
        }
    }

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

    /// <summary>Gets the generated command property name for a method.</summary>
    /// <param name="methodName">The source method name.</param>
    /// <param name="isAsync">Whether the method is asynchronous.</param>
    /// <returns>The generated command property name.</returns>
    private static string GetGeneratedCommandName(string methodName, bool isAsync)
    {
        var commandName = methodName;

        if (commandName.StartsWith("m_", System.StringComparison.Ordinal))
        {
            commandName = commandName[CommandNamePrefixLength..];
        }
        else if (commandName.StartsWith("_", System.StringComparison.Ordinal))
        {
            commandName = commandName.TrimStart('_');
        }

        if (commandName.EndsWith("Async", System.StringComparison.Ordinal) && isAsync)
        {
            commandName = commandName.Substring(0, commandName.Length - "Async".Length);
        }

        return $"{char.ToUpper(commandName[0], CultureInfo.InvariantCulture)}{commandName[1..]}Command";
    }

    /// <summary>Gets the generated backing-field name for a command property.</summary>
    /// <param name="generatedCommandName">The generated command property name.</param>
    /// <returns>The generated backing-field name.</returns>
    private static string GetGeneratedFieldName(string generatedCommandName) =>
        $"_{char.ToLower(generatedCommandName[0], CultureInfo.InvariantCulture)}{generatedCommandName[1..]}";
}
