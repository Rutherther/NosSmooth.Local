//
//  NosThreadSynchronizer.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Options;
using Remora.Results;

namespace NosSmooth.LocalBinding;

/// <summary>
/// Synchronizes with NosTale thread using a periodic function.
/// </summary>
public class NosThreadSynchronizer
{
    private readonly PeriodicBinding _periodicBinding;
    private readonly NosThreadSynchronizerOptions _options;
    private readonly ConcurrentQueue<SyncOperation> _queuedOperations;

    /// <summary>
    /// Initializes a new instance of the <see cref="NosThreadSynchronizer"/> class.
    /// </summary>
    /// <param name="periodicBinding">The periodic function binding.</param>
    /// <param name="options">The options.</param>
    public NosThreadSynchronizer(PeriodicBinding periodicBinding, IOptions<NosThreadSynchronizerOptions> options)
    {
        _periodicBinding = periodicBinding;
        _options = options.Value;
        _queuedOperations = new ConcurrentQueue<SyncOperation>();
    }

    /// <summary>
    /// Start the synchronizer operation.
    /// </summary>
    public void StartSynchronizer()
    {
        _periodicBinding.Periodic += Periodic;
    }

    /// <summary>
    /// Stop the synchronizer operation.
    /// </summary>
    public void StopSynchronizer()
    {
        _periodicBinding.Periodic -= Periodic;
    }

    private void Periodic()
    {
        var tasks = _options.MaxTasksPerIteration;

        while (tasks-- > 0 && _queuedOperations.TryDequeue(out var operation))
        {
            ExecuteOperation(operation);
        }
    }

    private void ExecuteOperation(SyncOperation operation)
    {
        try
        {
            var result = operation.action();
            operation.Result = result;
        }
        catch (Exception e)
        {
            // TODO: log?
            operation.Result = e;
        }

        if (operation.CancellationTokenSource is not null)
        {
            try
            {
                operation.CancellationTokenSource.Cancel();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }

    /// <summary>
    /// Enqueue the given operation to execute on next frame.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void EnqueueOperation(Action action)
    {
        _queuedOperations.Enqueue
        (
            new SyncOperation
            (
                () =>
                {
                    action();
                    return Result.FromSuccess();
                },
                null
            )
        );
    }

    /// <summary>
    /// Synchronizes to NosTale thread, executes the given action and returns its result.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>The result of the action.</returns>
    public async Task<Result> SynchronizeAsync(Func<Result> action, CancellationToken ct = default)
    {
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var syncOperation = new SyncOperation(action, linkedSource);
        _queuedOperations.Enqueue(syncOperation);

        try
        {
            await Task.Delay(Timeout.Infinite, linkedSource.Token);
        }
        catch (OperationCanceledException)
        {
            if (ct.IsCancellationRequested)
            { // Throw in case the top token was cancelled.
                throw;
            }
        }
        catch (Exception e)
        {
            return new ExceptionError(e);
        }

        return syncOperation.Result ?? Result.FromSuccess();
    }

    private record SyncOperation(Func<Result> action, CancellationTokenSource? CancellationTokenSource)
    {
        public Result? Result { get; set; }
    }
}