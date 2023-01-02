//
//  PeriodicBinding.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Extensions;
using NosSmooth.LocalBinding.Options;
using Reloaded.Hooks.Definitions.X86;
using Remora.Results;

namespace NosSmooth.LocalBinding.Objects;

/// <summary>
/// Binds to a periodic function to allow synchronizing.
/// </summary>
public class PeriodicBinding
{
    [Function
    (
        new FunctionAttribute.Register[0],
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp, FunctionAttribute.Register.eax, FunctionAttribute.Register.edx, FunctionAttribute.Register.ecx }
    )]
    private delegate void PeriodicDelegate();

    /// <summary>
    /// Create the periodic binding with finding the periodic function.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A periodic binding or an error.</returns>
    public static Result<PeriodicBinding> Create(NosBindingManager bindingManager, PeriodicBindingOptions options)
    {
        var binding = new PeriodicBinding();

        var periodicHookResult = bindingManager.CreateCustomAsmHookFromPattern<PeriodicDelegate>
            ("PeriodicBinding.Periodic", binding.PeriodicDetour, options.PeriodicHook);
        if (!periodicHookResult.IsDefined(out var periodicHook))
        {
            return Result<PeriodicBinding>.FromError(periodicHookResult);
        }

        binding._periodicHook = periodicHook;
        return binding;
    }

    private NosAsmHook<PeriodicDelegate>? _periodicHook;

    private PeriodicBinding()
    {
    }

    /// <summary>
    /// An action called on every period.
    /// </summary>
    public event EventHandler? PeriodicCall;

    /// <summary>
    /// Enable all networking hooks.
    /// </summary>
    public void EnableHooks()
    {
        _periodicHook?.Hook.EnableOrActivate();
    }

    /// <summary>
    /// Disable all the hooks that are currently enabled.
    /// </summary>
    public void DisableHooks()
    {
        _periodicHook?.Hook.Disable();
    }

    private void PeriodicDetour()
    {
        PeriodicCall?.Invoke(this, System.EventArgs.Empty);
    }
}