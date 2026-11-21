// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>One side of a binding, as an <see cref="ITypedPropertyBindingHook"/> reads it: a root and the properties walked from it.</summary>
[DebuggerDisplay("BindingHookPath: {Count} properties")]
public sealed class BindingHookPath
{
    /// <summary>The object the path starts at, or null when the side has none yet.</summary>
    private readonly object? _root;

    /// <summary>The properties walked from the root, in order.</summary>
    private readonly BindingHookProperty[] _properties;

    /// <summary>Initializes a new instance of the <see cref="BindingHookPath"/> class.</summary>
    /// <param name="root">The object the path starts at.</param>
    /// <param name="properties">The properties walked from the root, in order.</param>
    internal BindingHookPath(object? root, BindingHookProperty[] properties)
    {
        _root = root;
        _properties = properties;
    }

    /// <summary>Gets the number of properties in the path.</summary>
    public int Count => _properties.Length;

    /// <summary>Reads each property of the path and hands it to a visitor in its own types.</summary>
    /// <param name="visitor">Receives each property reached, in path order.</param>
    /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is <see langword="null"/>.</exception>
    /// <remarks>The walk stops before a property whose owner is null.</remarks>
    public void Accept(IBindingHookPropertyVisitor visitor)
    {
        ArgumentExceptionHelper.ThrowIfNull(visitor);

        var owner = _root;
        for (var i = 0; i < _properties.Length && owner is not null; i++)
        {
            // Only a value that owns the next property is needed as an object, so a leaf value is never boxed.
            owner = _properties[i].Visit(owner, visitor, i < _properties.Length - 1);
        }
    }
}
