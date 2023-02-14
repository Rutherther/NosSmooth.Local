//
//  WindowsMessageEventArgs.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using NosSmooth.LocalBinding.Structs;

namespace NosSmooth.LocalBinding.EventArgs;

/// <summary>
/// Event arguments for windows message.
/// </summary>
public class WindowsMessageEventArgs : CancelEventArgs
{
    /// <summary>
    /// Gets the sent message.
    /// </summary>
    public Message Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsMessageEventArgs"/> class.
    /// </summary>
    /// <param name="message">The windows message.</param>
    public WindowsMessageEventArgs(Message message)
    {
        Message = message;
    }
}