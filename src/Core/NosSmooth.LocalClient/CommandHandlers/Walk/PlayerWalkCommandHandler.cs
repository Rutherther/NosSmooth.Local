//
//  PlayerWalkCommandHandler.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using NosSmooth.Core.Client;
using NosSmooth.Core.Commands;
using NosSmooth.Core.Commands.Control;
using NosSmooth.Core.Commands.Walking;
using NosSmooth.Core.Extensions;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;
using Remora.Results;

namespace NosSmooth.LocalClient.CommandHandlers.Walk;

/// <summary>
/// Handles <see cref="PlayerWalkCommand"/>.
/// </summary>
public class PlayerWalkCommandHandler : ICommandHandler<PlayerWalkCommand>
{
    /// <summary>
    /// Group that is used for <see cref="TakeControlCommand"/>.
    /// </summary>
    public const string PlayerWalkControlGroup = "PlayerWalk";

    private readonly PlayerManager _playerManager;
    private readonly IPlayerWalkHook _playerWalkHook;
    private readonly NosThreadSynchronizer _threadSynchronizer;
    private readonly UserActionDetector _userActionDetector;
    private readonly INostaleClient _nostaleClient;
    private readonly WalkCommandHandlerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerWalkCommandHandler"/> class.
    /// </summary>
    /// <param name="playerManager">The player manager.</param>
    /// <param name="playerWalkHook">The player walk hook.</param>
    /// <param name="threadSynchronizer">The thread synchronizer.</param>
    /// <param name="userActionDetector">The user action detector.</param>
    /// <param name="nostaleClient">The nostale client.</param>
    /// <param name="options">The options.</param>
    public PlayerWalkCommandHandler
    (
        PlayerManager playerManager,
        IPlayerWalkHook playerWalkHook,
        NosThreadSynchronizer threadSynchronizer,
        UserActionDetector userActionDetector,
        INostaleClient nostaleClient,
        IOptions<WalkCommandHandlerOptions> options
    )
    {
        _options = options.Value;
        _playerManager = playerManager;
        _playerWalkHook = playerWalkHook;
        _threadSynchronizer = threadSynchronizer;
        _userActionDetector = userActionDetector;
        _nostaleClient = nostaleClient;
    }

    /// <inheritdoc/>
    public async Task<Result> HandleCommand(PlayerWalkCommand command, CancellationToken ct = default)
    {
        var handler = new ControlCommandWalkHandler
        (
            _nostaleClient,
            async (x, y, ct)
                => await _threadSynchronizer.SynchronizeAsync
                (
                    () => _userActionDetector.NotUserWalk(_playerWalkHook, x, y),
                    ct
                ),
            _playerManager,
            _options
        );

        return await handler.HandleCommand
        (
            command.TargetX,
            command.TargetY,
            command.ReturnDistanceTolerance,
            command,
            PlayerWalkControlGroup,
            ct
        );
    }
}