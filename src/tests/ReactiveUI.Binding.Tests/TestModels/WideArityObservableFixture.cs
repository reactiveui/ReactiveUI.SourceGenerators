// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>An object with enough observable properties to reach every wide-arity WhenAnyObservable overload.</summary>
public class WideArityObservableFixture : INotifyPropertyChanged
{
    /// <summary>How many streams the fixture carries, which is the widest arity these overloads reach.</summary>
    private const int StreamCount = 12;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets observed stream 1.</summary>
    public IObservable<string>? Stream1
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream1)));
        }
    }

    /// <summary>Gets or sets observed stream 2.</summary>
    public IObservable<string>? Stream2
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream2)));
        }
    }

    /// <summary>Gets or sets observed stream 3.</summary>
    public IObservable<string>? Stream3
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream3)));
        }
    }

    /// <summary>Gets or sets observed stream 4.</summary>
    public IObservable<string>? Stream4
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream4)));
        }
    }

    /// <summary>Gets or sets observed stream 5.</summary>
    public IObservable<string>? Stream5
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream5)));
        }
    }

    /// <summary>Gets or sets observed stream 6.</summary>
    public IObservable<string>? Stream6
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream6)));
        }
    }

    /// <summary>Gets or sets observed stream 7.</summary>
    public IObservable<string>? Stream7
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream7)));
        }
    }

    /// <summary>Gets or sets observed stream 8.</summary>
    public IObservable<string>? Stream8
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream8)));
        }
    }

    /// <summary>Gets or sets observed stream 9.</summary>
    public IObservable<string>? Stream9
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream9)));
        }
    }

    /// <summary>Gets or sets observed stream 10.</summary>
    public IObservable<string>? Stream10
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream10)));
        }
    }

    /// <summary>Gets or sets observed stream 11.</summary>
    public IObservable<string>? Stream11
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream11)));
        }
    }

    /// <summary>Gets or sets observed stream 12.</summary>
    public IObservable<string>? Stream12
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Stream12)));
        }
    }

    /// <summary>Fills every stream with a manually driven observable and hands them back in order.</summary>
    /// <returns>The observables the streams were filled with.</returns>
    internal ManualObservable<string>[] FillStreams()
    {
        var streams = new ManualObservable<string>[StreamCount];
        for (var i = 0; i < streams.Length; i++)
        {
            streams[i] = new();
        }

        Stream1 = streams[0];
        Stream2 = streams[1];
        Stream3 = streams[2];
        Stream4 = streams[3];
        Stream5 = streams[4];
        Stream6 = streams[5];
        Stream7 = streams[6];
        Stream8 = streams[7];
        Stream9 = streams[8];
        Stream10 = streams[9];
        Stream11 = streams[10];
        Stream12 = streams[11];

        return streams;
    }
}
