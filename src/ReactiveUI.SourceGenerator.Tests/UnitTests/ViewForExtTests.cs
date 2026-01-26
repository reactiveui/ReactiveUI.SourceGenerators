// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Extended unit tests for the IViewFor generator covering edge cases.
/// </summary>
[TestFixture]
public class ViewForExtTests : TestBase<IViewForGenerator>
{
    /// <summary>
    /// Tests IViewFor with LazySingleton registration type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task LazySingle()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IViewFor<TestViewModel>(RegistrationType = SplatRegistrationType.LazySingleton)]
            public partial class TestView : Window
            {
                public TestView() => ViewModel = new TestViewModel();
            }

            public partial class TestViewModel : ReactiveObject
            {
                public int TestProperty { get; set; }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with Constant registration type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Constant()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IViewFor<TestViewModel>(RegistrationType = SplatRegistrationType.Constant)]
            public partial class TestView : Window
            {
                public TestView() => ViewModel = new TestViewModel();
            }

            public partial class TestViewModel : ReactiveObject
            {
                public string? Name { get; set; }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with PerRequest registration type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task PerReq()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IViewFor<TestViewModel>(RegistrationType = SplatRegistrationType.PerRequest)]
            public partial class TestView : Window
            {
                public TestView() => ViewModel = new TestViewModel();
            }

            public partial class TestViewModel : ReactiveObject
            {
                public bool IsActive { get; set; }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with ViewModel registration.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromIViewForWithViewModelRegistration()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IViewFor<TestViewModel>(
                RegistrationType = SplatRegistrationType.LazySingleton,
                ViewModelRegistrationType = SplatRegistrationType.LazySingleton)]
            public partial class TestView : Window
            {
                public TestView() => ViewModel = new TestViewModel();
            }

            public partial class TestViewModel : ReactiveObject
            {
                public int Count { get; set; }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with nested ViewModel.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Nested()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class ParentViewModel : ReactiveObject
            {
                public string? ParentProperty { get; set; }

                public partial class ChildViewModel : ReactiveObject
                {
                    public string? ChildProperty { get; set; }
                }
            }

            [IViewFor<ParentViewModel.ChildViewModel>]
            public partial class ChildView : Window
            {
                public ChildView() => ViewModel = new ParentViewModel.ChildViewModel();
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with string-based ViewModel type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromIViewForWithStringViewModelType()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [IViewFor("TestNs.TestViewModel")]
            public partial class TestView : Window
            {
                public TestView() => ViewModel = new TestViewModel();
            }

            public partial class TestViewModel : ReactiveObject
            {
                public int TestProperty { get; set; }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with different namespaces for View and ViewModel.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task DiffNs()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace ViewModels;

            public partial class ProductViewModel : ReactiveObject
            {
                public string? ProductName { get; set; }
                public decimal Price { get; set; }
            }

            namespace Views;

            using ViewModels;

            [IViewFor<ProductViewModel>]
            public partial class ProductView : Window
            {
                public ProductView() => ViewModel = new ProductViewModel();
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests multiple IViewFor in same namespace.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromMultipleIViewForInSameNamespace()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class ViewModel1 : ReactiveObject
            {
                public int Property1 { get; set; }
            }

            public partial class ViewModel2 : ReactiveObject
            {
                public string? Property2 { get; set; }
            }

            public partial class ViewModel3 : ReactiveObject
            {
                public bool Property3 { get; set; }
            }

            [IViewFor<ViewModel1>]
            public partial class View1 : Window
            {
                public View1() => ViewModel = new ViewModel1();
            }

            [IViewFor<ViewModel2>]
            public partial class View2 : Window
            {
                public View2() => ViewModel = new ViewModel2();
            }

            [IViewFor<ViewModel3>(RegistrationType = SplatRegistrationType.LazySingleton)]
            public partial class View3 : Window
            {
                public View3() => ViewModel = new ViewModel3();
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with generic ViewModel.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Generic()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class GenericViewModel<T> : ReactiveObject where T : class
            {
                public T? Item { get; set; }
            }

            [IViewFor<GenericViewModel<string>>]
            public partial class StringItemView : Window
            {
                public StringItemView() => ViewModel = new GenericViewModel<string>();
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with record ViewModel.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Record()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial record RecordViewModel : ReactiveObject
            {
                public string? Name { get; set; }
                public int Age { get; set; }
            }

            [IViewFor<RecordViewModel>]
            public partial class RecordView : Window
            {
                public RecordView() => ViewModel = new RecordViewModel();
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with deeply nested View class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromIViewForWithNestedViewClass()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestViewModel : ReactiveObject
            {
                public string? Name { get; set; }
            }

            public partial class OuterContainer
            {
                public partial class MiddleContainer
                {
                    [IViewFor<TestViewModel>]
                    public partial class NestedView : Window
                    {
                        public NestedView() => ViewModel = new TestViewModel();
                    }
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with ViewModel having reactive properties.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromIViewForWithReactivePropertiesViewModel()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class ReactivePropertiesViewModel : ReactiveObject
            {
                [Reactive]
                private string? _firstName;

                [Reactive]
                private string? _lastName;

                [Reactive]
                private int _age;
            }

            [IViewFor<ReactivePropertiesViewModel>]
            public partial class ReactivePropertiesView : Window
            {
                public ReactivePropertiesView() => ViewModel = new ReactivePropertiesViewModel();
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with ViewModel having ReactiveCommands.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task FromIViewForWithReactiveCommandsViewModel()
    {
        const string sourceCode = """
            using System;
            using System.Threading.Tasks;
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class CommandsViewModel : ReactiveObject
            {
                [ReactiveCommand]
                private void DoSomething() { }

                [ReactiveCommand]
                private async Task SaveAsync()
                {
                    await Task.Delay(10);
                }
            }

            [IViewFor<CommandsViewModel>]
            public partial class CommandsView : Window
            {
                public CommandsView() => ViewModel = new CommandsViewModel();
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with all registration options.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task AllRegOpts()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class FullViewModel : ReactiveObject
            {
                public string? Title { get; set; }
            }

            [IViewFor<FullViewModel>(
                RegistrationType = SplatRegistrationType.Constant,
                ViewModelRegistrationType = SplatRegistrationType.PerRequest)]
            public partial class FullView : Window
            {
                public FullView() => ViewModel = new FullViewModel();
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with interface ViewModel type via string.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Interface()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public interface ITestViewModel
            {
                string? Name { get; set; }
            }

            public partial class TestViewModelImpl : ReactiveObject, ITestViewModel
            {
                public string? Name { get; set; }
            }

            [IViewFor("TestNs.ITestViewModel")]
            public partial class InterfaceView : Window
            {
                public InterfaceView() => ViewModel = new TestViewModelImpl();
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>
    /// Tests IViewFor with ViewModel from external namespace reference.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ExtNs()
    {
        const string sourceCode = """
            using System.Collections.ObjectModel;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace External.ViewModels
            {
                public partial class ExternalViewModel : ReactiveObject
                {
                    public string? ExternalProperty { get; set; }
                }
            }

            namespace App.Views
            {
                using External.ViewModels;

                [IViewFor<ExternalViewModel>]
                public partial class ExternalView : Window
                {
                    public ExternalView() => ViewModel = new ExternalViewModel();
                }
            }
            """;

        return TestHelper.TestPass(sourceCode);
    }
}
