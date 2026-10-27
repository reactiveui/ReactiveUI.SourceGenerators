// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;

namespace ReactiveUI.Binding.Wpf.Tests.XamlPage;

/// <summary>Covers binding the controls a WPF view names in XAML, whose fields the markup compiler declares.</summary>
public class NamedControlsViewTests
{
    /// <summary>The name the user types.</summary>
    private const string Typed = "grace";

    /// <summary>Bind and OneWayBind on named controls run through generated bindings.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NamedControls_BindThroughGeneratedBindings()
    {
        string? shown = null;
        string? written = null;
        var thread = new Thread(() =>
        {
            var viewModel = new NamedControlsViewModel { Name = "ada" };
            var view = new NamedControlsView { ViewModel = viewModel };
            var bindings = view.BindControls();

            view.NameBox.Text = Typed;
            written = viewModel.Name;
            shown = view.StatusText.Text;

            foreach (var binding in bindings)
            {
                binding.Dispose();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        await Assert.That(written).IsEqualTo(Typed);
        await Assert.That(shown).IsEqualTo(Typed);
    }
}
