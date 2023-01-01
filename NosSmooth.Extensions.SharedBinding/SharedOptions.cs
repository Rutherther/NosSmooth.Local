//
//  SharedOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using NosSmooth.Data.NOSFiles;
using NosSmooth.LocalBinding;
using NosSmooth.PacketSerializer.Packets;

namespace NosSmooth.Extensions.SharedBinding;

/// <summary>
/// Options for <see cref="SharedManager"/>.
/// </summary>
internal class SharedOptions
{
    /// <summary>
    /// Gets or sets the original descriptor of <see cref="NosBindingManager"/>.
    /// </summary>
    public ServiceDescriptor? BindingDescriptor { get; set; }

    /// <summary>
    /// Gets or sets the original descriptor of <see cref="NostaleDataFilesManager"/>.
    /// </summary>
    public ServiceDescriptor? FileDescriptor { get; set; }

    /// <summary>
    /// Gets or sets the original descriptor of <see cref="IPacketTypesRepository"/>.
    /// </summary>
    public ServiceDescriptor? PacketRepositoryDescriptor { get; set; }
}