// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Builds the compilation call sites are read from when the consumer uses ReactiveUI.SourceGenerators or a XAML source
/// generator: the consumer's compilation, plus declarations of the members those generators add.
/// </summary>
/// <remarks>
/// <para>
/// Every generator reads the compilation as the consumer wrote it, without any generator's output. A call site that
/// binds a property ReactiveUI.SourceGenerators writes, such as the one <c>[Reactive]</c> writes for a field, names a
/// member that does not exist yet, so it cannot be read at all. A class marked <c>[IReactiveObject]</c> looks as if it
/// raised no notification. A control named in a MAUI or Avalonia page is a field those platforms' generators write, so a
/// binding to it cannot be read either.
/// </para>
/// <para>
/// This pipeline declares those members and interfaces in one file that is never emitted, and adds it to a private copy
/// of the compilation. Call sites are read from that copy, so they see the same members the final build does. A
/// compilation that uses none of the attributes gets no copy, and its call sites are read as before.
/// </para>
/// </remarks>
internal static class SourceGeneratorsCompilation
{
    /// <summary>The capacity the declarations file starts with.</summary>
    private const int DeclarationsCapacity = 1024;

    /// <summary>Registers the attribute pipelines and returns the compilation call sites are read from.</summary>
    /// <param name="context">The generator initialization context.</param>
    /// <returns>The compilation with the declared members, or null when the consumer uses none of the attributes.</returns>
    internal static IncrementalValueProvider<Compilation?> Register(in IncrementalGeneratorInitializationContext context)
    {
        Func<SyntaxNode, CancellationToken, bool> isVariable = static (node, _) => node is VariableDeclaratorSyntax;
        Func<SyntaxNode, CancellationToken, bool> isMethod = static (node, _) => node is MethodDeclarationSyntax;
        Func<SyntaxNode, CancellationToken, bool> isType = static (node, _) => node is TypeDeclarationSyntax;

        var reactive = Collect(in context, Constants.SourceGeneratorsReactiveAttributeMetadataName, isVariable, SourceGeneratorsMemberExtractor.ExtractReactive);
        var collections = Collect(in context, Constants.SourceGeneratorsReactiveCollectionAttributeMetadataName, isVariable, SourceGeneratorsMemberExtractor.ExtractReactiveCollection);
        var derivedLists = Collect(in context, Constants.SourceGeneratorsBindableDerivedListAttributeMetadataName, isVariable, SourceGeneratorsMemberExtractor.ExtractBindableDerivedList);
        var commands = Collect(in context, Constants.SourceGeneratorsReactiveCommandAttributeMetadataName, isMethod, SourceGeneratorsMemberExtractor.ExtractReactiveCommand);
        var reactiveObjects = Collect(in context, Constants.SourceGeneratorsIReactiveObjectAttributeMetadataName, isType, SourceGeneratorsMemberExtractor.ExtractIReactiveObject);

        // XAML pages are read once per change to the file; their types are resolved against the compilation.
        var xamlFields = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (data, ct) => XamlPageReader.Read(data.Left, data.Right.GetOptions(data.Left), data.Right.GlobalOptions, ct))
            .Where(static page => page is not null)
            .Select(static (page, _) => page!)
            .Collect()
            .Combine(context.CompilationProvider)
            .Select(static (data, ct) => XamlMemberResolver.Resolve(data.Left, data.Right, ct));

        var declarations = reactiveObjects
            .Combine(reactive)
            .Combine(collections)
            .Combine(derivedLists)
            .Combine(commands)
            .Combine(xamlFields)
            .Select(static (data, _) => WriteDeclarations(
                data.Left.Left.Left.Left.Left,
                data.Left.Left.Left.Left.Right,
                data.Left.Left.Left.Right,
                data.Left.Left.Right,
                data.Left.Right,
                data.Right));

