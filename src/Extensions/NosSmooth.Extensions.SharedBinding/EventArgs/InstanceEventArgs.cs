//
//  InstanceEventArgs.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Extensions.SharedBinding.Lifetime;

namespace NosSmooth.Extensions.SharedBinding.EventArgs;

/// <summary>
/// Arguments containing information about a shared instance.
/// </summary>
public class InstanceEventArgs : System.EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceEventArgs"/> class.
    /// </summary>
    /// <param name="instanceInfo">The new instance.</param>
    public InstanceEventArgs(SharedInstanceInfo instanceInfo)
    {
        InstanceInfo = instanceInfo;
    }

    /// <summary>
    /// Gets the information about the new instance.
    /// </summary>
    public SharedInstanceInfo InstanceInfo { get; }
}