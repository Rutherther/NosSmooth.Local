//
//  EntityFollowHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Structs;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <summary>
/// A hook of NetworkManager.EntityFollow.
/// </summary>
internal class EntityFollowHook : CancelableNostaleHook<IEntityFollowHook.EntityFollowDelegate,
    IEntityFollowHook.EntityFollowWrapperDelegate, EntityEventArgs>, IEntityFollowHook
{
    /// <summary>
    /// Create the packet send hook.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A packet send hook or an error.</returns>
    public static Result<EntityFollowHook> Create
        (NosBindingManager bindingManager, NosBrowserManager browserManager, HookOptions<IEntityFollowHook> options)
    {
        var hook = CreateHook
        (
            bindingManager,
            () => new EntityFollowHook(browserManager.PlayerManager),
            hook => hook.Detour,
            options
        );

        return hook;
    }

    private readonly PlayerManager _playerManager;

    private EntityFollowHook(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }

    /// <inheritdoc />
    public override string Name => IHookManager.EntityFollowName;

    /// <inheritdoc />
    public override IEntityFollowHook.EntityFollowWrapperDelegate WrapperFunction => (entity) => OriginalFunction
        (_playerManager.Address, entity?.Address ?? 0);

    /// <inheritdoc />
    protected override IEntityFollowHook.EntityFollowDelegate WrapWithCalling(IEntityFollowHook.EntityFollowDelegate function)
        => (playerManagerPtr, entityPtr, un0, un1) =>
            {
                CallingFromNosSmooth = true;
                var res = function(playerManagerPtr, entityPtr, un0, un1);
                CallingFromNosSmooth = false;
                return res;
            };

    private nuint Detour
    (
        nuint playerManagerPtr,
        nuint entityPtr,
        int unknown1 = 0,
        int unknown2 = 1
    )
    {
        var entity = new MapBaseObj(_playerManager.Memory, entityPtr);
        var entityArgs = new EntityEventArgs(entity);
        return HandleCall(entityArgs);
    }
}