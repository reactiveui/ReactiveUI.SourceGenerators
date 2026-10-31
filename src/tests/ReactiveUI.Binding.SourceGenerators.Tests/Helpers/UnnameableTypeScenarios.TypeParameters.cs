// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Call sites built from the calling code's type parameters.</summary>
internal static partial class UnnameableTypeScenarios
{
    /// <summary>A WhenAnyValue call on a receiver typed by the calling method's type parameter.</summary>
    internal const string TypeParameterReceiver = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new HasA { A = 1 };
                var recorder = Watch(vm);
                vm.A = 3;
                return recorder.Values.ToArray();
            }

            private static Recorder<int> Watch<T>(T vm)
                where T : class, IHasA => Recorder.Watch(vm.WhenAnyValue(x => x.A));
        }

        public interface IHasA : INotifyPropertyChanged
        {
            int A { get; set; }
        }

        public sealed class HasA : IHasA
        {
            private int _a;

            public event PropertyChangedEventHandler? PropertyChanged;

            public int A
            {
                get => _a;
                set
                {
                    _a = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(A)));
                }
            }
        }
        """;

    /// <summary>
    /// A WhenAnyValue call on a receiver typed by a type parameter constrained to a class. The class constraint already
    /// makes the type parameter a reference type, so repeating the stub's own class constraint beside it is CS0450.
    /// </summary>
    internal const string TypeParameterClassConstrainedReceiver = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new DerivedViewModel { A = 1 };
                var recorder = Watch(vm);
                vm.A = 3;
                return recorder.Values.ToArray();
            }

            private static Recorder<int> Watch<T>(T vm)
                where T : ViewModelBase => Recorder.Watch(vm.WhenAnyValue(x => x.A));
        }

        public abstract class ViewModelBase : INotifyPropertyChanged
        {
            private int _a;

            public event PropertyChangedEventHandler? PropertyChanged;

            public int A
            {
                get => _a;
                set
                {
                    _a = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(A)));
                }
            }
        }

        public sealed class DerivedViewModel : ViewModelBase
        {
        }
        """;

    /// <summary>A WhenAnyValue call on a generic box whose value is typed by the calling method's type parameter.</summary>
    internal const string TypeParameterValue = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var box = new Box<int> { Value = 1 };
                var recorder = Watch(box);
                box.Value = 3;
                return recorder.Values.ToArray();
            }

            private static Recorder<T> Watch<T>(Box<T> box) => Recorder.Watch(box.WhenAnyValue(x => x.Value));
        }

        public sealed class Box<T> : INotifyPropertyChanged
        {
            private T _value = default!;

            public event PropertyChangedEventHandler? PropertyChanged;

            public T Value
            {
                get => _value;
                set
                {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }

            public string Name { get; set; } = string.Empty;
        }
        """;

    /// <summary>A WhenAnyValue call whose selector returns the calling method's type parameter.</summary>
    internal const string TypeParameterResult = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new Vm { A = 1 };
                var recorder = Project(vm, a => a * 2);
                vm.A = 3;
                return recorder.Values.ToArray();
            }

            private static Recorder<T> Project<T>(Vm vm, Func<int, T> map) => Recorder.Watch(vm.WhenAnyValue(x => x.A, a => map(a)));
        }
        """;

    /// <summary>
    /// A WhenAnyValue and a BindInteraction call in a generic view base class whose type parameter is constrained to a
    /// view model base class. The class constraint already makes the type parameter a reference type, so the generated
    /// code must not add the stubs' own class constraint beside it (CS0450).
    /// </summary>
    internal const string TypeParameterClassConstrainedView = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new ChildViewModel { Error = "a" };
                var view = new ChildView { ViewModel = vm };
                var errors = view.Wire();
                vm.Error = "b";
                _ = Recorder.Watch(vm.Confirm.Handle("question"));
                return new[] { errors.Values.Count, view.Handled };
            }
        }

        public class ViewModelBase : INotifyPropertyChanged
        {
            private string? _error;

            public event PropertyChangedEventHandler? PropertyChanged;

            public string? Error
            {
                get => _error;
                set
                {
                    _error = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
                }
            }

            public IInteraction<string, bool> Confirm { get; } = new Interaction<string, bool>();
        }

        public sealed class ChildViewModel : ViewModelBase
        {
        }

        public abstract class ViewBase<TViewModel> : IViewFor<TViewModel>
            where TViewModel : ViewModelBase
        {
            public TViewModel? ViewModel { get; set; }

            object? IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (TViewModel?)value;
            }

            public int Handled { get; private set; }

            public Recorder<string?> Wire()
            {
                var errors = Recorder.Watch(this.ViewModel!.WhenAnyValue(vm => vm.Error));
                _ = this.BindInteraction(this.ViewModel, vm => vm.Confirm, Handle);
                return errors;
            }

            private System.Threading.Tasks.Task Handle(IInteractionContext<string, bool> context)
            {
                Handled++;
                context.SetOutput(true);
                return System.Threading.Tasks.Task.CompletedTask;
            }
        }

        public sealed class ChildView : ViewBase<ChildViewModel>
        {
        }
        """;

    /// <summary>A OneWayBind call in a generic view base class, onto a view model typed by the class's type parameter.</summary>
    internal const string TypeParameterViewBase = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new HasA { A = 1 };
                var view = new HasAView { ViewModel = vm };
                using (view.BindA())
                {
                    vm.A = 3;
                }

                return new[] { view.Shown };
            }
        }

        public interface IHasA : INotifyPropertyChanged
        {
            int A { get; set; }
        }

        public sealed class HasA : IHasA
        {
            private int _a;

            public event PropertyChangedEventHandler? PropertyChanged;

            public int A
            {
                get => _a;
                set
                {
                    _a = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(A)));
                }
            }
        }

        public abstract class ViewBase<TViewModel> : INotifyPropertyChanged, IViewFor<TViewModel>
            where TViewModel : class, IHasA
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public TViewModel? ViewModel { get; set; }

            object? IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (TViewModel?)value;
            }

            public int Shown { get; set; }

            public IDisposable BindA() => this.OneWayBind(ViewModel, vm => vm.A, v => v.Shown);
        }

        public sealed class HasAView : ViewBase<HasA>
        {
        }
        """;

    /// <summary>A WhenAnyValue call on a box of arrays, whose type parameter only appears inside an array.</summary>
    internal const string TypeParameterInsideArray = """
        public static class Scenario
        {
            public static int[] Run() => new[] { Watch(new Box<int[]>()).Values.Count };

            private static Recorder<T[]> Watch<T>(Box<T[]> box) => Recorder.Watch(box.WhenAnyValue(x => x.Value));
        }

        public sealed class Box<T> : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public T Value { get; set; } = default!;
        }
        """;

    /// <summary>A WhenAnyValue call whose type parameter is constrained by another the call does not use.</summary>
    internal const string ConstraintNamesAnotherTypeParameter = """
        public static class Scenario
        {
            public static int[] Run() => new[] { Watch<Item, int>(new Box<Item>()).Values.Count };

            private static Recorder<T> Watch<T, TKey>(Box<T> box)
                where T : IKeyed<TKey> => Recorder.Watch(box.WhenAnyValue(x => x.Value));
        }

        public interface IKeyed<TKey>
        {
            TKey Key { get; }
        }

        public sealed class Item : IKeyed<int>
        {
            public int Key => 1;
        }

        public sealed class Box<T> : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public T Value { get; set; } = default!;
        }
        """;

    /// <summary>A WhenAnyValue call on a generic box, reading a property that does not carry the type parameter.</summary>
    internal const string UnmappableTypeParameter = """
        public static class Scenario
        {
            public static int[] Run() => new[] { Watch(new Box<int>()).Values.Count };

            private static Recorder<string> Watch<T>(Box<T> box) => Recorder.Watch(box.WhenAnyValue(x => x.Name));
        }

        public sealed class Box<T> : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public string Name { get; set; } = string.Empty;
        }
        """;

    /// <summary>
    /// One call to every generated API but ToProperty, made in a generic method between a view and a view model typed by
    /// its type parameters. Returns the same values as <see cref="HostedEveryApi"/> without the ToProperty value and the
    /// unhosted call.
    /// </summary>
    internal const string TypeParameterEveryApi = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var executed = 0;
                var ticks = new Subject<int>();
                var vm = new ViewModel { Age = 1, Name = "a", Ticks = ticks, Save = new RecordingCommand(() => executed++) };
                var view = new View { ViewModel = vm };
                var values = Bind<ViewModel, View>(vm, view, ticks);
                values[7] = executed;
                return values;
            }

            private static int[] Bind<TViewModel, TView>(TViewModel vm, TView view, Subject<int> ticks)
                where TViewModel : class, IViewModel
                where TView : class, IView<TViewModel>
            {
                var handled = 0;
                var pushes = new Subject<int>();
                var changed = Recorder.Watch(vm.WhenChanged(x => x.Age));
                var changing = Recorder.Watch(vm.WhenChanging(x => x.Age));
                var any = Recorder.Watch(vm.WhenAny(x => x.Age, change => change.Value));
                var anyObservable = Recorder.Watch(vm.WhenAnyObservable(x => x.Ticks));
                var bindings = new[]
                {
                    vm.BindOneWay(view, x => x.Age, v => v.AgeCopy),
                    vm.BindTwoWay(view, x => x.Name, v => v.NameCopy),
                    view.OneWayBind(vm, x => x.Age, v => v.AgeFromView),
                    view.Bind(vm, x => x.Name, v => v.NameFromView),
                    pushes.BindTo(vm, x => x.Pushed),
                    pushes.InvokeCommand(vm, x => x.Save),
                    view.BindInteraction(vm, x => x.Confirm, context =>
                    {
                        handled++;
                        context.SetOutput(true);
                        return System.Threading.Tasks.Task.CompletedTask;
                    }),
                    view.BindCommand(vm, x => x.Save, v => v.SaveButton),
                };

                vm.Age = 2;
                ticks.OnNext(5);
                pushes.OnNext(7);
                view.NameCopy = "b";
                view.SaveButton.PerformClick();
                _ = Recorder.Watch(vm.Confirm.Handle("question"));

                foreach (var binding in bindings)
                {
                    binding.Dispose();
                }

                return new[]
                {
                    changed.Values.Last(),
                    any.Values.Last(),
                    anyObservable.Values.Last(),
                    changing.Values.Count > 0 ? 1 : 0,
                    view.AgeCopy,
                    view.AgeFromView,
                    vm.Pushed,
                    0,
                    handled,
                    vm.Name == "b" && view.NameFromView == "b" ? 1 : 0,
                };
            }
        }

        public interface IViewModel : INotifyPropertyChanged, INotifyPropertyChanging
        {
            int Age { get; set; }

            string? Name { get; set; }

            IObservable<int>? Ticks { get; }

            ICommand? Save { get; }

            IInteraction<string, bool> Confirm { get; }

            int Pushed { get; set; }
        }

        public interface IView<TViewModel> : INotifyPropertyChanged, IViewFor<TViewModel>
            where TViewModel : class
        {
            int AgeCopy { get; set; }

            string? NameCopy { get; set; }

            int AgeFromView { get; set; }

            string? NameFromView { get; set; }

            int Pushed { get; set; }

            MyButton SaveButton { get; }
        }

        public sealed class ViewModel : IViewModel
        {
            private int _age;
            private string? _name;

            public event PropertyChangedEventHandler? PropertyChanged;

            public event PropertyChangingEventHandler? PropertyChanging;

            public int Age
            {
                get => _age;
                set
                {
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Age)));
                    _age = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Age)));
                }
            }

            public string? Name
            {
                get => _name;
                set
                {
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Name)));
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }

            public IObservable<int>? Ticks { get; set; }

            public ICommand? Save { get; set; }

            public IInteraction<string, bool> Confirm { get; } = new Interaction<string, bool>();

            public int Pushed { get; set; }
        }

        public sealed class View : IView<ViewModel>
        {
            private int _ageCopy;
            private string? _nameCopy;
            private int _ageFromView;
            private string? _nameFromView;
            private int _pushed;

            public event PropertyChangedEventHandler? PropertyChanged;

            public ViewModel? ViewModel { get; set; }

            object? IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (ViewModel?)value;
            }

            public int AgeCopy { get => _ageCopy; set => Set(ref _ageCopy, value); }

            public string? NameCopy { get => _nameCopy; set => Set(ref _nameCopy, value); }

            public int AgeFromView { get => _ageFromView; set => Set(ref _ageFromView, value); }

            public string? NameFromView { get => _nameFromView; set => Set(ref _nameFromView, value); }

            public int Pushed { get => _pushed; set => Set(ref _pushed, value); }

            public MyButton SaveButton { get; } = new MyButton();

            private void Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string name = "")
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
        """;
}
