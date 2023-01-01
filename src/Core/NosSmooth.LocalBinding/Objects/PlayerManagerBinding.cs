//
//  PlayerManagerBinding.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Extensions;
using NosSmooth.LocalBinding.Options;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Remora.Results;

namespace NosSmooth.LocalBinding.Objects;

/// <summary>
/// The nostale binding of a character.
/// </summary>
public class PlayerManagerBinding
{
    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx, FunctionAttribute.Register.ecx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    private delegate bool WalkDelegate(nuint playerManagerPtr, int position, short unknown0 = 0, int unknown1 = 1);

    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx, FunctionAttribute.Register.ecx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    private delegate bool FollowEntityDelegate
    (
        nuint playerManagerPtr,
        nuint entityPtr,
        int unknown1 = 0,
        int unknown2 = 1
    );

    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    private delegate void UnfollowEntityDelegate(nuint playerManagerPtr, int unknown = 0);

    /// <summary>
    /// Create the network binding with finding the network object and functions.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="playerManager">The player manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A network binding or an error.</returns>
    public static Result<PlayerManagerBinding> Create(NosBindingManager bindingManager, PlayerManager playerManager, CharacterBindingOptions options)
    {
        var binding = new PlayerManagerBinding
        (
            bindingManager,
            playerManager
        );

        var walkHookResult = bindingManager.CreateHookFromPattern<WalkDelegate>
            ("CharacterBinding.Walk", binding.WalkDetour, options.WalkHook);
        if (!walkHookResult.IsDefined(out var walkHook))
        {
            return Result<PlayerManagerBinding>.FromError(walkHookResult);
        }

        var entityFollowHookResult = bindingManager.CreateHookFromPattern<FollowEntityDelegate>
            ("CharacterBinding.EntityFollow", binding.FollowEntityDetour, options.EntityFollowHook);
        if (!entityFollowHookResult.IsDefined(out var entityFollowHook))
        {
            return Result<PlayerManagerBinding>.FromError(entityFollowHookResult);
        }

        var entityUnfollowHookResult = bindingManager.CreateHookFromPattern<UnfollowEntityDelegate>
            ("CharacterBinding.EntityUnfollow", binding.UnfollowEntityDetour, options.EntityUnfollowHook);
        if (!entityUnfollowHookResult.IsDefined(out var entityUnfollowHook))
        {
            return Result<PlayerManagerBinding>.FromError(entityUnfollowHookResult);
        }

        binding._walkHook = walkHook;
        binding._followHook = entityFollowHook;
        binding._unfollowHook = entityUnfollowHook;
        return binding;
    }

    private readonly NosBindingManager _bindingManager;

    private IHook<WalkDelegate> _walkHook = null!;
    private IHook<FollowEntityDelegate> _followHook = null!;
    private IHook<UnfollowEntityDelegate> _unfollowHook = null!;

    private PlayerManagerBinding
    (
        NosBindingManager bindingManager,
        PlayerManager playerManager
    )
    {
        PlayerManager = playerManager;
        _bindingManager = bindingManager;
    }

    /// <summary>
    /// Gets the player manager.
    /// </summary>
    public PlayerManager PlayerManager { get; }

    /// <summary>
    /// Event that is called when walk was called by NosTale.
    /// </summary>
    /// <remarks>
    /// The walk must be hooked for this event to be called.
    /// </remarks>
    public event Func<ushort, ushort, bool>? WalkCall;

    /// <summary>
    /// Event that is called when entity follow or unfollow was called.
    /// </summary>
    /// <remarks>
    /// The follow/unfollow entity must be hooked for this event to be called.
    /// </remarks>
    public event Func<MapBaseObj?, bool>? FollowEntityCall;

    /// <summary>
    /// Disable all PlayerManager hooks.
    /// </summary>
    public void DisableHooks()
    {
        _followHook.Disable();
        _unfollowHook.Disable();
        _walkHook.Disable();
    }

    /// <summary>
    /// Enable all PlayerManager hooks.
    /// </summary>
    public void EnableHooks()
    {
        _followHook.EnableOrActivate();
        _unfollowHook.EnableOrActivate();
        _walkHook.EnableOrActivate();
    }

    /// <summary>
    /// Walk to the given position.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result<bool> Walk(short x, short y)
    {
        int param = ((ushort)y << 16) | (ushort)x;
        try
        {
            return _walkHook.OriginalFunction(PlayerManager.Address, param);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private bool WalkDetour(nuint characterObject, int position, short unknown0, int unknown1)
    {
        var result = WalkCall?.Invoke((ushort)(position & 0xFFFF), (ushort)((position >> 16) & 0xFFFF));
        if (result ?? true)
        {
            return _walkHook.OriginalFunction(characterObject, position, unknown0, unknown1);
        }

        return false;
    }

    /// <summary>
    /// Follow the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result FollowEntity(MapBaseObj? entity)
        => FollowEntity(entity?.Address ?? nuint.Zero);

    /// <summary>
    /// Follow the entity.
    /// </summary>
    /// <param name="entityAddress">The entity address.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result FollowEntity(nuint entityAddress)
    {
        try
        {
            _followHook.OriginalFunction(PlayerManager.Address, entityAddress);
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Stop following entity.
    /// </summary>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result UnfollowEntity()
    {
        try
        {
            _unfollowHook.OriginalFunction(PlayerManager.Address);
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }

    private bool FollowEntityDetour
    (
        nuint playerManagerPtr,
        nuint entityPtr,
        int unknown1,
        int unknown2
    )
    {
        var result = FollowEntityCall?.Invoke(new MapBaseObj(_bindingManager.Memory, entityPtr));
        if (result ?? true)
        {
            return _followHook.OriginalFunction(playerManagerPtr, entityPtr, unknown1, unknown2);
        }

        return false;
    }

    private void UnfollowEntityDetour(nuint playerManagerPtr, int unknown)
    {
        var result = FollowEntityCall?.Invoke(null);
        if (result ?? true)
        {
            _unfollowHook.OriginalFunction(playerManagerPtr, unknown);
        }
    }
}