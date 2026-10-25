// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI;

/// <summary>Stands in for ReactiveUI's raise extensions, found in the same assembly as the interface.</summary>
public static class IReactiveObjectExtensions
{
    /// <summary>Gets the notifications raised, as <c>changed:Name</c> or <c>changing:Name</c>, with the sender's type.</summary>
    public static List<string> Raised { get; } = [];

    /// <summary>The raise extensions.</summary>
    /// <typeparam name="TSender">The object type.</typeparam>
    /// <param name="reactiveObject">The object.</param>
    extension<TSender>(TSender reactiveObject)
        where TSender : IReactiveObject
    {
        /// <summary>Records an after-change notification.</summary>
        /// <param name="propertyName">The property name.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanged(string propertyName) =>
            Raised.Add($"changed:{propertyName}@{reactiveObject.GetType().Name}");

        /// <summary>Records a before-change notification.</summary>
        /// <param name="propertyName">The property name.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanging(string propertyName) =>
            Raised.Add($"changing:{propertyName}@{reactiveObject.GetType().Name}");
    }
}
