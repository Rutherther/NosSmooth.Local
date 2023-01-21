//
//  IWinControlHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Hooks.Definitions.X86;

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// A hook of WinControl.MainWndProc.
/// </summary>
public interface IWinControlHook :
    INostaleHook<IWinControlHook.WinControlDelegate, IWinControlHook.WinControlWrapperDelegate, WindowsMessageEventArgs>
{
    /// <summary>
    /// NosTale window control function to hook.
    /// </summary>
    /// <param name="messagePtr">The message pointer.</param>
    /// <returns>1 to proceed to NosTale function, 0 to block the call.</returns>
    [Function
    (
        new[] { FunctionAttribute.Register.eax },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    public delegate nuint WinControlDelegate(nuint messagePtr);

    /// <summary>
    /// Packet send function.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public delegate void WinControlWrapperDelegate(Message message);
}