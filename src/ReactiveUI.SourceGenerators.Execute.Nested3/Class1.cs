// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Execute.Nested3;

/// <summary>Provides the third nested reactive-command sample.</summary>
[ExcludeFromCodeCoverage]
public partial class Class1 : ReactiveObject
{
    /// <summary>Stores the third generated property value.</summary>
    [Reactive]
    private string? _property1;

    /// <summary>Initializes a new instance of the <see cref="Class1"/> class.</summary>
    public Class1()
    {
        _ = SetPropertyCommand.Execute(new Nested1.Class1 { Property1 = "Initial Value" }).Subscribe(new CommandObserver());
    }

    /// <summary>Copies the input object to the corresponding second nested object.</summary>
    /// <param name="class1">The object to copy.</param>
    /// <returns>The copied object, or <see langword="null"/> when the input is null.</returns>
    [ReactiveCommand]
    private static SGReactiveUI.SourceGenerators.Execute.Nested2.Class1? SetProperty(Nested1.Class1? class1) => class1 is null ? null : new() { Property1 = class1.Property1 };

    /// <summary>Observes results emitted by the generated command.</summary>
    private sealed class CommandObserver : IObserver<SGReactiveUI.SourceGenerators.Execute.Nested2.Class1?>
    {
        /// <summary>Handles successful command completion.</summary>
        public void OnCompleted()
        {
        }

        /// <summary>Propagates an error emitted by the generated command.</summary>
        /// <param name="error">The error to propagate.</param>
        public void OnError(Exception error) => throw error;

        /// <summary>Handles an object emitted by the generated command.</summary>
        /// <param name="value">The emitted object.</param>
        public void OnNext(SGReactiveUI.SourceGenerators.Execute.Nested2.Class1? value)
        {
        }
    }
}
