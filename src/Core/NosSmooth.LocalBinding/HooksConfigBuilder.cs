//
//  HooksConfigBuilder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using NosSmooth.LocalBinding.Options;

namespace NosSmooth.LocalBinding;

/// <summary>
/// Provides user-friendly builder for configuring
/// the hooks.
/// </summary>
/// <remarks>
/// Use this by invoking .ConfigureHooks on IServiceCollection.
///
/// May change hook's pattern or offset as well as enable or disable
/// hooking altogether.
///
/// By default, networking (packet send, packet receive) hooking is enabled,
/// everything else is disabled.
///
/// The methods may be chained, you may call HookAll() and then start disabling
/// the ones you don't need.
/// </remarks>
public class HooksConfigBuilder
{
    private readonly HookOptionsBuilder _packetSendHook;
    private readonly HookOptionsBuilder _packetReceiveHook;
    private readonly HookOptionsBuilder _playerWalkHook;
    private readonly HookOptionsBuilder _petWalkHook;
    private readonly HookOptionsBuilder _entityFocusHook;
    private readonly HookOptionsBuilder _entityFollowHook;
    private readonly HookOptionsBuilder _entityUnfollowHook;
    private readonly HookOptionsBuilder _periodicHook;

    /// <summary>
    /// Initializes a new instance of the <see cref="HooksConfigBuilder"/> class.
    /// </summary>
    internal HooksConfigBuilder()
    {
        _playerWalkHook = new HookOptionsBuilder(new CharacterBindingOptions().WalkHook);
        _packetSendHook = new HookOptionsBuilder(new NetworkBindingOptions().PacketSendHook);
        _packetReceiveHook = new HookOptionsBuilder(new NetworkBindingOptions().PacketReceiveHook);
        _petWalkHook = new HookOptionsBuilder(new PetManagerBindingOptions().PetWalkHook);
        _entityFocusHook = new HookOptionsBuilder(new UnitManagerBindingOptions().EntityFocusHook);
        _entityFollowHook = new HookOptionsBuilder(new CharacterBindingOptions().EntityFollowHook);
        _entityUnfollowHook = new HookOptionsBuilder(new CharacterBindingOptions().EntityUnfollowHook);
        _periodicHook = new HookOptionsBuilder(new PeriodicBindingOptions().PeriodicHook);
    }

    /// <summary>
    /// Disable all hooks.
    /// </summary>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookNone()
    {
        _packetSendHook.Hook(false);
        _packetReceiveHook.Hook(false);
        _playerWalkHook.Hook(false);
        _petWalkHook.Hook(false);
        _entityFocusHook.Hook(false);
        _entityFollowHook.Hook(false);
        _entityUnfollowHook.Hook(false);
        _periodicHook.Hook(false);
        return this;
    }

    /// <summary>
    /// Enable all hooks.
    /// </summary>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookAll()
    {
        _packetSendHook.Hook(true);
        _packetReceiveHook.Hook(true);
        _playerWalkHook.Hook(true);
        _petWalkHook.Hook(true);
        _entityFocusHook.Hook(true);
        _entityFollowHook.Hook(true);
        _entityUnfollowHook.Hook(true);
        _periodicHook.Hook(true);
        return this;
    }

    /// <summary>
    /// Enable networking (packet send, packet receive) hooks.
    /// </summary>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookNetworking()
    {
        _packetSendHook.Hook(true);
        _packetReceiveHook.Hook(true);
        return this;
    }

    /// <summary>
    /// Configure periodic hook. Can be any periodic function in NosTale called on every frame.
    /// Enables the hook. In case you just want to change the pattern or offset
    /// and do not want to enable the hook, call .Hook(false) in the builder.
    /// </summary>
    /// <param name="configure">The configuring action.</param>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookPeriodic(Action<HookOptionsBuilder>? configure = default)
    {
        if (configure is not null)
        {
            _periodicHook.Hook();
            configure(_periodicHook);
        }
        else
        {
            _periodicHook.Hook(true);
        }

        return this;
    }

    /// <summary>
    /// Configure packet send hooks.
    /// Enables the hook. In case you just want to change the pattern or offset
    /// and do not want to enable the hook, call .Hook(false) in the builder.
    /// </summary>
    /// <param name="configure">The configuring action.</param>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookPacketSend(Action<HookOptionsBuilder>? configure = default)
    {
        if (configure is not null)
        {
            _packetSendHook.Hook();
            configure(_packetSendHook);
        }
        else
        {
            _packetSendHook.Hook(true);
        }

        return this;
    }

