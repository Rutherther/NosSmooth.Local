//
//  ServiceCollectionExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Hooks.Implementations;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Options;
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
    /// This adds <see cref="NosBindingManager"/>, <see cref="NosBrowserManager"/>,
    /// <see cref="IHookManager"/> and their siblings.
    /// You have to initialize the bindings using <see cref="NosBindingManager"/>
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
            .AddSingleton<NosThreadSynchronizer>()
            .AddSingleton<IHookManager, HookManager>()
            .AddSingleton<IPacketReceiveHook>(p => p.GetRequiredService<IHookManager>().PacketReceive)
            .AddSingleton<IPacketSendHook>(p => p.GetRequiredService<IHookManager>().PacketSend)
            .AddSingleton<IEntityFollowHook>(p => p.GetRequiredService<IHookManager>().EntityFollow)
            .AddSingleton<IEntityUnfollowHook>(p => p.GetRequiredService<IHookManager>().EntityUnfollow)
            .AddSingleton<IEntityFocusHook>(p => p.GetRequiredService<IHookManager>().EntityFocus)
            .AddSingleton<IPlayerWalkHook>(p => p.GetRequiredService<IHookManager>().PlayerWalk)
            .AddSingleton<IPetWalkHook>(p => p.GetRequiredService<IHookManager>().PetWalk)
            .AddSingleton<IPeriodicHook>(p => p.GetRequiredService<IHookManager>().Periodic)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().PlayerManager)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().SceneManager)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().PetManagerList)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().SceneManager)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().PetManagerList)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().PlayerManager)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().NetworkManager)
            .AddSingleton(p => p.GetRequiredService<NosBrowserManager>().UnitManager)
            .AddSingleton(p => p.GetRequiredService<IHookManager>().PacketReceive)
            .AddSingleton(p => p.GetRequiredService<IHookManager>().PacketSend)
            .AddSingleton(p => p.GetRequiredService<IHookManager>().PlayerWalk)
            .AddSingleton(p => p.GetRequiredService<IHookManager>().PetWalk)
            .AddSingleton(p => p.GetRequiredService<IHookManager>().EntityFocus)
            .AddSingleton(p => p.GetRequiredService<IHookManager>().EntityFollow)
            .AddSingleton(p => p.GetRequiredService<IHookManager>().EntityUnfollow)
            .AddSingleton(p => p.GetRequiredService<IHookManager>().Periodic);
    }

    /// <summary>
    /// Configures what functions to hook and allows the user to make pattern, offset changes.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="configure">Function for configuring the hook config.</param>
    /// <returns>The collection.</returns>
    public static IServiceCollection ConfigureHooks(this IServiceCollection serviceCollection, Action<HooksConfigBuilder> configure)
    {
        var builder = new HooksConfigBuilder(new HookManagerOptions());
        configure(builder);
        builder.Apply(serviceCollection);
        return serviceCollection;
    }
}