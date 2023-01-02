﻿//
//  PetWalkCommandHandler.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using NosSmooth.Core.Client;
using NosSmooth.Core.Commands;
using NosSmooth.Core.Commands.Control;
using NosSmooth.Core.Commands.Walking;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;
using Remora.Results;

namespace NosSmooth.LocalClient.CommandHandlers.Walk;

/// <summary>
/// Handles <see cref="PetWalkCommand"/>.
/// </summary>
public class PetWalkCommandHandler : ICommandHandler<PetWalkCommand>
{
    /// <summary>
    /// Group that is used for <see cref="TakeControlCommand"/>.
    /// </summary>
    public const string PetWalkControlGroup = "PetWalk";

    private readonly IPetWalkHook _petWalkHook;
    private readonly PetManagerList _petManagerList;
    private readonly NosThreadSynchronizer _threadSynchronizer;
    private readonly UserActionDetector _userActionDetector;
    private readonly INostaleClient _nostaleClient;
    private readonly WalkCommandHandlerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PetWalkCommandHandler"/> class.
    /// </summary>
    /// <param name="petWalkHook">The pet walk hook.</param>
    /// <param name="petManagerList">The pet manager list.</param>
    /// <param name="threadSynchronizer">The thread synchronizer.</param>
    /// <param name="userActionDetector">The user action detector.</param>
    /// <param name="nostaleClient">The nostale client.</param>
    /// <param name="options">The options.</param>
    public PetWalkCommandHandler
    (
        IPetWalkHook petWalkHook,
        PetManagerList petManagerList,
        NosThreadSynchronizer threadSynchronizer,
        UserActionDetector userActionDetector,
        INostaleClient nostaleClient,
        IOptions<WalkCommandHandlerOptions> options
    )
    {
        _options = options.Value;
        _petWalkHook = petWalkHook;
        _petManagerList = petManagerList;
        _threadSynchronizer = threadSynchronizer;
        _userActionDetector = userActionDetector;
        _nostaleClient = nostaleClient;
    }

    /// <inheritdoc/>
    public async Task<Result> HandleCommand(PetWalkCommand command, CancellationToken ct = default)
    {
        if (_petManagerList.Length < command.PetSelector + 1)
        {
            return new NotFoundError("Could not find the pet using the given selector.");
        }
        var petManager = _petManagerList[command.PetSelector];

        var handler = new ControlCommandWalkHandler
        (
            _nostaleClient,
            async (x, y, ct) =>
                await _threadSynchronizer.SynchronizeAsync
                (
                    () => _userActionDetector.NotUserAction<Result<bool>>
                    (
                        () => _petWalkHook.WrapperFunction(petManager, (ushort)x, (ushort)y)
                    )
                ),
            petManager,
            _options
        );

        return await handler.HandleCommand
        (
            command.TargetX,
            command.TargetY,
            command.ReturnDistanceTolerance,
            command,
            PetWalkControlGroup + "_" + command.PetSelector,
            ct
        );
    }
}