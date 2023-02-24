//
//  DefaultInstanceLifetime.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.Extensions.SharedBinding.Lifetime;

public class DefaultInstanceLifetime : IInstanceLifetime
{
    private readonly CancellationTokenSource _stoppingSource;
    private bool _stoppingHooked;

    public DefaultInstanceLifetime()
    {
        _stoppingSource = new CancellationTokenSource();
    }

    /// <inheritdoc/>
    public SharedInstanceInfo Info { get; }

    /// <inheritdoc />
    public CancellationToken InstanceStopping
    {
        get
        {
            _stoppingHooked = true;
            return _stoppingSource.Token;
        }
    }

    /// <inheritdoc />
    public Result RequestStop()
    {
        if (!_stoppingHooked)
        { // There is no way anything was hooked to InstanceStopping, thus
          // stop won't work for sure.
            return new CannotRequestStop()
        }

        try
        {
            _stoppingSource.Cancel();
            return Result.FromSuccess();
        }
        catch (TaskCanceledException)
        {
            return Result.FromSuccess();
        }
        catch (Exception e)
        {
            return e;
        }
    }
}