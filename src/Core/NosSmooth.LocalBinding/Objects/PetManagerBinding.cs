//
//  PetManagerBinding.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Extensions;
using NosSmooth.LocalBinding.Options;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace NosSmooth.LocalBinding.Objects;

/// <summary>
/// The binding to NosTale pet manager.
/// </summary>
public class PetManagerBinding
{
    private readonly IMemory _memory;

    /// <summary>
    /// Create nostale pet manager binding.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="petManagerList">The list of the pet managers.</param>
    /// <param name="options">The options.</param>
    /// <returns>A pet manager binding or and error.</returns>
    public static Result<PetManagerBinding> Create
        (NosBindingManager bindingManager, PetManagerList petManagerList, PetManagerBindingOptions options)
    {
        var petManager = new PetManagerBinding(bindingManager.Memory, petManagerList);
        var hookResult = bindingManager.CreateHookFromPattern<PetWalkDelegate>
        (
            "PetManagerBinding.PetWalk",
            petManager.PetWalkDetour,
            options.PetWalkHook
        );

        if (!hookResult.IsSuccess)
        {
            return Result<PetManagerBinding>.FromError(hookResult);
        }

        petManager._petWalkHook = hookResult.Entity;
        return petManager;
    }

    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx, FunctionAttribute.Register.ecx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    private delegate bool PetWalkDelegate
    (
        nuint petManagerPtr,
        uint position,
        short unknown0 = 0,
        int unknown1 = 1,
        int unknown2 = 1
    );

    private IHook<PetWalkDelegate> _petWalkHook = null!;

    private PetManagerBinding(IMemory memory, PetManagerList petManagerList)
    {
        _memory = memory;
        PetManagerList = petManagerList;
    }

    /// <summary>
    /// Event that is called when walk was called by NosTale.
    /// </summary>
    /// <remarks>
    /// The walk must be hooked for this event to be called.
    /// </remarks>
    public event Func<PetManager, ushort, ushort, bool>? PetWalkCall;

    /// <summary>
    /// Gets the hook of the pet walk function.
    /// </summary>
    public IHook PetWalkHook => _petWalkHook;

    /// <summary>
    /// Gets pet manager list.
    /// </summary>
    public PetManagerList PetManagerList { get; }

    /// <summary>
    /// Disable all PetManager hooks.
    /// </summary>
    public void DisableHooks()
    {
        _petWalkHook.Disable();
    }

    /// <summary>
    /// Enable all PetManager hooks.
    /// </summary>
    public void EnableHooks()
    {
        _petWalkHook.EnableOrActivate();
    }

    /// <summary>
    /// Walk the given pet to the given location.
    /// </summary>
    /// <param name="selector">Index of the pet to walk. -1 for every pet currently available.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>A result returned from NosTale or an error.</returns>
    public Result<bool> PetWalk(int selector, short x, short y)
    {
        uint position = Convert.ToUInt32(((ushort)y << 16) | (ushort)x);
        if (PetManagerList.Length < selector + 1)
        {
            return new NotFoundError("Could not find the pet using the given selector.");
        }

        if (selector == -1)
        {
            bool lastResult = true;
            for (int i = 0; i < PetManagerList.Length; i++)
            {
                lastResult = _petWalkHook.OriginalFunction(PetManagerList[i].Address, position);
            }

            return lastResult;
        }
        else
        {
            return _petWalkHook.OriginalFunction(PetManagerList[selector].Address, position);
        }
    }

    private bool PetWalkDetour
    (
        nuint petManagerPtr,
        uint position,
        short unknown0 = 0,
        int unknown1 = 1,
        int unknown2 = 1
    )
    {
        var result = PetWalkCall?.Invoke(new PetManager(_memory, petManagerPtr), (ushort)(position & 0xFFFF), (ushort)((position >> 16) & 0xFFFF));
        if (result ?? true)
        {
            return _petWalkHook.OriginalFunction
            (
                petManagerPtr,
                position,
                unknown0,
                unknown1,
                unknown2
            );
        }

        return false;
    }
}