// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

internal partial class InternalTestViewModel : ReactiveObject
{
    [Reactive]
    public partial int PublicPartialPropertyTest { get; set; }

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
    internal partial int InternalPartialPropertyTest { get; set; }

    [Reactive]
    protected internal partial int InternalProtectedPartialPropertyTest { get; set; }

    [Reactive]
    protected partial int ProtectedPartialPropertyTest { get; set; }

    [Reactive]
    private partial int PrivatePartialPropertyTest { get; set; }
}
