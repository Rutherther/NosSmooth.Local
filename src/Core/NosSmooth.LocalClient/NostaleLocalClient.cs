//
//  NostaleLocalClient.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NosSmooth.Core.Client;
using NosSmooth.Core.Commands;
using NosSmooth.Core.Commands.Control;
using NosSmooth.Core.Extensions;
using NosSmooth.Core.Packets;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;
using NosSmooth.LocalClient.CommandHandlers.Walk;
using NosSmooth.Packets;
using NosSmooth.Packets.Errors;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using Remora.Results;

namespace NosSmooth.LocalClient;

/// <summary>
/// The local nostale client.
/// </summary>
/// <remarks>
/// Client used for living in the same process as NostaleClientX.exe.
/// It hooks the send and receive packet methods.
/// </remarks>
public class NostaleLocalClient : BaseNostaleClient
{
    private readonly NetworkBinding _networkBinding;
    private readonly PlayerManagerBinding _playerManagerBinding;
    private readonly PetManagerBinding _petManagerBinding;
    private readonly ControlCommands _controlCommands;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketHandler _packetHandler;
    private readonly UserActionDetector _userActionDetector;
    private readonly ILogger _logger;
    private readonly IServiceProvider _provider;
    private readonly LocalClientOptions _options;
    private CancellationToken? _stopRequested;
    private IPacketInterceptor? _interceptor;

    /// <summary>
    /// Initializes a new instance of the <see cref="NostaleLocalClient"/> class.
    /// </summary>
    /// <param name="networkBinding">The network binding.</param>
    /// <param name="playerManagerBinding">The player manager binding.</param>
    /// <param name="petManagerBinding">The pet manager binding.</param>
    /// <param name="controlCommands">The control commands.</param>
    /// <param name="commandProcessor">The command processor.</param>
    /// <param name="packetSerializer">The packet serializer.</param>
    /// <param name="packetHandler">The packet handler.</param>
    /// <param name="userActionDetector">The user action detector.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options for the client.</param>
    /// <param name="provider">The dependency injection provider.</param>
    public NostaleLocalClient
    (
        NetworkBinding networkBinding,
        PlayerManagerBinding playerManagerBinding,
        PetManagerBinding petManagerBinding,
        ControlCommands controlCommands,
        CommandProcessor commandProcessor,
        IPacketSerializer packetSerializer,
        IPacketHandler packetHandler,
        UserActionDetector userActionDetector,
        ILogger<NostaleLocalClient> logger,
        IOptions<LocalClientOptions> options,
        IServiceProvider provider
    )
        : base(commandProcessor, packetSerializer)
    {
        _options = options.Value;
        _networkBinding = networkBinding;
        _playerManagerBinding = playerManagerBinding;
        _petManagerBinding = petManagerBinding;
        _controlCommands = controlCommands;
        _packetSerializer = packetSerializer;
        _packetHandler = packetHandler;
        _userActionDetector = userActionDetector;
        _logger = logger;
        _provider = provider;
    }

    /// <inheritdoc />
    public override async Task<Result> RunAsync(CancellationToken stopRequested = default)
    {
        _stopRequested = stopRequested;
        _logger.LogInformation("Starting local client");
        _networkBinding.PacketSend += SendCallback;
        _networkBinding.PacketReceive += ReceiveCallback;

        _playerManagerBinding.FollowEntityCall += FollowEntity;
        _playerManagerBinding.WalkCall += Walk;
        _petManagerBinding.PetWalkCall += PetWalk;

        try
        {
            await Task.Delay(-1, stopRequested);
        }
        catch
        {
            // ignored
        }

        _networkBinding.PacketSend -= SendCallback;
        _networkBinding.PacketReceive -= ReceiveCallback;
        _playerManagerBinding.FollowEntityCall -= FollowEntity;
        _playerManagerBinding.WalkCall -= Walk;
        _petManagerBinding.PetWalkCall -= PetWalk;

        return Result.FromSuccess();
    }

