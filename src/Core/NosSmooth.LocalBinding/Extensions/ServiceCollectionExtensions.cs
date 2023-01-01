//
//  ServiceCollectionExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;

namespace NosSmooth.LocalBinding.Extensions;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds bindings to Nostale objects along with <see cref="NosBindingManager"/> to initialize those.
    /// </summary>
    /// <remarks>
    /// Adds <see cref="PlayerManagerBinding"/> and <see cref="NetworkBinding"/>.
    /// You have to initialize these using <see cref="NosBindingManager"/>
    /// prior to requesting them from the provider, otherwise an exception
    /// will be thrown.
    /// </remarks>
    /// <param name="serviceCollection">The service collection.</param>
    /// <returns>The collection.</returns>
    public static IServiceCollection AddNostaleBindings(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<NosBindingManager>()
            .AddSingleton<NosBrowserManager>()
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().PlayerManager)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().SceneManager)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().PetManagerList)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().SceneManager)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().PetManagerList)
            .AddSingleton(p => p.GetRequiredService<NosBindingManager>().PlayerManager)
            .AddSingleton(p => p.GetRequiredService<NosBindingManager>().PetManager)
            .AddSingleton(p => p.GetRequiredService<NosBindingManager>().UnitManager)
            .AddSingleton(p => p.GetRequiredService<NosBindingManager>().Network);
    }

    /// <summary>
    /// Configures what functions to hook and allows the user to make pattern, offset changes.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="configure">Function for configuring the hook config.</param>
    /// <returns>The collection.</returns>
    public static IServiceCollection ConfigureHooks(this IServiceCollection serviceCollection, Action<HooksConfigBuilder> configure)
    {
        var builder = new HooksConfigBuilder();
        configure(builder);
        builder.Apply(serviceCollection);
        return serviceCollection;
    }
}