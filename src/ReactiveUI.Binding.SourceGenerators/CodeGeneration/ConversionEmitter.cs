// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Emits typed conversions, with legacy adaptation confined to a winning custom converter.</summary>
internal static class ConversionEmitter
{
    /// <summary>Completes the name of the local holding a stage's selected converter.</summary>
    private const string ConverterSuffix = "Converter";

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
            AppendSelection(builder, result, from, to, conversion, parameters);
        }

        _ = builder.BeginVar(result);
        if (conversion?.IsIdentity == true)
        {
            _ = builder.Append(result).Append(ConverterSuffix).Append($" == null ? ({GeneratedTypeNames.IObservable}<").Append(to).Append(">)").Append(source).Append(" : ");
        }

        _ = builder.Append($"{GeneratedTypeNames.LinqExtensions}.Choose<").Append(from).Append(", ").Append(to).Append(">(")
            .OpenContinuation()
            .Append(source).Line(",")
            .Line("__value =>")
            .OpenBlock();

        if (conversion is null)
        {
            AppendRuntimeBody(builder, from, to, parameters);
        }
        else
        {
            AppendNativeBody(builder, result, from, to, conversion, parameters);
        }

        _ = builder.CloseBlock(");").Outdent();
    }

    /// <summary>Selects an explicit converter or a registration that beats the generated score.</summary>
    /// <param name="builder">The writer, inside the worker's body.</param>
    /// <param name="result">The stage variable prefix.</param>
    /// <param name="from">The input type.</param>
    /// <param name="to">The output type.</param>
    /// <param name="conversion">The generated candidate.</param>
    /// <param name="parameters">The caller's conversion inputs.</param>
    private static void AppendSelection(SourceWriter builder, string result, string from, string to, ConversionInfo conversion, ConversionParameters parameters) =>
        _ = builder.Append($"{GeneratedTypeNames.IBindingTypeConverter} ").Append(result).Append(ConverterSuffix).Append(" = ").Append(parameters.Override).EndStatement()
            .BeginIf().Append(result).Append(ConverterSuffix).Append(" == null").CloseCondition()
            .Append(result).Append("Converter = global::ReactiveUI.Binding.BindingConverters.Current.TypedConverters.TryGetConverter(typeof(")
            .Append(from).Append("), typeof(").Append(to).Line("));")
            .BeginIf().Append(result).Append("Converter != null && ").Append(result).Append("Converter.GetAffinityForObjects() <= ")
            .Append(conversion.Affinity).CloseCondition()
            .Append(result).Line("Converter = null;")
            .CloseBlock()
            .CloseBlock();

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

    /// <summary>Dispatches a winning custom provider before evaluating the generated conversion.</summary>
    /// <param name="builder">The writer, inside the chooser's body.</param>
    /// <param name="result">The stage variable prefix.</param>
    /// <param name="from">The input type.</param>
    /// <param name="to">The output type.</param>
    /// <param name="conversion">The generated candidate.</param>
    /// <param name="parameters">The caller's conversion inputs.</param>
    private static void AppendNativeBody(SourceWriter builder, string result, string from, string to, ConversionInfo conversion, ConversionParameters parameters)
    {
        _ = builder.Append("object __hint = ").Append(parameters.Hint).EndStatement()
            .BeginIf().Append(result).Append(ConverterSuffix).Append(" != null").CloseCondition()
            .BeginIf().Append(result).Append($"Converter is {GeneratedTypeNames.IBindingTypeConverter}<")
            .Append(from).Append(", ").Append(to).Append("> __typed").CloseCondition()
            .Append(to).Line(" __converted;")
            .If("__typed.TryConvert(__value, __hint, out __converted)")
            .Return("(true, __converted)")
            .CloseBlock()
            .CloseBlock()
            .Else()
            .Line("object __boxed;")
            .BeginIf().Append(result).Append("Converter.TryConvertTyped(__value, __hint, out __boxed)").CloseCondition()
            .BeginReturn().Append("(true, (").Append(to).Line(")__boxed);")
            .CloseBlock()
            .CloseBlock();
        AppendRejected(builder, to, conversion, parameters);
        _ = builder.CloseBlock();
        if (conversion.Preparation.Length != 0)
        {
            _ = builder.Line(conversion.Preparation);
        }

        _ = builder.BeginReturn().Append(conversion.Condition).Append(" ? (true, ").Append(conversion.Expression).Append(") : ");
        _ = conversion.AssignmentFallback is null
            ? builder.Append("(false, default(").Append(to).Append("))")
            : builder.Append("(true, ").Append(conversion.AssignmentFallback).Append(')');
        _ = builder.EndStatement();
    }

    /// <summary>Preserves assignability fallback for a registered converter that declines a value.</summary>
    /// <param name="builder">The writer, inside the converter branch.</param>
    /// <param name="to">The output type.</param>
    /// <param name="conversion">The generated conversion.</param>
    /// <param name="parameters">The explicit conversion inputs.</param>
    private static void AppendRejected(SourceWriter builder, string to, ConversionInfo conversion, ConversionParameters parameters)
    {
        if (conversion.AssignmentFallback is not null)
        {
            _ = builder.BeginIf().Append(parameters.Override).Append(" == null").CloseCondition()
                .BeginReturn().Append("(true, ").Append(conversion.AssignmentFallback).Line(");")
                .CloseBlock();
        }

        _ = builder.BeginReturn().Append("(false, default(").Append(to).Line("));");
    }
}
