// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>One property of a bound path, as a generated binding describes it to <see cref="BindingHooks"/>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[DebuggerDisplay("BindingHookProperty")]
public abstract class BindingHookProperty
{
    /// <summary>Initializes a new instance of the <see cref="BindingHookProperty"/> class.</summary>
    private protected BindingHookProperty()
    {
    }

    /// <summary>Describes a property through a typed reader and the expression that names it.</summary>
    /// <typeparam name="TOwner">The type that declares the property.</typeparam>
    /// <typeparam name="TValue">The property's type.</typeparam>
    /// <param name="read">Reads the property from the object that owns it.</param>
    /// <param name="expression">Builds the lambda that names the property.</param>
    /// <returns>The property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="read"/> or <paramref name="expression"/> is <see langword="null"/>.</exception>
    /// <remarks>The expression is built on request, so a binding no hook inspects builds none.</remarks>
    public static BindingHookProperty Create<TOwner, TValue>(
        Func<TOwner, TValue> read,
        Func<Expression<Func<TOwner, TValue>>> expression)
    {
        ArgumentExceptionHelper.ThrowIfNull(read);
        ArgumentExceptionHelper.ThrowIfNull(expression);

        return new Typed<TOwner, TValue>(read, expression);
    }

    /// <summary>Reads the property from its owner, describing the read as a change.</summary>
    /// <param name="owner">The object that owns the property.</param>
    /// <param name="value">The value read, which owns the next property of the path.</param>
    /// <returns>The owner, the expression naming the property and the value read.</returns>
    internal abstract IObservedChange<object, object> Read(object owner, out object? value);

    /// <summary>Reads the property from its owner and hands it to a visitor in its own types.</summary>
    /// <param name="owner">The object that owns the property.</param>
    /// <param name="visitor">The visitor that receives the property.</param>
    /// <param name="ownsNext">Whether the value read owns the next property of the path.</param>
    /// <returns>The value read when it owns the next property; otherwise <see langword="null"/>.</returns>
    internal abstract object? Visit(object owner, IBindingHookPropertyVisitor visitor, bool ownsNext);

    /// <summary>A property read through accessors typed as its owner and its value.</summary>
    /// <typeparam name="TOwner">The type that declares the property.</typeparam>
    /// <typeparam name="TValue">The property's type.</typeparam>
    /// <param name="read">Reads the property from the object that owns it.</param>
    /// <param name="expression">Builds the lambda that names the property.</param>
    private sealed class Typed<TOwner, TValue>(Func<TOwner, TValue> read, Func<Expression<Func<TOwner, TValue>>> expression) : BindingHookProperty
    {
        /// <inheritdoc/>
        internal override IObservedChange<object, object> Read(object owner, out object? value)
        {
            // A hook is handed every value as an object, so this is the one place a value type is boxed.
            value = read((TOwner)owner);
            return new ObservedChange<object, object>(owner, expression().Body, value!);
        }

        /// <inheritdoc/>
        internal override object? Visit(object owner, IBindingHookPropertyVisitor visitor, bool ownsNext)
        {
            var typedOwner = (TOwner)owner;
            var value = read(typedOwner);
            visitor.Visit(typedOwner, expression(), value);
            return ownsNext ? value : null;
        }
    }
}
