// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.ComponentModel;
using ReactiveUI.Binding.Tests.Fallback;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>The runtime bindings accept a view model path that reads through an indexer.</summary>
[NotInParallel]
public class IndexerUnsafeBindingTests
{
    /// <summary>The first item in the view model's list.</summary>
    private const string FirstItem = "Foo";

    /// <summary>The item that replaces the first.</summary>
    private const string ReplacementItem = "Bar";

    /// <summary>A one-way binding through an indexer carries the indexed item.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBindUnsafe_IndexerPath_CarriesTheItem()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var viewModel = new IndexerViewModel();
        var view = new IndexerView { ViewModel = viewModel };

        using var binding = view.OneWayBindUnsafe(viewModel, static vm => vm.Items[0], static v => v.Caption);

        await Assert.That(view.Caption).IsEqualTo(FirstItem);
    }

    /// <summary>A one-way binding through an indexer follows a replaced item.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBindUnsafe_IndexerPath_FollowsTheReplacedItem()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var viewModel = new IndexerViewModel();
        var view = new IndexerView { ViewModel = viewModel };

        using var binding = view.OneWayBindUnsafe(viewModel, static vm => vm.Items[0], static v => v.Caption);
        viewModel.Items[0] = ReplacementItem;

        await Assert.That(view.Caption).IsEqualTo(ReplacementItem);
    }

    /// <summary>A one-way binding reads a property of the indexed item.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBindUnsafe_PropertyOfIndexedItem_CarriesTheProperty()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var viewModel = new IndexerViewModel();
        var view = new IndexerView { ViewModel = viewModel };

        using var binding = view.OneWayBindUnsafe(viewModel, static vm => vm.Items[0].Length, static v => v.Length);

        await Assert.That(view.Length).IsEqualTo(FirstItem.Length);
    }

    /// <summary>A view model exposing a list of items.</summary>
    internal sealed class IndexerViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { }
            remove { }
        }

        /// <summary>Gets the items.</summary>
        public ObservableCollection<string> Items { get; } = [FirstItem];
    }

    /// <summary>A view that exposes a typed view model.</summary>
    internal sealed class IndexerView : IViewFor<IndexerViewModel>, INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public IndexerViewModel? ViewModel
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(ViewModel)));
            }
        }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IndexerViewModel?)value;
        }

        /// <summary>Gets or sets the caption.</summary>
        public string? Caption
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Caption)));
            }
        }

        /// <summary>Gets or sets the length.</summary>
        public int Length
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Length)));
            }
        }
    }
}
