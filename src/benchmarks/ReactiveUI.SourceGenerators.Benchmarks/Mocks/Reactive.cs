using System;
using System.Collections.Generic;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace Mocks.Reactive__N__;

/// <summary>A view model with field and partial-property reactive members.</summary>
public partial class PersonViewModel : ReactiveObject
{
    [Reactive]
    private string _firstName = string.Empty;

    [Reactive(nameof(FullName))]
    private string _lastName = string.Empty;

    [Reactive(SetModifier = AccessModifier.Protected)]
    private int _age;

    [Reactive(Inheritance = InheritanceModifier.Virtual)]
    private double? _height;

    [Reactive]
    private List<string>? _tags;

    [Reactive]
    private DateTimeOffset _updated;

    public string FullName => FirstName + " " + LastName;

    /// <summary>Gets or sets the nickname.</summary>
    [Reactive]
    public partial string? Nickname { get; set; }

    [Reactive]
    public partial int Score { get; protected set; }

    [Reactive("Summary")]
    public partial bool IsActive { get; set; }

    public string Summary => IsActive ? FullName : string.Empty;
}

public partial class AddressViewModel : ReactiveObject
{
    [Reactive]
    private string? _street;

    [Reactive]
    private string? _city;

    [Reactive]
    private string? _postCode;

    [Reactive]
    public partial string? Country { get; set; }

    public partial class Nested : ReactiveObject
    {
        [Reactive]
        private int _level;
    }
}

public partial class GenericViewModel<T> : ReactiveObject
    where T : class
{
    [Reactive]
    private T? _value;

    [Reactive]
    public partial IReadOnlyList<T>? Items { get; set; }
}
