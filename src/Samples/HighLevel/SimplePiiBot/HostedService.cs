//
//  HostedService.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NosSmooth.Core.Client;
using NosSmooth.Core.Extensions;
using NosSmooth.Data.NOSFiles;
using NosSmooth.LocalBinding;
using NosSmooth.Packets.Extensions;
using NosSmooth.Packets.Packets;

namespace SimplePiiBot;

/// <summary>
/// The simple pii bot hosted service to start the client.
/// </summary>
public class HostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IPacketTypesRepository _packetRepository;
    private readonly NostaleDataFilesManager _filesManager;
    private readonly NosBindingManager _bindingManager;
    private readonly ILogger<HostedService> _logger;
    private readonly IHostLifetime _lifetime;

    /// <summary>
    /// Initializes a new instance of the <see cref="HostedService"/> class.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <param name="packetRepository">The packet repository.</param>
    /// <param name="filesManager">The file manager.</param>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="lifetime">The lifetime.</param>
    public HostedService
    (
        IServiceProvider services,
        IPacketTypesRepository packetRepository,
        NostaleDataFilesManager filesManager,
        NosBindingManager bindingManager,
        ILogger<HostedService> logger,
        IHostLifetime lifetime
    )
    {
        _services = services;
        _packetRepository = packetRepository;
        _filesManager = filesManager;
        _bindingManager = bindingManager;
        _logger = logger;
        _lifetime = lifetime;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var packetResult = _packetRepository.AddDefaultPackets();
        if (!packetResult.IsSuccess)
        {
            _logger.LogResultError(packetResult);
            return;
        }

        var filesResult = _filesManager.Initialize();
        if (!filesResult.IsSuccess)
        {
            _logger.LogResultError(filesResult);
            return;
        }

        var bindingResult = _bindingManager.Initialize();
        if (!bindingResult.IsSuccess)
        {
            _logger.LogResultError(bindingResult);
            return;
        }

        var runResult = await _services.GetRequiredService<INostaleClient>().RunAsync(stoppingToken);
        if (!runResult.IsSuccess)
        {
            _logger.LogResultError(runResult);
            await _lifetime.StopAsync(default);
        }
    }
}