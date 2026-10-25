// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Buckets call sites that share one generated overload.</summary>
internal static class SignatureGrouping
{
    /// <summary>Groups call sites by the key their overload signature writes, in the order each key is first seen.</summary>
    /// <typeparam name="TInvocation">The call-site model.</typeparam>
    /// <typeparam name="TGroup">The group model one overload is generated from.</typeparam>
    /// <param name="invocations">The detected call sites.</param>
    /// <param name="appendKey">Writes the parts of a call site that decide its overload's signature.</param>
    /// <param name="createGroup">Builds a group from its first call site and all of its call sites.</param>
    /// <returns>One group per distinct signature.</returns>
    internal static List<TGroup> Group<TInvocation, TGroup>(
        ImmutableArray<TInvocation> invocations,
        Action<PooledStringBuilder, TInvocation> appendKey,
        Func<TInvocation, TInvocation[], TGroup> createGroup)
    {
        var groupMap = new Dictionary<string, List<TInvocation>>(invocations.Length);
        var order = new List<List<TInvocation>>(invocations.Length);
        var keySb = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            appendKey(keySb.Clear(), inv);
            var key = keySb.ToString();

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = [];
                groupMap[key] = list;
                order.Add(list);
            }

            list.Add(inv);
        }

        keySb.Return();

        var result = new List<TGroup>(order.Count);
        for (var i = 0; i < order.Count; i++)
        {
            var members = order[i];
            result.Add(createGroup(members[0], [.. members]));
        }

        return result;
    }
}
