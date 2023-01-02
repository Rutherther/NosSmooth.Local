//
//  NetworkBinding.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Extensions;
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
    private delegate nuint PacketSendDelegate(nuint packetObject, nuint packetString);

    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    private delegate nuint PacketReceiveDelegate(nuint packetObject, nuint packetString);

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

        var sendHookResult = bindingManager.CreateCustomAsmHookFromPattern<PacketSendDelegate>
            ("NetworkBinding.SendPacket", binding.SendPacketDetour, options.PacketSendHook);
        if (!sendHookResult.IsDefined(out var sendHook))
        {
            return Result<NetworkBinding>.FromError(sendHookResult);
        }

        var receiveHookResult = bindingManager.CreateCustomAsmHookFromPattern<PacketReceiveDelegate>
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
    private NosAsmHook<PacketSendDelegate> _sendHook = null!;
    private NosAsmHook<PacketReceiveDelegate> _receiveHook = null!;
    private bool _callingReceive;
    private bool _callingSend;

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
    public event EventHandler<PacketEventArgs>? PacketSendCall;

    /// <summary>
    /// Event that is called when packet receive was called by NosTale.
    /// </summary>
    /// <remarks>
    /// The receive must be hooked for this event to be called.
    /// </remarks>
    public event EventHandler<PacketEventArgs>? PacketReceiveCall;

    /// <summary>
    /// Send the given packet.
    /// </summary>
    /// <param name="packet">The packet to send.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result SendPacket(string packet)
    {
        try
        {
            _callingSend = true;
            using var nostaleString = NostaleStringA.Create(_bindingManager.Memory, packet);
            _sendHook.OriginalFunction.GetWrapper()(GetManagerAddress(false), nostaleString.Get());
            _callingSend = false;
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
            _callingReceive = true;
            using var nostaleString = NostaleStringA.Create(_bindingManager.Memory, packet);
            _receiveHook.OriginalFunction.GetWrapper()(GetManagerAddress(true), nostaleString.Get());
            _callingReceive = false;
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
        _receiveHook.Hook.EnableOrActivate();
        _sendHook.Hook.EnableOrActivate();
    }

    /// <summary>
    /// Disable all the hooks that are currently enabled.
    /// </summary>
    public void DisableHooks()
    {
        _receiveHook.Hook.Disable();
        _sendHook.Hook.Disable();
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

    private nuint SendPacketDetour(nuint packetObject, nuint packetString)
    {
        if (_callingSend)
        {
            return 1;
        }

        var packet = Marshal.PtrToStringAnsi((IntPtr)packetString);
        if (packet is null)
        { // ?
            return 1;
        }
        var packetArgs = new PacketEventArgs(packet);
        PacketSendCall?.Invoke(this, packetArgs);
        return packetArgs.Cancel ? 0 : (nuint)1;
    }

    private nuint ReceivePacketDetour(nuint packetObject, nuint packetString)
    {
        if (_callingReceive)
        {
            return 1;
        }

        var packet = Marshal.PtrToStringAnsi((IntPtr)packetString);
        if (packet is null)
        { // ?
            return 1;
        }

        var packetArgs = new PacketEventArgs(packet);
        PacketReceiveCall?.Invoke(this, packetArgs);
        return packetArgs.Cancel ? 0 : (nuint)1;
    }
}