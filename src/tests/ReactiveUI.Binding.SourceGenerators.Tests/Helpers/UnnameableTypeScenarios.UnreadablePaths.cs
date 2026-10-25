// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Call sites whose paths name a private member or something other than a property, or that are written so the generator cannot read them.</summary>
internal static partial class UnnameableTypeScenarios
{
    /// <summary>A WhenAnyValue call on a private property, made inside the partial class that declares it.</summary>
    internal const string HostedPrivateMember = """
        public sealed partial class Scenario : INotifyPropertyChanged
        {
            private int _hidden;

            public event PropertyChangedEventHandler? PropertyChanged;

            private int Hidden
            {
                get => _hidden;
                set
                {
                    _hidden = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hidden)));
                }
            }

            public static int[] Run()
            {
                var scenario = new Scenario { Hidden = 1 };
                var recorder = Recorder.Watch(scenario.WhenAnyValue(x => x.Hidden));
                scenario.Hidden = 3;
                return recorder.Values.ToArray();
            }
        }
        """;

    /// <summary>A WhenAnyValue call on a private property of a class that is not partial.</summary>
    internal const string UnhostablePrivateMember = """
        public sealed class Scenario : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            private int Hidden { get; set; }

            public static int[] Run()
            {
                var scenario = new Scenario { Hidden = 1 };
                var recorder = Recorder.Watch(scenario.WhenAnyValue(x => x.Hidden));
                return recorder.Values.ToArray();
            }
        }
        """;

    /// <summary>A WhenAnyValue call whose selector computes a value rather than naming a property.</summary>
    internal const string ComputedPath = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var recorder = Recorder.Watch(new Vm { A = 1 }.WhenAnyValue(x => x.A + 1));
                return recorder.Values.ToArray();
            }
        }
        """;

    /// <summary>A WhenChanged call written through the stub's declaring class.</summary>
    internal const string StaticFormCall = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var recorder = Recorder.Watch(ReactiveUIBindingExtensions.WhenChanged(new Vm { A = 1 }, x => x.A));
                return recorder.Values.ToArray();
            }
        }
        """;

    /// <summary>A WhenChanged call whose property path is held in a variable.</summary>
    internal const string StoredPath = """
        public static class Scenario
        {
            public static int[] Run()
            {
                System.Linq.Expressions.Expression<Func<Vm, int>> path = x => x.A;
                var recorder = Recorder.Watch(new Vm { A = 1 }.WhenChanged(path));
                return recorder.Values.ToArray();
            }
        }
        """;

    /// <summary>A BindCommand call whose command parameter path is held in a variable.</summary>
    internal const string StoredCommandParameter = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var executed = 0;
                var view = new View { ViewModel = new ViewModel { Save = new RecordingCommand(() => executed++) } };
                System.Linq.Expressions.Expression<Func<ViewModel, string?>> parameter = x => x.Name;
                using (view.BindCommand(view.ViewModel, vm => vm.Save, v => v.SaveButton, parameter))
                {
                    view.SaveButton.PerformClick();
                }

                return new[] { executed };
            }
        }

        public sealed class ViewModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public ICommand? Save { get; set; }

            public string? Name { get; set; }
        }

        public sealed class View : INotifyPropertyChanged, IViewFor<ViewModel>
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public ViewModel? ViewModel { get; set; }

            object? IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (ViewModel?)value;
            }

            public MyButton SaveButton { get; } = new MyButton();
        }
        """;
}
