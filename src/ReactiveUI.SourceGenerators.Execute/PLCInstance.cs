// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// Represents a programmable logic controller (PLC) instance within the application.
/// </summary>
public class PLCInstance
{
    /// <summary>
    /// Gets or sets the unique identifier for the PLC instance.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the name of the PLC instance.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the IP address of the PLC instance.
    /// </summary>
    public string IPAddress { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether the PLC instance is currently active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets the network port number used for the connection.
    /// </summary>
    public int Port { get; set; }
}
