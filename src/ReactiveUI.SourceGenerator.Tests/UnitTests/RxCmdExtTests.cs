// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Extended unit tests for the ReactiveCommand generator covering edge cases.
/// </summary>
[TestFixture]
public class RxCmdExtTests : TestBase<ReactiveCommandGenerator>
{
    /// <summary>
    /// Tests ReactiveCommand with CanExecute observable property.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithCanExecute()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;
            using System.Reactive.Linq;
            using System.Threading.Tasks;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public IObservable<bool> CanDoWork => Observable.Return(true);

                [ReactiveCommand(CanExecute = nameof(CanDoWork))]
                private void DoWork() { }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with CancellationToken parameter.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithCancellationToken()
    {
        const string sourceCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private async Task LongRunningOperation(CancellationToken ct)
                {
                    await Task.Delay(1000, ct);
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with CancellationToken and parameter.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithCancellationTokenAndParameter()
    {
        const string sourceCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private async Task<string> ProcessWithCancellation(string input, CancellationToken ct)
                {
                    await Task.Delay(100, ct);
                    return input.ToUpper();
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand returning ValueTask.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithValueTask()
    {
        const string sourceCode = """
            using System;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private ValueTask DoValueTaskWork()
                {
                    return ValueTask.CompletedTask;
                }

                [ReactiveCommand]
                private ValueTask<int> GetValueTaskResult()
                {
                    return ValueTask.FromResult(42);
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with protected access modifier.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithProtectedAccess()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand(AccessModifier = PropertyAccessModifier.Protected)]
                private void ProtectedCommand() { }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with private access modifier.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithPrivateAccess()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand(AccessModifier = PropertyAccessModifier.Private)]
                private void PrivateCommand() { }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with complex generic return type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithComplexGenericReturnType()
    {
        const string sourceCode = """
            using System;
            using System.Collections.Generic;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private Dictionary<string, List<int>> GetComplexData()
                {
                    return new Dictionary<string, List<int>>();
                }

                [ReactiveCommand]
                private async Task<IReadOnlyList<string>> GetAsyncComplexData()
                {
                    await Task.Delay(10);
                    return new List<string> { "a", "b" };
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with tuple return type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithTupleReturnType()
    {
        const string sourceCode = """
            using System;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private (int Id, string Name) GetNamedTuple()
                {
                    return (1, "Test");
                }

                [ReactiveCommand]
                private async Task<(bool Success, string Message)> GetAsyncTuple()
                {
                    await Task.Delay(10);
                    return (true, "Done");
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with multiple parameters.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithMultipleParameters()
    {
        const string sourceCode = """
            using System;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private string CombineValues((int number, string text) input)
                {
                    return $"{input.text}: {input.number}";
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with nullable parameter.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithNullableParameter()
    {
        const string sourceCode = """
            using System;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private string? ProcessNullable(string? input)
                {
                    return input?.ToUpper();
                }

                [ReactiveCommand]
                private int ProcessNullableInt(int? input)
                {
                    return input ?? 0;
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand in generic class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandInGenericClass()
    {
        const string sourceCode = """
            using System;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class GenericVM<T> : ReactiveObject where T : class
            {
                [ReactiveCommand]
                private T? ProcessItem(T? item)
                {
                    return item;
                }

                [ReactiveCommand]
                private async Task<T?> ProcessItemAsync(T? item)
                {
                    await Task.Delay(10);
                    return item;
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with enum parameter.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithEnumParameter()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public enum Status { Pending, Active, Completed }

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private string GetStatusMessage(Status status)
                {
                    return status.ToString();
                }

                [ReactiveCommand]
                private void SetStatus(Status? status) { }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with array parameter.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithArrayParameter()
    {
        const string sourceCode = """
            using System;
            using System.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private int SumArray(int[] numbers)
                {
                    return numbers.Sum();
                }

                [ReactiveCommand]
                private string[] ProcessStrings(string[]? input)
                {
                    return input ?? Array.Empty<string>();
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with IObservable return.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithObservableReturn()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private IObservable<int> GetObservableSequence()
                {
                    return Observable.Range(1, 10);
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with both CanExecute and OutputScheduler.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithCanExecuteAndScheduler()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public IObservable<bool> CanExecuteCommand => Observable.Return(true);

                [ReactiveCommand(CanExecute = nameof(CanExecuteCommand), OutputScheduler = "RxApp.MainThreadScheduler")]
                private async Task<string> ExecuteWithScheduler()
                {
                    await Task.Delay(100);
                    return "Done";
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests multiple ReactiveCommands in same class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromMultipleReactiveCommands()
    {
        const string sourceCode = """
            using System;
            using System.Reactive.Linq;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public IObservable<bool> CanExecuteSave => Observable.Return(true);
                public IObservable<bool> CanExecuteDelete => Observable.Return(false);

                [ReactiveCommand]
                private void Create() { }

                [ReactiveCommand(CanExecute = nameof(CanExecuteSave))]
                private async Task Save()
                {
                    await Task.Delay(100);
                }

                [ReactiveCommand(CanExecute = nameof(CanExecuteDelete))]
                private void Delete() { }

                [ReactiveCommand(AccessModifier = PropertyAccessModifier.Internal)]
                private int Calculate(int input) => input * 2;
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand in record class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandInRecordClass()
    {
        const string sourceCode = """
            using System;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial record TestVMRecord : ReactiveObject
            {
                [ReactiveCommand]
                private void DoSomething() { }

                [ReactiveCommand]
                private async Task<int> GetValueAsync()
                {
                    await Task.Delay(10);
                    return 42;
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with delegate parameter.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithDelegateParameter()
    {
        const string sourceCode = """
            using System;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private void ExecuteAction(Action? action)
                {
                    action?.Invoke();
                }

                [ReactiveCommand]
                private int ExecuteFunc(Func<int>? func)
                {
                    return func?.Invoke() ?? 0;
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with Task of nullable return.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandWithTaskOfNullableReturn()
    {
        const string sourceCode = """
            using System;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveCommand]
                private async Task<string?> GetNullableStringAsync()
                {
                    await Task.Delay(10);
                    return null;
                }

                [ReactiveCommand]
                private async Task<int?> GetNullableIntAsync()
                {
                    await Task.Delay(10);
                    return 42;
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests ReactiveCommand with deeply nested class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromReactiveCommandInDeeplyNestedClass()
    {
        const string sourceCode = """
            using System;
            using System.Threading.Tasks;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class Outer : ReactiveObject
            {
                [ReactiveCommand]
                private void OuterCommand() { }

                public partial class Middle : ReactiveObject
                {
                    [ReactiveCommand]
                    private async Task MiddleCommand()
                    {
                        await Task.Delay(10);
                    }

                    public partial class Inner : ReactiveObject
                    {
                        [ReactiveCommand]
                        private int InnerCommand() => 42;

                        public partial class Deepest : ReactiveObject
                        {
                            [ReactiveCommand]
                            private string DeepestCommand(string input) => input;
                        }
                    }
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }
}
