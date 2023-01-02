//
//  PacketSendHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <summary>
/// A hook of NetworkManager.PacketSend.
/// </summary>
internal class PacketSendHook : CancelableNostaleHook<IPacketSendHook.PacketSendDelegate,
    IPacketSendHook.PacketSendWrapperDelegate, PacketEventArgs>, IPacketSendHook
{
    /// <summary>
    /// Create the packet send hook.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A packet send hook or an error.</returns>
    public static Result<PacketSendHook> Create
        (NosBindingManager bindingManager, NosBrowserManager browserManager, HookOptions<IPacketSendHook> options)
    {
        var hook = CreateHook
            (
                bindingManager,
                () => new PacketSendHook(browserManager.NetworkManager),
                (sendHook) => sendHook.Detour,
                options
            );

        return hook;
    }

    private PacketSendHook(NetworkManager networkManager)
    {
        _networkManager = networkManager;
    }

    private readonly NetworkManager _networkManager;

    /// <inheritdoc />
    public override string Name => IHookManager.PacketSendName;

    /// <inheritdoc />
    public override IPacketSendHook.PacketSendWrapperDelegate WrapperFunction => (packetString) =>
    {
        var packetObject = _networkManager.GetAddressForPacketSend();
        using var nostaleString = NostaleStringA.Create(_networkManager.Memory, packetString);
        OriginalFunction(packetObject, nostaleString.Get());
    };

    /// <inheritdoc />
    protected override IPacketSendHook.PacketSendDelegate WrapWithCalling(IPacketSendHook.PacketSendDelegate function)
        => (packetObject, packetString) =>
        {
            CallingFromNosSmooth = true;
            var res = function(packetObject, packetString);
            CallingFromNosSmooth = false;
            return res;
        };

    private nuint Detour(nuint packetObject, nuint packetString)
    {
        var packet = Marshal.PtrToStringAnsi((IntPtr)packetString);
        if (packet is null)
        { // ?
            return 1;
        }

        var packetArgs = new PacketEventArgs(packet);
        return HandleCall(packetArgs);
    }
}