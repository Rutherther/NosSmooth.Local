//
//  SharedHookManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Extensions.SharedBinding.Hooks.Specific;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Options;
using Remora.Results;

namespace NosSmooth.Extensions.SharedBinding.Hooks;

/// <summary>
/// A hook manager managing <see cref="SingleHookManager"/>s of all of the instances.
/// </summary>
public class SharedHookManager
{
    private readonly IHookManager _underlyingManager;

    private bool _initialized;
    private Dictionary<string, int> _hookedCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="SharedHookManager"/> class.
    /// </summary>
    /// <param name="underlyingManager">The underlying hook manager.</param>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    public SharedHookManager
    (
        IHookManager underlyingManager
    )
    {
        _hookedCount = new Dictionary<string, int>();
        _underlyingManager = underlyingManager;
    }

    /// <summary>
    /// Initialize a shared NosSmooth instance.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="options">The initial options to be respected.</param>
    /// <returns>The dictionary containing all of the hooks.</returns>
    public Result<Dictionary<string, INostaleHook>> InitializeInstance
        (NosBindingManager bindingManager, NosBrowserManager browserManager, HookManagerOptions options)
    {
        if (!_initialized)
        {
            var result = _underlyingManager.Initialize(bindingManager, browserManager);
            _initialized = true;

            if (!result.IsSuccess)
            {
                return Result<Dictionary<string, INostaleHook>>.FromError(result.Error);
            }
        }

        var hooks = new Dictionary<string, INostaleHook>();

        // TODO: initialize using reflection
        hooks.Add
        (
            _underlyingManager.Periodic.Name,
            InitializeSingleHook
            (
                new PeriodicHook(_underlyingManager.Periodic),
                options.PeriodicHook
            )
        );

        hooks.Add
        (
            _underlyingManager.EntityFocus.Name,
            InitializeSingleHook
            (
                new EntityFocusHook(_underlyingManager.EntityFocus),
                options.EntityFocusHook
            )
        );

        hooks.Add
        (
            _underlyingManager.EntityFollow.Name,
            InitializeSingleHook
            (
                new EntityFollowHook(_underlyingManager.EntityFollow),
                options.EntityFollowHook
            )
        );

        hooks.Add
        (
            _underlyingManager.EntityUnfollow.Name,
            InitializeSingleHook
            (
                new EntityUnfollowHook(_underlyingManager.EntityUnfollow),
                options.EntityUnfollowHook
            )
        );

        hooks.Add
        (
            _underlyingManager.PacketReceive.Name,
            InitializeSingleHook
            (
                new PacketReceiveHook(_underlyingManager.PacketReceive),
                options.PacketReceiveHook
            )
        );

        hooks.Add
        (
            _underlyingManager.PacketSend.Name,
            InitializeSingleHook
            (
                new PacketSendHook(_underlyingManager.PacketSend),
                options.PacketSendHook
            )
        );

        hooks.Add
        (
            _underlyingManager.PetWalk.Name,
            InitializeSingleHook
            (
                new PetWalkHook(_underlyingManager.PetWalk),
                options.PetWalkHook
            )
        );

        hooks.Add
        (
            _underlyingManager.PlayerWalk.Name,
            InitializeSingleHook
            (
                new PlayerWalkHook(_underlyingManager.PlayerWalk),
                options.PlayerWalkHook
            )
        );

        return hooks;
    }

    private INostaleHook<TFunction, TWrapperFunction, TEventArgs> InitializeSingleHook<TFunction, TWrapperFunction,
        TEventArgs>(SingleHook<TFunction, TWrapperFunction, TEventArgs> hook, HookOptions options)
        where TFunction : Delegate
        where TWrapperFunction : Delegate
        where TEventArgs : System.EventArgs
    {
        hook.StateChanged += (_, state) =>
        {
            if (!_hookedCount.ContainsKey(hook.Name))
            {
                _hookedCount[hook.Name] = 0;
            }

            _hookedCount[hook.Name] += state.Enabled ? 1 : -1;

            if (state.Enabled)
            {
                _underlyingManager.Enable(new[] { hook.Name });
            }
            else if (_hookedCount[hook.Name] == 0)
            {
                _underlyingManager.Disable(new[] { hook.Name });
            }
        };

        if (options.Hook)
        {
            hook.Enable();
        }

        return hook;
    }
}