// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides nested reactive command generation examples.</summary>
[ExcludeFromCodeCoverage]
public partial class TestViewModel3 : ReactiveObject
{
    /// <summary>Represents the default generated command result.</summary>
    private const int CommandResult = 10;

    /// <summary>Stores the first generated reactive property value.</summary>
    [Reactive]
    private float _testVM3Property;

    /// <summary>Stores the second generated reactive property value.</summary>
    [Reactive]
    private float _testVM3Property2;

    /// <summary>Returns the generated command result.</summary>
    /// <returns>The generated command result.</returns>
    [ReactiveCommand]
    private int Test1() => CommandResult + (int)_testVM3Property;

    /// <summary>Provides the first nested reactive command generation example.</summary>
    public partial class TestInnerClass1 : ReactiveObject
    {
        /// <summary>Stores the first nested reactive property value.</summary>
        [Reactive]
        private int _testInner1;

        /// <summary>Stores the second nested reactive property value.</summary>
        [Reactive]
        private int _testInner11;

        /// <summary>Returns the first nested generated command result.</summary>
        /// <returns>The first nested generated command result.</returns>
        [ReactiveCommand]
        private int TestI1() => CommandResult + _testInner1;
    }

    /// <summary>Provides the second nested reactive command generation example.</summary>
    public partial class TestInnerClass2 : ReactiveObject
    {
        /// <summary>Stores the first nested reactive property value.</summary>
        [Reactive]
        private int _testInner2;

        /// <summary>Stores the second nested reactive property value.</summary>
        [Reactive]
        private int _testInner22;

        /// <summary>Returns the second nested generated command result.</summary>
        /// <returns>The second nested generated command result.</returns>
        [ReactiveCommand]
        private int TestI2() => CommandResult + _testInner2;

        /// <summary>Provides the third nested reactive command generation example.</summary>
        /// <seealso cref="ReactiveUI.Reactive.ReactiveObject" />
        public partial class TestInnerClass3 : ReactiveObject
        {
            /// <summary>Stores the first deeply nested reactive property value.</summary>
            [Reactive]
            private int _testInner3;

            /// <summary>Stores the second deeply nested reactive property value.</summary>
            [Reactive]
            private int _testInner33;

            /// <summary>Returns the deeply nested generated command result.</summary>
            /// <returns>The deeply nested generated command result.</returns>
            [ReactiveCommand]
            private int TestI3() => CommandResult + _testInner3;
        }
    }
}
