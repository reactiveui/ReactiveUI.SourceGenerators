// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>The Splat registrations of an older to-do app, which kept its converters in a dependency resolver.</summary>
public static class LegacyAppDependencyResolver
{
    /// <summary>Registers the three converters the older app used and returns the resolver that holds them.</summary>
    /// <returns>A Splat resolver holding a typed, a fallback and a set-method converter.</returns>
    public static ModernDependencyResolver Create()
    {
        ModernDependencyResolver resolver = new();

        resolver.RegisterConstant<IBindingTypeConverter>(new LegacyPriorityToColorConverter());
        resolver.RegisterConstant<IBindingFallbackConverter>(new LegacyObjectToStringFallbackConverter());
        resolver.RegisterConstant<ISetMethodBindingConverter>(new LegacyTagListSetMethodConverter());

        return resolver;
    }
}
