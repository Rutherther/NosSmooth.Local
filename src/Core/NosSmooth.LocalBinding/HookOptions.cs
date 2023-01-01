//
//  HookOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.LocalBinding;

/// <summary>
/// A configuration for hooking a function.
/// </summary>
/// <param name="Hook">Whether to hook the function.</param>
/// <param name="MemoryPattern">The memory pattern in hex. Use ?? for any bytes.</param>
/// <param name="Offset">The offset to find the function at.</param>
public record HookOptions(bool Hook, string MemoryPattern, int Offset);