    /// <inheritdoc />
    public override async Task<Result> ReceivePacketAsync(string packetString, CancellationToken ct = default)
    {
        ReceivePacket(packetString);
        await ProcessPacketAsync(PacketSource.Server, packetString);
        return Result.FromSuccess();
    }

    /// <inheritdoc />
    public override async Task<Result> SendPacketAsync(string packetString, CancellationToken ct = default)
    {
        SendPacket(packetString);
        await ProcessPacketAsync(PacketSource.Client, packetString);
        return Result.FromSuccess();
    }

    private bool ReceiveCallback(string packet)
    {
        bool accepted = true;
        if (_options.AllowIntercept)
        {
            if (_interceptor is null)
            {
                _interceptor = _provider.GetRequiredService<IPacketInterceptor>();
            }

            accepted = _interceptor.InterceptReceive(ref packet);
        }

        Task.Run(async () => await ProcessPacketAsync(PacketSource.Server, packet));

        return accepted;
    }

    private bool SendCallback(string packet)
    {
        bool accepted = true;
        if (_options.AllowIntercept)
        {
            if (_interceptor is null)
            {
                _interceptor = _provider.GetRequiredService<IPacketInterceptor>();
            }

            accepted = _interceptor.InterceptSend(ref packet);
        }

        Task.Run(async () => await ProcessPacketAsync(PacketSource.Client, packet));

        return accepted;
    }

    private void SendPacket(string packetString)
    {
        _networkBinding.SendPacket(packetString);
        _logger.LogDebug($"Sending client packet {packetString}");
    }

    private void ReceivePacket(string packetString)
    {
        _networkBinding.ReceivePacket(packetString);
        _logger.LogDebug($"Receiving client packet {packetString}");
    }

    private async Task ProcessPacketAsync(PacketSource type, string packetString)
    {
        try
        {
            var packetResult = _packetSerializer.Deserialize(packetString, type);
            IPacket packet;
            if (!packetResult.IsSuccess)
            {
                if (packetResult.Error is not PacketConverterNotFoundError)
                {
                    _logger.LogWarning("Could not parse {Packet}. Reason:", packetString);
                    _logger.LogResultError(packetResult);
                    packet = new ParsingFailedPacket(packetResult, packetString);
                }
                else
                {
                    packet = new UnresolvedPacket(packetString.Split(' ')[0], packetString);
                }
            }
            else
            {
                packet = packetResult.Entity;
            }

            Result result;
            if (type == PacketSource.Server)
            {
                result = await _packetHandler.HandleReceivedPacketAsync
                    (this, packet, packetString, _stopRequested ?? default);
            }
            else
            {
                result = await _packetHandler.HandleSentPacketAsync
                    (this, packet, packetString, _stopRequested ?? default);
            }

            if (!result.IsSuccess)
            {
                _logger.LogError("There was an error whilst handling packet");
                _logger.LogResultError(result);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "The process packet threw an exception");
        }
    }

    private bool FollowEntity(MapBaseObj? obj)
    {
        Task.Run
        (
            async () => await _controlCommands.CancelAsync
                (ControlCommandsFilter.UserCancellable, false, (CancellationToken)_stopRequested!)
        );
        return true;
    }

    private bool PetWalk(PetManager petManager, ushort x, ushort y)
    {
        if (!_userActionDetector.IsPetWalkUserOperation(petManager, x, y))
        { // do not cancel operations made by NosTale or bot
            return true;
        }

        if (_controlCommands.AllowUserActions)
        {
            Task.Run
            (
                async () => await _controlCommands.CancelAsync
                    (ControlCommandsFilter.UserCancellable, false, (CancellationToken)_stopRequested!)
            );
        }
        return _controlCommands.AllowUserActions;
    }

    private bool Walk(ushort x, ushort y)
    {
        if (!_userActionDetector.IsWalkUserAction(x, y))
        { // do not cancel operations made by NosTale or bot
            return true;
        }

        if (_controlCommands.AllowUserActions)
        {
            Task.Run
            (
                async () => await _controlCommands.CancelAsync
                    (ControlCommandsFilter.UserCancellable, false, (CancellationToken)_stopRequested!)
            );
        }
        return _controlCommands.AllowUserActions;
    }
}