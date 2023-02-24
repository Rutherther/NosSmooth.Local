//
//  ConflictEventArgs.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Extensions.SharedBinding.Lifetime;

namespace NosSmooth.Extensions.SharedBinding.EventArgs;

/// <summary>
/// Arguments containing information about a shared instance.
/// The conflict may be resolved by setting the correct property.
/// </summary>
public class ConflictEventArgs : System.EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictEventArgs"/> class.
    /// </summary>
    /// <param name="conflictingInstances">All of the conflicting instances.</param>
    /// <param name="defaultResolution">The default method to resolve the conflict.</param>
    public ConflictEventArgs(IReadOnlyList<SharedInstanceInfo> conflictingInstances, ConflictResolution defaultResolution)
    {
        ConflictingInstances = conflictingInstances;
        Resolve = defaultResolution;
    }

    /// <summary>
    /// Gets the instances that are in conflict.
    /// </summary>
    public IReadOnlyList<SharedInstanceInfo> ConflictingInstances { get; }

    /// <summary>
    /// Gets or sets the method of resolution.
    /// </summary>
    public ConflictResolution Resolve { get; set; }

    /// <summary>
    /// Possible methods of resolution.
    /// </summary>
    public enum ConflictResolution
    {
        /// <summary>
        /// Allow the new instance, keep the old ones.
        /// </summary>
        Allow,

        /// <summary>
        /// Do not allow the new instance, keep the old ones.
        /// </summary>
        Restrict,

        /// <summary>
        /// Allow the instance, try to detach the old instance.
        /// In case the old instance cannot be detached, fall back
        /// to <see cref="Restrict"/>.
        /// </summary>
        DetachOriginal
    }
}