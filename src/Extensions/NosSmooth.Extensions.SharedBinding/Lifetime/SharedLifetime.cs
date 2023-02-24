//
//  SharedLifetime.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Extensions.SharedBinding.EventArgs;
using Remora.Results;

namespace NosSmooth.Extensions.SharedBinding.Lifetime;

/// <summary>
/// Events for shared lifetime.
/// </summary>
public class SharedLifetime
{
    /// <summary>
    /// A new instance has been attached.
    /// </summary>
    public event EventHandler<InstanceEventArgs>? InstanceAttached;

    /// <summary>
    /// A new instance has been attached and a conflict was detected,
    /// the same instance type is already attached.
    /// </summary>
    public event EventHandler<ConflictEventArgs>? ConflictDetected;

    /// <summary>
    /// Initialize a new shared instance.
    /// </summary>
    /// <param name="instanceInfo">The new instance.</param>
    /// <returns>A result, if errorful, the new instance should not start.</returns>
    public Result<CancellationTokenSource> Initialize(SharedInstanceInfo instanceInfo)
    {
        return Result<CancellationTokenSource>.FromSuccess(new CancellationTokenSource());
    }

    /// <summary>
    /// Initialize a new shared instance without its cooperation.
    /// In case the instance is not allowed, an exception will be thrown.
    /// </summary>
    /// <param name="instanceInfo">The shared instance.</param>
    /// <exception cref="Exception">Throws an exception in case the instance cannot be attached.</exception>
    public void ForceInitialize(SharedInstanceInfo instanceInfo)
    {
        var result = Initialize(instanceInfo);

        if (!result.IsSuccess)
        {
            throw new Exception($"Initialization not allowed! {result.Error.Message}");
        }
    }
}