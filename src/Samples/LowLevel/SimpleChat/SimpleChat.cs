﻿//
//  SimpleChat.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NosSmooth.Core.Client;
using NosSmooth.Core.Extensions;
using NosSmooth.Extensions.SharedBinding.Extensions;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalClient.Extensions;
using NosSmooth.Packets.Enums;
using NosSmooth.Packets.Enums.Chat;
using NosSmooth.Packets.Enums.Entities;
using NosSmooth.Packets.Server.Chat;
using NosSmooth.PacketSerializer.Extensions;
using NosSmooth.PacketSerializer.Packets;

namespace SimpleChat;

/// <summary>
/// The main simple chat class.
/// </summary>
public class SimpleChat
{
    /// <summary>
    /// Run the client.
    /// </summary>
    /// <returns>The task that may or may not have succeeded.</returns>
    public async Task RunAsync()
    {
        var provider = new ServiceCollection()
            .AddLocalClient()
            .AddManagedNostaleCore()
            .ShareNosSmooth()
            .AddPacketResponder<SayResponder>()
            .AddLogging
            (
                b =>
                {
                    b.ClearProviders();
                    b.AddConsole();
                    b.SetMinimumLevel(LogLevel.Debug);
                }
            )
            .BuildServiceProvider();

        var logger = provider.GetRequiredService<ILogger<SimpleChat>>();
        logger.LogInformation("Hello world from SimpleChat!");

        var bindingManager = provider.GetRequiredService<NosBindingManager>();
        var initializeResult = bindingManager.Initialize();
        if (!initializeResult.IsSuccess)
        {
            logger.LogError($"Could not initialize NosBindingManager.");
            logger.LogResultError(initializeResult);
        }
        
        if (!bindingManager.IsModulePresent<IPeriodicHook>() || !bindingManager.IsModulePresent<IPacketSendHook>()
            || !bindingManager.IsModulePresent<IPacketReceiveHook>())
        {
            logger.LogError
            (
                "At least one of: periodic, packet receive, packet send has not been loaded correctly, the bot may not be used at all. Aborting"
            );
            return;
        }

        var packetTypesRepository = provider.GetRequiredService<IPacketTypesRepository>();
        var packetAddResult = packetTypesRepository.AddDefaultPackets();
        if (!packetAddResult.IsSuccess)
        {
            logger.LogError("Could not initialize default packet serializers correctly");
            logger.LogResultError(packetAddResult);
        }

        var client = provider.GetRequiredService<ManagedNostaleClient>();

        await client.ReceivePacketAsync
        (
            new SayPacket(EntityType.Map, 1, SayColor.Red, "Hello world from NosSmooth!")
        );

        await client.RunAsync();
    }
}