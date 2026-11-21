// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Emits the conversion stage of a binding: a generated conversion, or the runtime's when there is none.</summary>
internal static class ConversionEmitter
{
    /// <summary>Emits a conversion stage using the default hint and converter selection.</summary>
    /// <param name="builder">The writer, inside the worker's body.</param>
    /// <param name="source">The source observable variable.</param>
    /// <param name="result">The converted observable variable.</param>
    /// <param name="from">The declared input type.</param>
    /// <param name="to">The declared output type.</param>
    /// <param name="conversion">The selected generated conversion.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EmitStage(SourceWriter builder, string source, string result, string from, string to, ConversionInfo? conversion) =>
        EmitStage(builder, source, result, from, to, conversion, new("null", "null"));

    /// <summary>Emits conversion selection once per binding and typed delivery for each value.</summary>
    /// <param name="builder">The writer, inside the worker's body.</param>
    /// <param name="source">The source observable variable.</param>
    /// <param name="result">The converted observable variable.</param>
    /// <param name="from">The declared input type.</param>
    /// <param name="to">The declared output type.</param>
    /// <param name="conversion">The selected generated conversion.</param>
    /// <param name="parameters">The caller's hint and converter override.</param>
    internal static void EmitStage(
        SourceWriter builder,
        string source,
        string result,
        string from,
        string to,
        ConversionInfo? conversion,
        ConversionParameters parameters)
    {
        if (conversion is not null)
        {
            AppendGeneratedStage(builder, source, result, from, to, conversion, parameters);
            return;
        }

        _ = builder.BeginVar(result)
            .Append($"{GeneratedTypeNames.LinqExtensions}.Choose<").Append(from).Append(", ").Append(to).Append(">(")
            .OpenContinuation()
            .Append(source).Line(",")
            .Line("__value =>")
            .OpenBlock();
        AppendRuntimeBody(builder, from, to, parameters);
        _ = builder.CloseBlock(");").Outdent();
    }

    /// <summary>Hands a generated conversion to the runtime, which runs it unless a converter outranks it.</summary>
    /// <param name="builder">The writer, inside the worker's body.</param>
    /// <param name="source">The source observable variable.</param>
    /// <param name="result">The converted observable variable.</param>
    /// <param name="from">The input type.</param>
    /// <param name="to">The output type.</param>
    /// <param name="conversion">The generated conversion.</param>
    /// <param name="parameters">The caller's hint and converter override.</param>
    /// <remarks>
    /// Choosing the converter and calling it are the same for every binding, so the runtime does both. The
    /// generated code states only the conversion itself, which reads the value and the hint it is passed.
    /// </remarks>
    private static void AppendGeneratedStage(
        SourceWriter builder,
        string source,
        string result,
        string from,
        string to,
        ConversionInfo conversion,
        ConversionParameters parameters)
    {
        _ = builder.BeginVar(result)
            .Append($"{GeneratedTypeNames.GeneratedConversion}.Convert<").Append(from).Append(", ").Append(to).Append(">(")
            .OpenContinuation()
            .Append(source).Line(",")
            .Append(parameters.Override).Line(",")
            .Append(parameters.Hint).Line(",")
            .Append(conversion.Affinity).Line(",")
            .AppendLiteral(conversion.IsIdentity).Line(",");

        if (conversion.Preparation.Length == 0)
        {
            _ = builder.Append("(__value, __hint) => ");
            _ = AppendGeneratedResult(builder, to, conversion).Line(",");
        }
        else
        {
            _ = builder.Line("(__value, __hint) =>")
                .OpenBlock()
                .Line(conversion.Preparation)
                .BeginReturn();
            _ = AppendGeneratedResult(builder, to, conversion).EndStatement()
                .CloseBlock(",");
        }

        _ = conversion.AssignmentFallback is null
            ? builder.Append("null")
            : builder.Append("__value => ").Append(conversion.AssignmentFallback);
        _ = builder.Line(");").Outdent();
    }

    /// <summary>Writes what the generated conversion produces for one value.</summary>
    /// <param name="builder">The writer, part way through the line the result is written on.</param>
    /// <param name="to">The output type.</param>
    /// <param name="conversion">The generated conversion.</param>
    /// <returns>The writer, with the line left open after the result.</returns>
    private static SourceWriter AppendGeneratedResult(SourceWriter builder, string to, ConversionInfo conversion)
    {
        _ = builder.Append(conversion.Condition).Append(" ? (true, ").Append(conversion.Expression).Append(") : ");
        return conversion.AssignmentFallback is null
            ? builder.Append("(false, default(").Append(to).Append("))")
            : builder.Append("(true, ").Append(conversion.AssignmentFallback).Append(')');
    }

    /// <summary>Emits the runtime route when no compile-time conversion is available.</summary>
    /// <param name="builder">The writer, inside the chooser's body.</param>
    /// <param name="from">The input type.</param>
    /// <param name="to">The output type.</param>
    /// <param name="parameters">The caller's conversion inputs.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendRuntimeBody(SourceWriter builder, string from, string to, ConversionParameters parameters) =>
        builder.Append(to).Line(" __converted;")
            .BeginVar("__success").Append(GeneratedTypeNames.RuntimeBindingConverter).Append(".TryConvert<")
            .Append(from).Append(", ").Append(to).Append(">(__value, ").Append(parameters.Hint).Append(", ").Append(parameters.Override).Line(", out __converted);")
            .Return("(__success, __converted)");
}
