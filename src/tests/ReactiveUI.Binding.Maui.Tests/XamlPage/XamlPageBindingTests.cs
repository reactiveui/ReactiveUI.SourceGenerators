// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Maui.Tests.XamlPage;

/// <summary>
/// Covers binding the controls a MAUI page names in XAML. MAUI's own source generator declares them, which the binding
/// generator does not see, so it reads the page to find them.
/// </summary>
public class XamlPageBindingTests
{
    /// <summary>Bind, OneWayBind and BindCommand on named controls run through generated bindings.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NamedControls_BindThroughGeneratedBindings()
    {
        var viewModel = new LoginViewModel { UserName = "ada" };
        var page = new LoginPage { ViewModel = viewModel };

        var bindings = page.BindControls();
        try
        {
            await Assert.That(page.UserName.Text).IsEqualTo("ada");

            page.UserName.Text = "grace";
            await Assert.That(viewModel.UserName).IsEqualTo("grace");

            page.LoginButton.Command.Execute(null);
            await Assert.That(page.Status.Text).IsEqualTo("Welcome grace");
        }
        finally
        {
            foreach (var binding in bindings)
            {
                binding.Dispose();
            }
        }
    }
}
