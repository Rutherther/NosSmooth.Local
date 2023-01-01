//
//  CharacterBindingOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Objects;

namespace NosSmooth.LocalBinding.Options;

/// <summary>
/// Options for <see cref="PlayerManagerBinding"/>.
/// </summary>
public class CharacterBindingOptions
{
    /// <summary>
    /// Gets or sets the configuration for player walk function hook.
    /// </summary>
    public HookOptions WalkHook { get; set; } = new HookOptions(false, "55 8B EC 83 C4 EC 53 56 57 66 89 4D FA", 0);

    /// <summary>
    /// Gets or sets the configuration for entity follow function hook.
    /// </summary>
    public HookOptions EntityFollowHook { get; set; }
        = new HookOptions(false, "55 8B EC 51 53 56 57 88 4D FF 8B F2 8B F8", 0);

    /// <summary>
    /// Gets or sets the configuration for entity unfollow function hook.
    /// </summary>
    public HookOptions EntityUnfollowHook { get; set; }
        = new HookOptions(false, "80 78 14 00 74 1A", 0);
}