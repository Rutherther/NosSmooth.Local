//
//  NetworkBindingOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Objects;

namespace NosSmooth.LocalBinding.Options;

/// <summary>
/// Options for <see cref="NetworkBinding"/>.
/// </summary>
public class NetworkBindingOptions
{
    /// <summary>
    /// Gets or sets the configuration for packet receive function hook.
    /// </summary>
    public HookOptions PacketReceiveHook { get; set; }
        = new HookOptions(true, "55 8B EC 83 C4 ?? 53 56 57 33 C9 89 4D ?? 89 4D ?? 89 55 ?? 8B D8 8B 45 ??", 0);

    /// <summary>
    /// Gets or sets the configuration for packet send function hook.
    /// </summary>
    public HookOptions PacketSendHook { get; set; }
        = new HookOptions(true, "53 56 8B F2 8B D8 EB 04", 0);

    /// <summary>
    /// Gets or sets the pattern to find the network object at.
    /// </summary>
    /// <remarks>
    /// The address of the object is "three pointers down" from address found on this pattern.
    /// </remarks>
    public string NetworkObjectPattern { get; set; }
        = "A1 ?? ?? ?? ?? 8B 00 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? A1 ?? ?? ?? ?? 8B 00 8B 40 40";
}