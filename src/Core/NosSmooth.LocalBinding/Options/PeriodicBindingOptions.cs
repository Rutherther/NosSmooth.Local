//
//  PeriodicBindingOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Objects;

namespace NosSmooth.LocalBinding.Options;

/// <summary>
/// Options for <see cref="PeriodicBinding"/>.
/// </summary>
public class PeriodicBindingOptions
{
    /// <summary>
    /// Gets or sets the configuration for any periodic function hook.
    /// </summary>
    public HookOptions PeriodicHook { get; set; }
        = new HookOptions(true, "55 8B EC 53 56 83 C4", 0);
}