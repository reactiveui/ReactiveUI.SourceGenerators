using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mocks.Noise__N__;

// Code no generator acts on, with the attributes an application carries anyway. Every corpus includes it, so each
// generator's cost of looking past code that is not its own is measured.

[Serializable]
[DataContract]
public partial class OrderDto
{
    [DataMember]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [DataMember]
    [JsonPropertyName("customer")]
    public string? Customer { get; set; }

    [Obsolete("Use Lines")]
    [JsonIgnore]
    public string? LegacyLines { get; set; }

    [Description("The order total")]
    public decimal Total { get; set; }

    [ExcludeFromCodeCoverage]
    public override string ToString() => Customer ?? string.Empty;
}

[Description("A plain service")]
public sealed class OrderService
{
    [Obsolete("Use SaveAsync")]
    public void Save(OrderDto order) => Validate(order);

    [ExcludeFromCodeCoverage]
    private static void Validate(OrderDto order) => ArgumentNullException.ThrowIfNull(order);
}

[DataContract]
public sealed record Address([property: DataMember] string Street, [property: DataMember] string City);

[Flags]
public enum OrderState
{
    None = 0,
    Placed = 1,
    Shipped = 2,
}

[Serializable]
public partial class Customer : INotifyPropertyChanged
{
    [NonSerialized]
    private int _version;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Browsable(false)]
    public int Version
    {
        get => _version;
        set
        {
            _version = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Version)));
        }
    }
}
