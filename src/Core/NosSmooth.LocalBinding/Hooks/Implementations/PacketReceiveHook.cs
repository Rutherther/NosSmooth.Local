//
//  PacketReceiveHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <summary>
/// A hook of NetworkManager.PacketSend.
/// </summary>
internal class PacketReceiveHook : CancelableNostaleHook<IPacketReceiveHook.PacketReceiveDelegate,
    IPacketReceiveHook.PacketReceiveWrapperDelegate, PacketEventArgs>, IPacketReceiveHook
{
    /// <summary>
    /// Create the packet send hook.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A packet send hook or an error.</returns>
    public static Result<PacketReceiveHook> Create
        (NosBindingManager bindingManager, NosBrowserManager browserManager, HookOptions<IPacketReceiveHook> options)
    {
        var hook = CreateHook
            (
                bindingManager,
                () => new PacketReceiveHook(browserManager.NetworkManager),
                (hook) => hook.Detour,
                options
            );

        return hook;
    }

    private NetworkManager _networkManager;

    private PacketReceiveHook(NetworkManager networkManager)
    {
        _networkManager = networkManager;
    }

    /// <inheritdoc />
    public override string Name => IHookManager.PacketReceiveName;

    /// <inheritdoc />
    public override IPacketReceiveHook.PacketReceiveWrapperDelegate WrapperFunction => (packetString) =>
    {
        var packetObject = _networkManager.GetAddressForPacketReceive();
        using var nostaleString = NostaleStringA.Create(_networkManager.Memory, packetString);
        OriginalFunction(packetObject, nostaleString.Get());
    };

    /// <inheritdoc />
    protected override IPacketReceiveHook.PacketReceiveDelegate WrapWithCalling(IPacketReceiveHook.PacketReceiveDelegate function)
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