//
//  AttackCommandHandler.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Core.Client;
using NosSmooth.Core.Commands;
using NosSmooth.Core.Commands.Attack;
using NosSmooth.Core.Commands.Control;
using NosSmooth.Core.Extensions;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;
using Remora.Results;

namespace NosSmooth.LocalClient.CommandHandlers.Attack;

/// <summary>
/// Handler of <see cref="AttackCommand"/>.
/// </summary>
public class AttackCommandHandler : ICommandHandler<AttackCommand>
{
    private readonly INostaleClient _nostaleClient;
    private readonly UnitManagerBinding _unitManagerBinding;
    private readonly SceneManager _sceneManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttackCommandHandler"/> class.
    /// </summary>
    /// <param name="nostaleClient">The NosTale client.</param>
    /// <param name="unitManagerBinding">The unit manager binding.</param>
    /// <param name="sceneManager">The scene manager.</param>
    public AttackCommandHandler
        (INostaleClient nostaleClient, UnitManagerBinding unitManagerBinding, SceneManager sceneManager)
    {
        _nostaleClient = nostaleClient;
        _unitManagerBinding = unitManagerBinding;
        _sceneManager = sceneManager;
    }

    /// <inheritdoc />
    public async Task<Result> HandleCommand(AttackCommand command, CancellationToken ct = default)
    {
        if (command.TargetId is not null)
        {
            var entityResult = _sceneManager.FindEntity(command.TargetId.Value);
            if (entityResult.IsDefined(out var entity))
            {
                _unitManagerBinding.FocusEntity(entity);
            }
        }

        ControlCancelReason? reason = null;
        var takeControlCommand = command.CreateTakeControl
        (
            "Attack",
            command.HandleAttackCallback,
            (r) =>
            {
                reason = r;
                return Task.FromResult(Result.FromSuccess());
            }
        );

        var result = await _nostaleClient.SendCommandAsync(takeControlCommand, ct);
        if (!result.IsSuccess)
        {
            return result;
        }

        if (reason is not null)
        {
            return new GenericError($"The command could not finish, because {reason}");
        }

        return Result.FromSuccess();
    }
}