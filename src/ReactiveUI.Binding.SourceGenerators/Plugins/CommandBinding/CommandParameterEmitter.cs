// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Keeps streamed parameters typed until ICommand requests its object-valued argument.</summary>
internal static class CommandParameterEmitter
{
    /// <summary>Determines whether a parameter stream is available in the generated worker.</summary>
    /// <param name="inv">The command binding.</param>
    /// <returns>True when the caller supplied a stream or property expression.</returns>
    internal static bool HasParameter(BindCommandInvocationInfo inv) =>
        inv.HasObservableParameter || inv is { HasExpressionParameter: true, ParameterPropertyPath: not null };

    /// <summary>Emits typed parameter storage, preserving null until the first value arrives.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The command binding.</param>
    internal static void EmitCapture(SourceWriter sb, BindCommandInvocationInfo inv)
    {
        var type = inv.ParameterTypeFullName ?? "object";
        _ = sb.Append(type).Append(" __latestParam = default(").Append(type).Line(");")
            .Lines($$"""
                        var __parameterGate = new object();
                        var __hasParameter = false;
                        var __argumentCached = false;
                        object __argument = null;
                        object __ReadParameter()
                        {
                            lock (__parameterGate)
                            {
                                if (!__hasParameter)
                                {
                                    return null;
                                }
                                if (!__argumentCached)
                                {
                                    __argument = __latestParam;
                                    __argumentCached = true;
                                }
                                return __argument;
                            }
                        }
                        var __paramSub = {{GeneratedTypeNames.Subscribe}}(withParameter, __parameter =>
                        {
                            lock (__parameterGate)
                            {
                                __latestParam = __parameter;
                                __hasParameter = true;
                                __argumentCached = false;
                            }
                        });
                        """);
    }

    /// <summary>Returns the expression read at the ICommand object-parameter boundary.</summary>
    /// <param name="inv">The command binding.</param>
    /// <returns>The parameter reader or null.</returns>
    internal static string Read(BindCommandInvocationInfo inv) => HasParameter(inv) ? "__ReadParameter()" : "null";
}
