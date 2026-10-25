// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Marks a partial, get-only property as backed by an <see cref="ObservableAsPropertyHelper{T}"/>. The source
/// generator writes the property's body and a helper field named <c>_{name}Helper</c>, which you assign with
/// <c>ToProperty</c>.
/// </summary>
/// <remarks>
/// <para>
/// The property has to be declared by you, as a partial property, so every generator in the build can see it. A
/// property only another generator writes is invisible to this one, so it could not be observed or bound.
/// </para>
/// <para>
/// The attribute is also accepted on a field, a method or an observable property, the forms ReactiveUI's older source
/// generator supported. Nothing is generated for those. An analyzer reports each one and offers a code fix that
/// rewrites it as a partial property. <see cref="PropertyName"/> and <see cref="Inheritance"/> exist for that rewrite.
/// </para>
/// </remarks>
/// <example>
/// <code language="csharp">
/// <![CDATA[
/// public partial class PersonViewModel : INotifyPropertyChanged
/// {
///     public PersonViewModel(IObservable<string> names) =>
///         _fullNameHelper = names.ToProperty(this, x => x.FullName);
///
///     public event PropertyChangedEventHandler? PropertyChanged;
///
///     [ObservableAsProperty]
///     public partial string FullName { get; }
/// }
/// ]]>
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[DebuggerDisplay("ObservableAsPropertyAttribute: ReadOnly = {ReadOnly}, UseProtected = {UseProtected}, InitialValue = {InitialValue}")]
public sealed class ObservableAsPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the property the code fix writes for a method or an observable property. When it is
    /// null, the name is the member's name followed by <c>Property</c>. A partial property keeps its own name.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the helper field is <see langword="readonly"/>, so it can only be
    /// assigned in a constructor. The default is <see langword="false"/>.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>Gets or sets a value indicating whether the helper field is <see langword="protected"/> rather than <see langword="private"/>.</summary>
    public bool UseProtected { get; set; }

    /// <summary>
    /// Gets or sets the value the property returns until its helper is assigned, as a C# expression such as
    /// <c>"1.5d"</c>. For a <see cref="string"/> property the text is the value itself, not an expression. When it is
    /// null, a non-nullable <see cref="string"/> property returns <see cref="string.Empty"/> and any other property
    /// returns its type's default.
    /// </summary>
    public string? InitialValue { get; set; }

    /// <summary>
    /// Gets or sets the modifier the code fix gives the property it writes for a field. A partial property carries its
    /// own modifiers.
    /// </summary>
    public ObservableAsPropertyInheritance Inheritance { get; set; }
}
