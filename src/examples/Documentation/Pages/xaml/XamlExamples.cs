// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Xaml;

/// <summary>
/// Shows bindings to controls a page names in XAML. MAUI and Avalonia declare those controls with a source generator,
/// whose output the binding generator does not see, so it reads the XAML to find them.
/// </summary>
public static class XamlExamples
{
    /// <summary>Binds a MAUI page's named entry, button and label to a view model.</summary>
    public static void BindMauiNamedControls()
    {
        SignInViewModel viewModel = new SignInViewModel { UserName = "ada" };
        MauiSignInPage page = new MauiSignInPage { ViewModel = viewModel };

        using IDisposable name = page.Bind(viewModel, x => x.UserName, v => v.UserNameEntry.Text);
        using IDisposable status = page.OneWayBind(viewModel, x => x.Status, v => v.StatusLabel.Text);
        using IDisposable signIn = page.BindCommand(viewModel, x => x.SignIn, v => v.SignInButton);

        page.UserNameEntry.Text = "grace";
        page.SignInButton.Command.Execute(null);

        Console.WriteLine(viewModel.UserName);
        Console.WriteLine(page.StatusLabel.Text);

        // Output:
        // grace
        // Signed in as grace
    }

    /// <summary>Binds an Avalonia view's named text box and text block to a view model.</summary>
    public static void BindAvaloniaNamedControls()
    {
        SignInViewModel viewModel = new SignInViewModel { UserName = "ada" };
        AvaloniaSignInView view = new AvaloniaSignInView { ViewModel = viewModel };

        using IDisposable name = view.Bind(viewModel, x => x.UserName, v => v.UserNameBox.Text);
        using IDisposable status = view.OneWayBind(viewModel, x => x.Status, v => v.StatusText.Text);

        Console.WriteLine(view.UserNameBox.Text);

        view.UserNameBox.Text = "linus";
        viewModel.SignIn.Execute(null);

        Console.WriteLine(view.StatusText.Text);

        // Output:
        // ada
        // Signed in as linus
    }
}
