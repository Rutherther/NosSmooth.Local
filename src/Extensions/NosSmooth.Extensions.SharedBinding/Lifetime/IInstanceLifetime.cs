//
//  IInstanceLifetime.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.Extensions.SharedBinding.Lifetime;

public interface IInstanceLifetime
{
    public SharedInstanceInfo Info { get; }

    public CancellationToken InstanceStopping { get; }

    public CancellationToken InstanceStopped { get; }

    public Task<Result> RequestStop();
}