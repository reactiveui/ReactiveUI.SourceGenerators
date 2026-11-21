// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Checks the text a registration choice is written as.</summary>
public class ChainRegistrationEmitterTests
{
    /// <summary>The choice ends every line with a line feed alone, whatever platform generates it.</summary>
    /// <param name="isBeforeChange">Whether the registration observes before-change notifications.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task AppendChoiceOpen_EndsEveryLineWithLineFeedOnly(bool isBeforeChange)
    {
        const int generatedAffinity = 5;
        var output = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment();

        ChainRegistrationEmitter.AppendChoiceOpen(output, "__p1", segment, generatedAffinity, isBeforeChange);

        var text = output.ToString();
        await Assert.That(text).Contains('\n');
        await Assert.That(text).DoesNotContain('\r');
    }

    /// <summary>
    /// The arguments open the runtime's choice with everything it needs fixed at compile time, and stop before the
    /// generated observation so each caller can write its own.
    /// </summary>
    /// <param name="isBeforeChange">Whether the registration observes before-change notifications.</param>
    /// <param name="distinctUntilChanged">Whether a winning registration filters consecutive equal values.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false, false)]
    [Arguments(true, false)]
    [Arguments(false, true)]
    public async Task AppendChoiceArguments_OpensTheRuntimeChoice(bool isBeforeChange, bool distinctUntilChanged)
    {
        const string indent = "    ";
        const string source = "global::ReactiveUI.Binding.Observables.PluginObservationSource";
        const int generatedAffinity = 5;
        var segment = ModelFactory.CreatePropertyPathSegment();
        var sb = new SourceWriter();

        var returned = ChainRegistrationEmitter.AppendChoiceArguments(
            sb,
            "__source",
            segment,
            "global::System.Int32",
            new(generatedAffinity, isBeforeChange, distinctUntilChanged));

        string[] expected =
        [
            $"{source}.{(distinctUntilChanged ? "ChooseDistinct" : "Choose")}<{segment.DeclaringTypeFullName}, global::System.Int32>(",
            $"{indent}({segment.DeclaringTypeFullName})__source, \"{segment.PropertyName}\", 5, {(isBeforeChange ? "true" : "false")},",
            $"{indent}__o => __o.{segment.PropertyName},",
            $"{indent}() => __e => __e.{segment.PropertyName},",
            string.Empty,
        ];

        await Assert.That(returned).IsSameReferenceAs(sb);
        await Assert.That(sb.ToString()).IsEqualTo(string.Join("\n", expected));
    }
}
