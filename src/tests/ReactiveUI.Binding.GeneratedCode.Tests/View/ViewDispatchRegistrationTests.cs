// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.View;

/// <summary>Runtime tests that the generated view dispatch registers itself without a binding running first.</summary>
public class ViewDispatchRegistrationTests
{
    /// <summary>Verifies the registered view locator resolves a generated view with no binding call made.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewLocator_ResolvesGeneratedView_WithoutABindingRunning()
    {
        var viewModel = new TodoViewModel();

        var view = ViewLocator.GetCurrent().ResolveView(viewModel);

        await Assert.That(view).IsTypeOf<TodoView>();
        await Assert.That(view!.ViewModel).IsSameReferenceAs(viewModel);
    }

    /// <summary>Verifies the views of this assembly and of a referenced assembly both resolve, each assembly having registered its own lookup.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultViewLocator_ResolvesViewsFromEveryAssemblyThatGeneratesADispatch()
    {
        var locator = new DefaultViewLocator();

        var referenced = locator.ResolveView(new TodoViewModel(), null);
        var own = locator.ResolveView(new NoteViewModel(), null);

        await Assert.That(referenced).IsTypeOf<TodoView>();
        await Assert.That(own).IsTypeOf<NoteView>();
    }

    /// <summary>Verifies the default view locator resolves a view whose base class is an open generic.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultViewLocator_ResolvesViewDerivedFromOpenGenericBase()
    {
        var viewModel = new TodoViewModel();

        var view = new DefaultViewLocator().ResolveView(viewModel, null);

        await Assert.That(view).IsTypeOf<TodoView>();
    }
}
