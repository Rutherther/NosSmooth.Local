//
//  NetworkBinding.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Options;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Remora.Results;

namespace NosSmooth.LocalBinding.Objects;

/// <summary>
/// The binding to nostale network object.
/// </summary>
public class NetworkBinding
{
    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    private delegate void PacketSendDelegate(nuint packetObject, nuint packetString);

    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    private delegate void PacketReceiveDelegate(nuint packetObject, nuint packetString);

    /// <summary>
    /// Create the network binding with finding the network object and functions.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A network binding or an error.</returns>
    public static Result<NetworkBinding> Create(NosBindingManager bindingManager, NetworkBindingOptions options)
    {
        var process = Process.GetCurrentProcess();
        var networkObjectAddress = bindingManager.Scanner.FindPattern(options.NetworkObjectPattern);
        if (!networkObjectAddress.Found)
        {
            return new BindingNotFoundError(options.NetworkObjectPattern, "NetworkBinding");
        }

        var binding = new NetworkBinding
        (
            bindingManager,
            (nuint)(networkObjectAddress.Offset + (int)process.MainModule!.BaseAddress + 0x01)
        );

        var sendHookResult = bindingManager.CreateHookFromPattern<PacketSendDelegate>
            ("NetworkBinding.SendPacket", binding.SendPacketDetour, options.PacketSendHook);
        if (!sendHookResult.IsDefined(out var sendHook))
        {
            return Result<NetworkBinding>.FromError(sendHookResult);
        }

        var receiveHookResult = bindingManager.CreateHookFromPattern<PacketReceiveDelegate>
            ("NetworkBinding.ReceivePacket", binding.ReceivePacketDetour, options.PacketReceiveHook);
        if (!receiveHookResult.IsDefined(out var receiveHook))
        {
            return Result<NetworkBinding>.FromError(receiveHookResult);
        }

        binding._sendHook = sendHook;
        binding._receiveHook = receiveHook;
        return binding;
    }

    private readonly NosBindingManager _bindingManager;
    private readonly nuint _networkManagerAddress;
    private IHook<PacketSendDelegate> _sendHook = null!;
    private IHook<PacketReceiveDelegate> _receiveHook = null!;

    private NetworkBinding
    (
        NosBindingManager bindingManager,
        nuint networkManagerAddress
    )
    {
        _bindingManager = bindingManager;
        _networkManagerAddress = networkManagerAddress;
    }

    /// <summary>
    /// Event that is called when packet send was called by NosTale.
    /// </summary>
    /// <remarks>
    /// The send must be hooked for this event to be called.
    /// </remarks>
    public event Func<string, bool>? PacketSend;

    /// <summary>
    /// Event that is called when packet receive was called by NosTale.
    /// </summary>
    /// <remarks>
    /// The receive must be hooked for this event to be called.
    /// </remarks>
    public event Func<string, bool>? PacketReceive;

    /// <summary>
    /// Send the given packet.
    /// </summary>
    /// <param name="packet">The packet to send.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result SendPacket(string packet)
    {
        try
        {
            using var nostaleString = NostaleStringA.Create(_bindingManager.Memory, packet);
            _sendHook.OriginalFunction(GetManagerAddress(false), nostaleString.Get());
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Receive the given packet.
    /// </summary>
    /// <param name="packet">The packet to receive.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result ReceivePacket(string packet)
    {
        try
        {
            using var nostaleString = NostaleStringA.Create(_bindingManager.Memory, packet);
            _receiveHook.OriginalFunction(GetManagerAddress(true), nostaleString.Get());
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Enable all networking hooks.
    /// </summary>
    public void EnableHooks()
    {
        _receiveHook.Enable();
        _sendHook.Enable();
    }

    /// <summary>
    /// Disable all the hooks that are currently enabled.
    /// </summary>
    public void DisableHooks()
    {
        _receiveHook.Disable();
        _sendHook.Disable();
    }

    private nuint GetManagerAddress(bool third)
    {
        nuint networkManager = _networkManagerAddress;
        _bindingManager.Memory.Read(networkManager, out networkManager);
        _bindingManager.Memory.Read(networkManager, out networkManager);
        _bindingManager.Memory.Read(networkManager, out networkManager);

        if (third)
        {
            _bindingManager.Memory.Read(networkManager + 0x34, out networkManager);
        }

        return networkManager;
    }

    private void SendPacketDetour(nuint packetObject, nuint packetString)
    {
        var packet = Marshal.PtrToStringAnsi((IntPtr)packetString);
        if (packet is null)
        { // ?
            _sendHook.OriginalFunction(packetObject, packetString);
        }
        else
        {
            var result = PacketSend?.Invoke(packet);
            if (result ?? true)
            {
                _sendHook.OriginalFunction(packetObject, packetString);
            }
        }
    }

    private bool _receivedCancel = false;

    private void ReceivePacketDetour(nuint packetObject, nuint packetString)
    {
        var packet = Marshal.PtrToStringAnsi((IntPtr)packetString);
        if (packet is null)
        { // ?
            _receiveHook.OriginalFunction(packetObject, packetString);
        }
        else
        {
            var result = PacketReceive?.Invoke(packet);
            if (result ?? true)
            {
                // This is a TEMPORARY fix, I don't know why,
                // but upon logging in (for OpenNos servers)
                // there is an exception when receiving packet
                // cancel.
                // TODO FIX THIS correctly
                if (_receivedCancel || !packet.StartsWith("cancel"))
                {
                    _receiveHook.OriginalFunction(packetObject, packetString);
                }
                else
                {
                    _receivedCancel = true;
                }
            }
        }
    }
}