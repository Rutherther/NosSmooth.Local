//
//  IHookManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// A manager holding all NosTale hooks with actions to execute on all of them.
/// </summary>
public interface IHookManager
{
    /// <summary>
    /// A name of packet send hook.
    /// </summary>
    public const string PacketSendName = "NetworkManager.PacketSend";

    /// <summary>
    /// A name of packet receive hook.
    /// </summary>
    public const string PacketReceiveName = "NetworkManager.PacketReceive";

    /// <summary>
    /// A name of character walk hook.
    /// </summary>
    public const string CharacterWalkName = "CharacterManager.Walk";

    /// <summary>
    /// A name of pet walk hook.
    /// </summary>
    public const string PetWalkName = "PetManager.Walk";

    /// <summary>
    /// A name of entity follow hook.
    /// </summary>
    public const string EntityFollowName = "CharacterManager.EntityFollow";

    /// <summary>
    /// A name of entity unfollow hook.
    /// </summary>
    public const string EntityUnfollowName = "CharacterManager.EntityUnfollow";

    /// <summary>
    /// A name of entity focus hook.
    /// </summary>
    public const string EntityFocusName = "UnitManager.EntityFocus";

    /// <summary>
    /// A name of periodic hook.
    /// </summary>
    public const string PeriodicName = "Periodic";

    /// <summary>
    /// Gets the packet send hook.
    /// </summary>
    public IPacketSendHook PacketSend { get; }

    /// <summary>
    /// Gets the packet receive hook.
    /// </summary>
    public IPacketReceiveHook PacketReceive { get; }

    /// <summary>
    /// Gets the player walk hook.
    /// </summary>
    public IPlayerWalkHook PlayerWalk { get; }

    /// <summary>
    /// Gets the entity follow hook.
    /// </summary>
    public IEntityFollowHook EntityFollow { get; }

    /// <summary>
    /// Gets the entity unfollow hook.
    /// </summary>
    public IEntityUnfollowHook EntityUnfollow { get; }

    /// <summary>
    /// Gets the player walk hook.
    /// </summary>
    public IPetWalkHook PetWalk { get; }

    /// <summary>
    /// Gets the entity focus hook.
    /// </summary>
    public IEntityFocusHook EntityFocus { get; }

    /// <summary>
    /// Gets the periodic function hook.
    /// </summary>
    /// <remarks>
    /// May be any function that is called periodically.
    /// This is used for synchronizing using <see cref="NosThreadSynchronizer"/>.
    /// </remarks>
    public IPeriodicHook Periodic { get; }

    /// <summary>
    /// Gets all of the hooks.
    /// </summary>
    public IReadOnlyList<INostaleHook> Hooks { get; }

    /// <summary>
    /// Initializes all hooks.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <returns>A result that may or may not have failed.</returns>
    public IResult Initialize(NosBindingManager bindingManager, NosBrowserManager browserManager);

    /// <summary>
    /// Enable hooks from the given list.
    /// </summary>
    /// <remarks>
    /// Use constants from <see cref="IHookManager"/>,
    /// such as IHookManager.PacketSendName.
    /// </remarks>
    /// <param name="names">The hooks to enable.</param>
    public void Enable(IEnumerable<string> names);

    /// <summary>
    /// Disable all hooks.
    /// </summary>
    public void DisableAll();

    /// <summary>
    /// Enable all hooks.
    /// </summary>
    public void EnableAll();
}