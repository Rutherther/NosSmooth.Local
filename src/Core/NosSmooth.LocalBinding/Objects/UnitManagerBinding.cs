//
//  UnitManagerBinding.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Extensions;
using NosSmooth.LocalBinding.Options;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X86;
using Remora.Results;

namespace NosSmooth.LocalBinding.Objects;

/// <summary>
/// The nostale binding of a scene manager.
/// </summary>
/// <remarks>
/// The scene manager holds addresses to entities, mouse position, ....
/// </remarks>
public class UnitManagerBinding
{
    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    private delegate nuint FocusEntityDelegate(nuint unitManagerPtr, nuint entityPtr);

    /// <summary>
    /// Create the scene manager binding.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="bindingOptions">The options for the binding.</param>
    /// <returns>A network binding or an error.</returns>
    public static Result<UnitManagerBinding> Create
        (NosBindingManager bindingManager, UnitManagerBindingOptions bindingOptions)
    {
        var process = Process.GetCurrentProcess();

        var unitManagerStaticAddress = bindingManager.Scanner.FindPattern(bindingOptions.UnitManagerPattern);
        if (!unitManagerStaticAddress.Found)
        {
            return new BindingNotFoundError(bindingOptions.UnitManagerPattern, "UnitManagerBinding.UnitManager");
        }

        var binding = new UnitManagerBinding
        (
            bindingManager,
            (int)process.MainModule!.BaseAddress + unitManagerStaticAddress.Offset,
            bindingOptions.UnitManagerOffsets
        );

        var entityFocusHookResult = bindingManager.CreateHookFromPattern<FocusEntityDelegate>
            ("UnitManager.EntityFocus", binding.FocusEntityDetour, bindingOptions.EntityFocusHook);
        if (!entityFocusHookResult.IsDefined(out var entityFocusHook))
        {
            return Result<UnitManagerBinding>.FromError(entityFocusHookResult);
        }

        binding._focusHook = entityFocusHook;
        return binding;
    }

    private readonly int _staticUnitManagerAddress;
    private readonly int[] _unitManagerOffsets;

    private readonly NosBindingManager _bindingManager;

    private IHook<FocusEntityDelegate> _focusHook = null!;

    private UnitManagerBinding
    (
        NosBindingManager bindingManager,
        int staticUnitManagerAddress,
        int[] unitManagerOffsets
    )
    {
        _bindingManager = bindingManager;
        _staticUnitManagerAddress = staticUnitManagerAddress;
        _unitManagerOffsets = unitManagerOffsets;
    }

    /// <summary>
    /// Gets the address of unit manager.
    /// </summary>
    public nuint Address => _bindingManager.Memory.FollowStaticAddressOffsets
        (_staticUnitManagerAddress, _unitManagerOffsets);

    /// <summary>
    /// Event that is called when entity focus was called by NosTale.
    /// </summary>
    /// <remarks>
    /// The focus entity must be hooked for this event to be called.
    /// </remarks>
    public event Func<MapBaseObj?, bool>? EntityFocus;

    /// <summary>
    /// Focus the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result FocusEntity(MapBaseObj? entity)
        => FocusEntity(entity?.Address ?? nuint.Zero);

    /// <summary>
    /// Disable all UnitManager hooks.
    /// </summary>
    public void DisableHooks()
    {
        _focusHook.Disable();
    }

    /// <summary>
    /// Enable all UnitManager hooks.
    /// </summary>
    public void EnableHooks()
    {
        _focusHook.Enable();
    }

    /// <summary>
    /// Focus the entity.
    /// </summary>
    /// <param name="entityAddress">The entity address.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result FocusEntity(nuint entityAddress)
    {
        try
        {
            _focusHook.OriginalFunction(Address, entityAddress);
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }

    private nuint FocusEntityDetour(nuint unitManagerPtr, nuint entityId)
    {
        MapBaseObj? obj = null;
        if (entityId != nuint.Zero)
        {
            obj = new MapBaseObj(_bindingManager.Memory, entityId);
        }

        var result = EntityFocus?.Invoke(obj);

        if (result ?? true)
        {
            return _focusHook.OriginalFunction(unitManagerPtr, entityId);
        }

        return 0;
    }
}