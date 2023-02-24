//
//  SharedInstanceInfo.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace NosSmooth.Extensions.SharedBinding.Lifetime;

public record SharedInstanceInfo(string Name, string? Version, Assembly Assembly);
