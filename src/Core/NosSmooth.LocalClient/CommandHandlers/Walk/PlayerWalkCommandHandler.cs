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
using NosSmooth.LocalBinding.Objects;
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

    private readonly PlayerManagerBinding _playerManagerBinding;
    private readonly INostaleClient _nostaleClient;
    private readonly WalkCommandHandlerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerWalkCommandHandler"/> class.
    /// </summary>
    /// <param name="playerManagerBinding">The character object binding.</param>
    /// <param name="nostaleClient">The nostale client.</param>
    /// <param name="options">The options.</param>
    public PlayerWalkCommandHandler
    (
        PlayerManagerBinding playerManagerBinding,
        INostaleClient nostaleClient,
        IOptions<WalkCommandHandlerOptions> options
    )
    {
        _options = options.Value;
        _playerManagerBinding = playerManagerBinding;
        _nostaleClient = nostaleClient;
    }

    /// <inheritdoc/>
    public async Task<Result> HandleCommand(PlayerWalkCommand command, CancellationToken ct = default)
    {
        var handler = new ControlCommandWalkHandler
        (
            _nostaleClient,
            (x, y) => _playerManagerBinding.Walk(x, y),
            _playerManagerBinding.PlayerManager,
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