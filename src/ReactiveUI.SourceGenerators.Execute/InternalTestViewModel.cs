// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides reactive-property generation examples with varied access modifiers.</summary>
[ExcludeFromCodeCoverage]
public partial class InternalTestViewModel : ReactiveObject
{
    /// <summary>Represents the first sample collection item.</summary>
    private const int FirstItemValue = 1;

    /// <summary>Represents the second sample collection item.</summary>
    private const int SecondItemValue = 2;

    /// <summary>Represents the third sample collection item.</summary>
    private const int ThirdItemValue = 3;

    /// <summary>Stores the collection used by the reactive collection example.</summary>
    [ReactiveCollection]
    private ObservableCollection<int>? _publicObservableCollectionTest;

    /// <summary>Initializes a new instance of the <see cref="InternalTestViewModel"/> class.</summary>
    public InternalTestViewModel()
    {
        // observe property changes
        _ = Changed
            .Subscribe(x =>
            {
                // handle property changes
                if (x.PropertyName != nameof(PublicObservableCollectionTest))
                {
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"PublicObservableCollectionTest changed: {PublicObservableCollectionTest?.Count}");
            });
        PublicObservableCollectionTest = [];
        PublicObservableCollectionTest.Add(FirstItemValue);
        PublicObservableCollectionTest.Add(SecondItemValue);
        PublicObservableCollectionTest.Add(ThirdItemValue);
        PublicObservableCollectionTest = [];
        PublicObservableCollectionTest.Add(FirstItemValue);
        PublicObservableCollectionTest.Add(SecondItemValue);
        PublicObservableCollectionTest.Add(ThirdItemValue);
    }

    [Reactive]
    public partial int PublicPartialPropertyTest { get; set; }

    [Reactive]
    public required partial bool PublicRequiredPartialPropertyTest { get; set; }

    [Reactive]
    public partial int PublicPartialPropertyWithInternalProtectedTest { get; protected internal set; }

    [Reactive]
    public partial int PublicPartialPropertyWithPrivateProtectedTest { get; private protected set; }

    [Reactive]
    public partial int PublicPartialPropertyWithProtectedTest { get; protected set; }

    [Reactive]
    public partial int PublicPartialPropertyWithInternalTest { get; internal set; }

    [Reactive]
    public partial int PublicPartialPropertyWithPrivateTest { get; private set; }

    [Reactive]
    public partial string PublicPartialStringPropertyTest { get; set; } = "initial";

    [Reactive]
    internal partial int InternalPartialPropertyTest { get; set; }

    [Reactive]
    protected internal partial int InternalProtectedPartialPropertyTest { get; set; }

    [Reactive]
    protected partial int ProtectedPartialPropertyTest { get; set; }

    [Reactive]
    private partial int PrivatePartialPropertyTest { get; set; }
}
