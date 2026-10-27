// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Tests.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>
/// A runtime view-first binding whose view raises no change notification reads the view through links that emit
/// once on subscribe. The binding still sees one initial value per side.
/// </summary>
[NotInParallel]
public class PocoLinkUnsafeBindingTests
{
    /// <summary>The value the view model starts out holding.</summary>
    private const string ViewModelValue = "model";

    /// <summary>The value typed into the view.</summary>
    private const string EditedValue = "edited";

    /// <summary>Creating a two-way binding writes the view model's value to the view and never back.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindUnsafe_ViewWithoutNotification_DoesNotWriteBackOnCreate()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var viewModel = new CountingViewModel { Name = ViewModelValue };
        var view = new SilentView { ViewModel = viewModel };
        viewModel.NameWrites = 0;

        using var binding = view.BindUnsafe(viewModel, static vm => vm.Name, static v => v.Editor.IsNotNullString);
        var writesOnCreate = viewModel.NameWrites;
        var viewOnCreate = view.Editor.IsNotNullString;
        view.Editor.IsNotNullString = EditedValue;

        using (Assert.Multiple())
        {
            await Assert.That(writesOnCreate).IsEqualTo(0);
            await Assert.That(viewOnCreate).IsEqualTo(ViewModelValue);
            await Assert.That(viewModel.Name).IsEqualTo(EditedValue);
        }
    }

    /// <summary>A view model that counts the writes to its bound property.</summary>
    internal sealed class CountingViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the number of times <see cref="Name"/> has been written.</summary>
        public int NameWrites { get; set; }

        /// <summary>Gets or sets the bound name.</summary>
        public string Name
        {
            get;
            set
            {
                NameWrites++;
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Name)));
            }
        } = string.Empty;
    }

    /// <summary>A view that raises no change notification and holds a control that does.</summary>
    internal sealed class SilentView : IViewFor<CountingViewModel>
    {
        /// <inheritdoc/>
        public CountingViewModel? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (CountingViewModel?)value;
        }

        /// <summary>Gets the notifying control the binding writes to.</summary>
        public TestFixture Editor { get; } = new();
    }
}