    /// <summary>
    /// Configure packet receive hooks.
    /// Enables the hook. In case you just want to change the pattern or offset
    /// and do not want to enable the hook, call .Hook(false) in the builder.
    /// </summary>
    /// <param name="configure">The configuring action.</param>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookPacketReceive(Action<HookOptionsBuilder>? configure = default)
    {
        if (configure is not null)
        {
            _packetReceiveHook.Hook();
            configure(_packetReceiveHook);
        }
        else
        {
            _packetReceiveHook.Hook(true);
        }

        return this;
    }

    /// <summary>
    /// Configure player walk hooks.
    /// Enables the hook. In case you just want to change the pattern or offset
    /// and do not want to enable the hook, call .Hook(false) in the builder.
    /// </summary>
    /// <param name="configure">The configuring action.</param>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookPlayerWalk(Action<HookOptionsBuilder>? configure = default)
    {
        if (configure is not null)
        {
            _playerWalkHook.Hook();
            configure(_playerWalkHook);
        }
        else
        {
            _playerWalkHook.Hook(true);
        }

        return this;
    }

    /// <summary>
    /// Configure pet walk hooks.
    /// Enables the hook. In case you just want to change the pattern or offset
    /// and do not want to enable the hook, call .Hook(false) in the builder.
    /// </summary>
    /// <param name="configure">The configuring action.</param>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookPetWalk(Action<HookOptionsBuilder>? configure = default)
    {
        if (configure is not null)
        {
            _petWalkHook.Hook();
            configure(_petWalkHook);
        }
        else
        {
            _petWalkHook.Hook(true);
        }

        return this;
    }

    /// <summary>
    /// Configure entity focus hooks.
    /// Enables the hook. In case you just want to change the pattern or offset
    /// and do not want to enable the hook, call .Hook(false) in the builder.
    /// </summary>
    /// <param name="configure">The configuring action.</param>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookEntityFocus(Action<HookOptionsBuilder>? configure = default)
    {
        if (configure is not null)
        {
            _entityFocusHook.Hook();
            configure(_entityFocusHook);
        }
        else
        {
            _entityFocusHook.Hook(true);
        }

        return this;
    }

    /// <summary>
    /// Configure entity follow hooks.
    /// Enables the hook. In case you just want to change the pattern or offset
    /// and do not want to enable the hook, call .Hook(false) in the builder.
    /// </summary>
    /// <param name="configure">The configuring action.</param>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookEntityFollow(Action<HookOptionsBuilder>? configure = default)
    {
        if (configure is not null)
        {
            _entityFollowHook.Hook();
            configure(_entityFollowHook);
        }
        else
        {
            _entityFollowHook.Hook(true);
        }

        return this;
    }

    /// <summary>
    /// Configure entity unfollow hooks.
    /// Enables the hook. In case you just want to change the pattern or offset
    /// and do not want to enable the hook, call .Hook(false) in the builder.
    /// </summary>
    /// <param name="configure">The configuring action.</param>
    /// <returns>This builder.</returns>
    public HooksConfigBuilder HookEntityUnfollow(Action<HookOptionsBuilder>? configure = default)
    {
        if (configure is not null)
        {
            _entityUnfollowHook.Hook();
            configure(_entityUnfollowHook);
        }
        else
        {
            _entityUnfollowHook.Hook(true);
        }

        return this;
    }

    /// <summary>
    /// Applies configurations to the given collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    internal void Apply(IServiceCollection serviceCollection)
    {
        serviceCollection.Configure<CharacterBindingOptions>
        (
            characterOptions =>
            {
                characterOptions.WalkHook = _playerWalkHook.Build();
                characterOptions.EntityFollowHook = _entityFollowHook.Build();
                characterOptions.EntityUnfollowHook = _entityUnfollowHook.Build();
            }
        );

        serviceCollection.Configure<NetworkBindingOptions>
        (
            characterOptions =>
            {
                characterOptions.PacketReceiveHook = _packetReceiveHook.Build();
                characterOptions.PacketSendHook = _packetSendHook.Build();
            }
        );

        serviceCollection.Configure<PetManagerBindingOptions>
        (
            characterOptions =>
            {
                characterOptions.PetWalkHook = _petWalkHook.Build();
            }
        );

        serviceCollection.Configure<UnitManagerBindingOptions>
        (
            characterOptions =>
            {
                characterOptions.EntityFocusHook = _entityFocusHook.Build();
            }
        );
    }
}