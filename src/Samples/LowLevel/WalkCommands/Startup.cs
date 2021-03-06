//
//  Startup.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NosSmooth.ChatCommands;
using NosSmooth.Core.Client;
using NosSmooth.Core.Extensions;
using NosSmooth.Data.NOSFiles;
using NosSmooth.Data.NOSFiles.Extensions;
using NosSmooth.Extensions.Pathfinding.Extensions;
using NosSmooth.LocalBinding;
using NosSmooth.LocalClient;
using NosSmooth.LocalClient.Extensions;
using NosSmooth.Packets.Extensions;
using NosSmooth.Packets.Packets;
using Remora.Commands.Extensions;
using WalkCommands.Commands;

namespace WalkCommands;

/// <summary>
/// Startup class of WalkCommands.
/// </summary>
public class Startup
{
    private IServiceProvider BuildServices()
    {
        var collection = new ServiceCollection()
            .AddLocalClient()
            .AddNostaleDataFiles()
            .AddNostalePathfinding()
            .AddScoped<Commands.WalkCommands>()
            .AddScoped<DetachCommand>()
            .AddSingleton<CancellationTokenSource>()
            .Configure<LocalClientOptions>(o => o.AllowIntercept = true)
            .AddNostaleChatCommands()
            .AddLogging
            (
                b =>
                {
                    b.ClearProviders();
                    b.AddConsole();
                    b.SetMinimumLevel(LogLevel.Debug);
                }
            );

        collection.AddCommandTree()
            .WithCommandGroup<DetachCommand>()
            .WithCommandGroup<CombatCommands>()
            .WithCommandGroup<Commands.WalkCommands>();
        return collection.BuildServiceProvider();
    }

    /// <summary>
    /// Run the MoveToMiniland.
    /// </summary>
    /// <returns>A task that may or may not have succeeded.</returns>
    public async Task RunAsync()
    {
        var provider = BuildServices();
        var bindingManager = provider.GetRequiredService<NosBindingManager>();
        var logger = provider.GetRequiredService<ILogger<Startup>>();
        var initializeResult = bindingManager.Initialize();
        if (!initializeResult.IsSuccess)
        {
            logger.LogError($"Could not initialize NosBindingManager.");
            logger.LogResultError(initializeResult);
        }

        var packetTypesRepository = provider.GetRequiredService<IPacketTypesRepository>();
        var packetAddResult = packetTypesRepository.AddDefaultPackets();
        if (!packetAddResult.IsSuccess)
        {
            logger.LogError("Could not initialize default packet serializers correctly");
            logger.LogResultError(packetAddResult);
        }

        var dataManager = provider.GetRequiredService<NostaleDataFilesManager>();
        var dataResult = dataManager.Initialize();
        if (!dataResult.IsSuccess)
        {
            logger.LogError("Could not initialize the nostale data files");
            logger.LogResultError(dataResult);
        }

        var mainCancellation = provider.GetRequiredService<CancellationTokenSource>();

        var client = provider.GetRequiredService<INostaleClient>();
        await client.RunAsync(mainCancellation.Token);
    }
}