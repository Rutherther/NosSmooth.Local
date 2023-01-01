//
//  SharedManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NosSmooth.Data.NOSFiles;
using NosSmooth.LocalBinding;
using NosSmooth.PacketSerializer.Packets;

namespace NosSmooth.Extensions.SharedBinding;

/// <summary>
/// Manager for sharing <see cref="NosBindingManager"/>,
/// <see cref="NostaleDataFilesManager"/> and
/// <see cref="IPacketTypesRepository"/>.
/// </summary>
public class SharedManager
{
    private static SharedManager? _instance;
    private NosBindingManager? _bindingManager;
    private NostaleDataFilesManager? _filesManager;
    private IPacketTypesRepository? _packetRepository;

    /// <summary>
    /// A singleton instance.
    /// One per process.
    /// </summary>
    public static SharedManager Instance
    {
        get
        {
            if (_instance is null)
            {
                _instance = new SharedManager();
            }

            return _instance;
        }
    }

    /// <summary>
    /// Gets the shared nos binding manager.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <returns>The shared manager.</returns>
    public NosBindingManager GetNosBindingManager(IServiceProvider services)
    {
        if (_bindingManager is null)
        {
            _bindingManager = GetFromDescriptor<NosBindingManager>(services, o => o.BindingDescriptor);
        }

        return _bindingManager;

    }

    /// <summary>
    /// Gets the shared file manager.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <returns>The shared manager.</returns>
    public NostaleDataFilesManager GetFilesManager(IServiceProvider services)
    {
        if (_filesManager is null)
        {
            _filesManager = GetFromDescriptor<NostaleDataFilesManager>(services, o => o.FileDescriptor);
        }

        return _filesManager;

    }

    /// <summary>
    /// Gets the shared packet type repository.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <returns>The shared repository.</returns>
    public IPacketTypesRepository GetPacketRepository(IServiceProvider services)
    {
        if (_packetRepository is null)
        {
            _packetRepository = GetFromDescriptor<IPacketTypesRepository>(services, o => o.PacketRepositoryDescriptor);
        }

        return _packetRepository;

    }

    private T GetFromDescriptor<T>(IServiceProvider services, Func<SharedOptions, ServiceDescriptor?> getDescriptor)
    {
        var options = services.GetRequiredService<IOptions<SharedOptions>>();
        var descriptor = getDescriptor(options.Value);

        if (descriptor is null)
        {
            throw new InvalidOperationException
                ($"Could not find {typeof(T)} in the service provider when trying to make a shared instance.");
        }

        if (descriptor.ImplementationInstance is not null)
        {
            return (T)descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationFactory is not null)
        {
            return (T)descriptor.ImplementationFactory(services);
        }

        if (descriptor.ImplementationType is not null)
        {
            return (T)ActivatorUtilities.CreateInstance(services, descriptor.ImplementationType);
        }

        return ActivatorUtilities.CreateInstance<T>(services);
    }
}