        // The declarations are parsed once per change to them, not once per edit anywhere in the compilation.
        var tree = declarations
            .Combine(context.ParseOptionsProvider)
            .Select(static (data, ct) => data.Left.Length == 0
                ? null
                : CSharpSyntaxTree.ParseText(data.Left, data.Right as CSharpParseOptions, cancellationToken: ct));

        return context.CompilationProvider
            .Combine(tree)
            .Select(static (data, _) => data.Right is null ? null : data.Left.AddSyntaxTrees(data.Right));
    }

    /// <summary>Writes the declarations of every member and interface, grouped by the type they belong to.</summary>
    /// <param name="groups">The members each attribute describes, with nulls for the ones it declined.</param>
    /// <returns>The declarations file, or an empty string when there is nothing to declare.</returns>
    internal static string WriteDeclarations(params ImmutableArray<SourceGeneratorsMember?>[] groups)
    {
        var order = new List<PartialTypeDeclaration>();
        var interfaces = new Dictionary<PartialTypeDeclaration, string>();
        var properties = new Dictionary<PartialTypeDeclaration, List<string>>();
        foreach (var group in groups)
        {
            foreach (var member in group)
            {
                if (member is not null)
                {
                    Add(member, order, interfaces, properties);
                }
            }
        }

        if (order.Count == 0)
        {
            return string.Empty;
        }

        var writer = SourceWriter.Rent(DeclarationsCapacity);
        foreach (var declaration in order)
        {
            var opened = interfaces.TryGetValue(declaration, out var implemented)
                ? declaration with { TypeHeaders = WithInterface(declaration.TypeHeaders, implemented) }
                : declaration;

            CodeGeneratorHelpers.OpenPartialDeclaration(writer, opened);
            foreach (var property in properties[declaration])
            {
                _ = writer.Line(property);
            }

            CodeGeneratorHelpers.ClosePartialDeclaration(writer, opened);
        }

        return writer.ToStringAndReturn();
    }

    /// <summary>Files a member under the type it belongs to, keeping the order types are first seen in.</summary>
    /// <param name="member">The member.</param>
    /// <param name="order">The types, in the order they were first seen.</param>
    /// <param name="interfaces">The interface each type gains.</param>
    /// <param name="properties">The properties each type gains.</param>
    private static void Add(
        SourceGeneratorsMember member,
        List<PartialTypeDeclaration> order,
        Dictionary<PartialTypeDeclaration, string> interfaces,
        Dictionary<PartialTypeDeclaration, List<string>> properties)
    {
        if (!properties.TryGetValue(member.Declaration, out var list))
        {
            list = [];
            properties.Add(member.Declaration, list);
            order.Add(member.Declaration);
        }

        if (member.Interface is { } implemented)
        {
            interfaces[member.Declaration] = implemented;
            return;
        }

        list.Add(member.Property!);
    }

    /// <summary>Runs one attribute's pipeline and gathers what it describes.</summary>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="metadataName">The attribute's metadata name.</param>
    /// <param name="predicate">The kind of declaration the attribute is placed on.</param>
    /// <param name="extract">The description of what the attribute adds.</param>
    /// <returns>Every description, with nulls for the declarations the attribute adds nothing to.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IncrementalValueProvider<ImmutableArray<SourceGeneratorsMember?>> Collect(
        in IncrementalGeneratorInitializationContext context,
        string metadataName,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, SourceGeneratorsMember?> extract) =>
        context.SyntaxProvider.ForAttributeWithMetadataName(metadataName, predicate, extract).Collect();

    /// <summary>Adds an interface to the innermost type's header.</summary>
    /// <param name="headers">The headers, outermost first.</param>
    /// <param name="implemented">The fully qualified interface.</param>
    /// <returns>The headers with the interface on the last one.</returns>
    private static EquatableArray<string> WithInterface(EquatableArray<string> headers, string implemented)
    {
        var copy = new string[headers.Length];
        for (var i = 0; i < copy.Length; i++)
        {
            copy[i] = headers[i];
        }

        copy[^1] = $"{copy[^1]} : {implemented}";
        return new(copy);
    }
}
