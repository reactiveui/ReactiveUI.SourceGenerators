// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
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
    /// The arguments hand the registration everything it needs fixed at compile time, in the order the observable's
    /// constructor takes them, and end on the timing so each caller can finish the call its own way.
    /// </summary>
    /// <param name="isBeforeChange">Whether the registration observes before-change notifications.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task AppendPluginObservableArguments_WritesTheLinkInConstructorOrder(bool isBeforeChange)
    {
        const string indent = "    ";
        var segment = ModelFactory.CreatePropertyPathSegment();
        var sb = new SourceWriter().Indent();

        var returned = ChainRegistrationEmitter.AppendPluginObservableArguments(
            sb,
            "__registration",
            "__source",
            segment,
            "global::System.Int32",
            isBeforeChange);

        string[] expected =
        [
            $"{indent}__registration,",
            $"{indent}__source,",
            $"{indent}((global::System.Linq.Expressions.Expression<global::System.Func<{segment.DeclaringTypeFullName}, global::System.Int32>>)(__e => __e.{segment.PropertyName})).Body,",
            $"{indent}\"{segment.PropertyName}\",",
            $"{indent}(object __o) => (({segment.DeclaringTypeFullName})__o).{segment.PropertyName},",
            $"{indent}{(isBeforeChange ? "true" : "false")}",
        ];

        await Assert.That(returned).IsSameReferenceAs(sb);
        await Assert.That(sb.ToString()).IsEqualTo(string.Join("\n", expected));
    }
}
