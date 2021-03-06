//
//  UnitManagerBindingOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Objects;

namespace NosSmooth.LocalBinding.Options;

/// <summary>
/// Options for <see cref="UnitManagerBinding"/>.
/// </summary>
public class UnitManagerBindingOptions
{
    /// <summary>
    /// Gets or sets the pattern to static address of unit manager.
    /// </summary>
    public string UnitManagerPattern { get; set; }
        = "A1 ?? ?? ?? ?? E8 ?? ?? ?? ?? 33 C0 5A 59 59 64 89 10 68 ?? ?? ?? ?? 8D 45 F0 BA";

    /// <summary>
    /// Gets or sets the pointer offsets from the unit manager static address.
    /// </summary>
    public int[] UnitManagerOffsets { get; set; }
        = { 1, 0 };

    /// <summary>
    /// Gets or sets the pattern to find the focus entity method at.
    /// </summary>
    public string FocusEntityPattern { get; set; }
        = "73 00 00 00 55 8b ec b9 05 00 00 00";

    /// <summary>
    /// Gets or sets whether to hook the Focus entity function.
    /// </summary>
    public bool HookFocusEntity { get; set; } = true;
}