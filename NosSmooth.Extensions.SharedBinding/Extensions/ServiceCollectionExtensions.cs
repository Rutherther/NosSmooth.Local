//
//  ServiceCollectionExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NosSmooth.Data.NOSFiles;
using NosSmooth.LocalBinding;
using NosSmooth.PacketSerializer.Packets;

namespace NosSmooth.Extensions.SharedBinding.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Replaces <see cref="NosBindingManager"/>
    /// with shared equivalent. That allows for multiple programs injected inside NosTale.
    /// </summary>
    /// <param name="serviceCollection">The collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection ShareBinding(this IServiceCollection serviceCollection)
    {
        var original = serviceCollection
            .Last(x => x.ServiceType == typeof(NosBindingManager));

        return serviceCollection
            .Configure<SharedOptions>(o => o.BindingDescriptor = original)
            .Replace
                (ServiceDescriptor.Singleton<NosBindingManager>(p => SharedManager.Instance.GetNosBindingManager(p)));
    }

    /// <summary>
    /// Replaces <see cref="NostaleDataFilesManager"/>
    /// with shared equivalent. That allows for multiple programs injected inside NosTale.
    /// </summary>
    /// <param name="serviceCollection">The collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection ShareFileManager(this IServiceCollection serviceCollection)
    {
        var original = serviceCollection
            .Last(x => x.ServiceType == typeof(NostaleDataFilesManager));

        return serviceCollection
            .Configure<SharedOptions>(o => o.FileDescriptor = original)
            .Replace
                (ServiceDescriptor.Singleton<NostaleDataFilesManager>(p => SharedManager.Instance.GetFilesManager(p)));
    }

    /// <summary>
    /// Replaces <see cref="IPacketTypesRepository"/>
    /// with shared equivalent. That allows for multiple programs injected inside NosTale.
    /// </summary>
    /// <param name="serviceCollection">The collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection SharePacketRepository(this IServiceCollection serviceCollection)
    {
        var original = serviceCollection
            .Last(x => x.ServiceType == typeof(IPacketTypesRepository));

        return serviceCollection
            .Configure<SharedOptions>(o => o.PacketRepositoryDescriptor = original)
            .Replace
            (
                ServiceDescriptor.Singleton<IPacketTypesRepository>(p => SharedManager.Instance.GetPacketRepository(p))
            );
    }

    /// <summary>
    /// Replaces <see cref="NosBindingManager"/>, <see cref="NostaleDataFilesManager"/> and <see cref="IPacketTypesRepository"/>
    /// with their shared equvivalents. That allows for multiple programs injected inside NosTale.
    /// </summary>
    /// <param name="serviceCollection">The collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection ShareNosSmooth(this IServiceCollection serviceCollection)
    {
        if (serviceCollection.Any(x => x.ServiceType == typeof(NosBindingManager)))
        {
            serviceCollection.ShareBinding();
        }

        if (serviceCollection.Any(x => x.ServiceType == typeof(NostaleDataFilesManager)))
        {
            serviceCollection.ShareFileManager();
        }

        if (serviceCollection.Any(x => x.ServiceType == typeof(IPacketTypesRepository)))
        {
            serviceCollection.SharePacketRepository();
        }

        return serviceCollection;
    }
}