//
//  HookManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using NosSmooth.LocalBinding.Options;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <inheritdoc />
internal class HookManager : IHookManager
{
    private readonly HookManagerOptions _options;
    private List<INostaleHook> _hooks;

    /// <summary>
    /// Initializes a new instance of the <see cref="HookManager"/> class.
    /// </summary>
    /// <param name="options">The hook manager options.</param>
    public HookManager(IOptions<HookManagerOptions> options)
    {
        _options = options.Value;
        _hooks = new List<INostaleHook>();
    }

    /// <inheritdoc/>
    public IPacketSendHook PacketSend => GetHook<IPacketSendHook>(IHookManager.PacketSendName);

    /// <inheritdoc/>
    public IPacketReceiveHook PacketReceive => GetHook<IPacketReceiveHook>(IHookManager.PacketReceiveName);

    /// <inheritdoc/>
    public IPlayerWalkHook PlayerWalk => GetHook<IPlayerWalkHook>(IHookManager.CharacterWalkName);

    /// <inheritdoc/>
    public IEntityFollowHook EntityFollow => GetHook<IEntityFollowHook>(IHookManager.EntityFollowName);

    /// <inheritdoc/>
    public IEntityUnfollowHook EntityUnfollow => GetHook<IEntityUnfollowHook>(IHookManager.EntityUnfollowName);

    /// <inheritdoc/>
    public IPetWalkHook PetWalk => GetHook<IPetWalkHook>(IHookManager.PetWalkName);

    /// <inheritdoc/>
    public IEntityFocusHook EntityFocus => GetHook<IEntityFocusHook>(IHookManager.EntityFocusName);

    /// <inheritdoc/>
    public IPeriodicHook Periodic => GetHook<IPeriodicHook>(IHookManager.PeriodicName);

    /// <inheritdoc/>
    public IReadOnlyList<INostaleHook> Hooks => _hooks.AsReadOnly();

    /// <inheritdoc/>
    public IResult Initialize(NosBindingManager bindingManager, NosBrowserManager browserManager)
    {
        return HandleResults
        (
            () => PeriodicHook.Create(bindingManager, _options.PeriodicHook).Map(MapHook),
            () => EntityFocusHook.Create(bindingManager, browserManager, _options.EntityFocusHook).Map(MapHook),
            () => EntityFollowHook.Create(bindingManager, browserManager, _options.EntityFollowHook).Map(MapHook),
            () => EntityUnfollowHook.Create(bindingManager, browserManager, _options.EntityUnfollowHook).Map(MapHook),
            () => PlayerWalkHook.Create(bindingManager, browserManager, _options.PlayerWalkHook).Map(MapHook),
            () => PetWalkHook.Create(bindingManager, _options.PetWalkHook).Map(MapHook),
            () => PacketSendHook.Create(bindingManager, browserManager, _options.PacketSendHook).Map(MapHook),
            () => PacketReceiveHook.Create(bindingManager, browserManager, _options.PacketReceiveHook).Map(MapHook)
        );
    }

    private INostaleHook MapHook<T>(T original)
        where T : INostaleHook
    {
        return original;
    }

    private IResult HandleResults(params Func<Result<INostaleHook>>[] functions)
    {
        List<IResult> errorResults = new List<IResult>();
        foreach (var func in functions)
        {
            try
            {
                var result = func();
                if (result.IsSuccess)
                {
                    _hooks.Add(result.Entity);
                }
                else
                {
                    errorResults.Add(result);
                }
            }
            catch (Exception e)
            {
                errorResults.Add((Result)e);
            }

        }

        return errorResults.Count switch
        {
            0 => Result.FromSuccess(),
            1 => errorResults[0],
            _ => (Result)new AggregateError(errorResults)
        };
    }

    /// <inheritdoc/>
    public void Enable(IEnumerable<string> names)
    {
        foreach (var hook in Hooks
            .Where(x => names.Contains(x.Name)))
        {
            hook.Enable();
        }
    }

    /// <inheritdoc/>
    public void Disable(IEnumerable<string> names)
    {
        foreach (var hook in Hooks
            .Where(x => names.Contains(x.Name)))
        {
            hook.Disable();
        }
    }

    /// <inheritdoc/>
    public void DisableAll()
    {
        foreach (var hook in Hooks)
        {
            hook.Disable();
        }
    }

    /// <inheritdoc/>
    public void EnableAll()
    {
        foreach (var hook in Hooks)
        {
            hook.Enable();
        }
    }

    private T GetHook<T>(string name)
        where T : INostaleHook
    {
        var hook = _hooks.FirstOrDefault(x => x.Name == name);

        if (hook is null || hook is not T typed)
        {
            throw new InvalidOperationException
                ($"Could not load hook {name}. Did you forget to call IHookManager.Initialize?");
        }

        return typed;
    }
}