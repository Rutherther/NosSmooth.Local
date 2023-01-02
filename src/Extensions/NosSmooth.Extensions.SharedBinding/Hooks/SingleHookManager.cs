//
//  SingleHookManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Options;
using Remora.Results;

namespace NosSmooth.Extensions.SharedBinding.Hooks;

/// <summary>
/// A hook manager for a single NosSmooth instance using shared data.
/// </summary>
public class SingleHookManager : IHookManager
{
    private readonly SharedHookManager _sharedHookManager;
    private readonly HookManagerOptions _options;
    private Dictionary<string, INostaleHook> _hooks;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleHookManager"/> class.
    /// </summary>
    /// <param name="sharedHookManager">The shared hook manager.</param>
    /// <param name="options">The hook options.</param>
    public SingleHookManager(SharedHookManager sharedHookManager, IOptions<HookManagerOptions> options)
    {
        _hooks = new Dictionary<string, INostaleHook>();
        _sharedHookManager = sharedHookManager;
        _options = options.Value;
    }

    /// <inheritdoc />
    public IPacketSendHook PacketSend => GetHook<IPacketSendHook>(IHookManager.PacketSendName);

    /// <inheritdoc />
    public IPacketReceiveHook PacketReceive => GetHook<IPacketReceiveHook>(IHookManager.PacketReceiveName);

    /// <inheritdoc />
    public IPlayerWalkHook PlayerWalk => GetHook<IPlayerWalkHook>(IHookManager.CharacterWalkName);

    /// <inheritdoc />
    public IEntityFollowHook EntityFollow => GetHook<IEntityFollowHook>(IHookManager.EntityFollowName);

    /// <inheritdoc />
    public IEntityUnfollowHook EntityUnfollow => GetHook<IEntityUnfollowHook>(IHookManager.EntityUnfollowName);

    /// <inheritdoc />
    public IPetWalkHook PetWalk => GetHook<IPetWalkHook>(IHookManager.PetWalkName);

    /// <inheritdoc />
    public IEntityFocusHook EntityFocus => GetHook<IEntityFocusHook>(IHookManager.EntityFocusName);

    /// <inheritdoc />
    public IPeriodicHook Periodic => GetHook<IPeriodicHook>(IHookManager.PeriodicName);

    /// <inheritdoc />
    public IReadOnlyList<INostaleHook> Hooks => _hooks.Values.ToList();

    /// <inheritdoc />
    public IResult Initialize(NosBindingManager bindingManager, NosBrowserManager browserManager)
    {
        var hooksResult = _sharedHookManager.InitializeInstance(bindingManager, browserManager, _options);
        if (!hooksResult.IsDefined(out var hooks))
        {
            return hooksResult;
        }

        _hooks = hooks;
        return Result.FromSuccess();
    }

    /// <inheritdoc />
    public void Enable(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            var hook = GetHook<INostaleHook>(name);
            hook.Enable();
        }
    }

    /// <inheritdoc />
    public void Disable(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            var hook = GetHook<INostaleHook>(name);
            hook.Disable();
        }
    }

    /// <inheritdoc />
    public void DisableAll()
    {
        foreach (var hook in _hooks.Values)
        {
            hook.Disable();
        }
    }

    /// <inheritdoc />
    public void EnableAll()
    {
        foreach (var hook in _hooks.Values)
        {
            hook.Enable();
        }
    }

    private T GetHook<T>(string name)
        where T : INostaleHook
    {
        if (!_hooks.ContainsKey(name) || _hooks[name] is not T typed)
        {
            throw new InvalidOperationException
                ($"Could not load hook {name}. Did you forget to call IHookManager.Initialize?");
        }

        return typed;
    }
}