//
//  NosBindingManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Options;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Helpers;
using Reloaded.Hooks.Tools;
using Reloaded.Hooks.X86;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace NosSmooth.LocalBinding;

/// <summary>
/// Nostale entity binding manager.
/// </summary>
public class NosBindingManager : IDisposable
{
    private readonly NosBrowserManager _browserManager;
    private readonly PetManagerBindingOptions _petManagerBindingOptions;
    private readonly CharacterBindingOptions _characterBindingOptions;
    private readonly NetworkBindingOptions _networkBindingOptions;
    private readonly UnitManagerBindingOptions _unitManagerBindingOptions;
    private readonly PeriodicBindingOptions _periodicBindingOptions;

    private NetworkBinding? _networkBinding;
    private PlayerManagerBinding? _characterBinding;
    private UnitManagerBinding? _unitManagerBinding;
    private PetManagerBinding? _petManagerBinding;
    private PeriodicBinding? _periodicBinding;

    /// <summary>
    /// Initializes a new instance of the <see cref="NosBindingManager"/> class.
    /// </summary>
    /// <param name="browserManager">The NosTale browser manager.</param>
    /// <param name="characterBindingOptions">The character binding options.</param>
    /// <param name="networkBindingOptions">The network binding options.</param>
    /// <param name="sceneManagerBindingOptions">The scene manager binding options.</param>
    /// <param name="petManagerBindingOptions">The pet manager binding options.</param>
    /// <param name="periodicBindingOptions">The periodic binding options.</param>
    public NosBindingManager
    (
        NosBrowserManager browserManager,
        IOptions<CharacterBindingOptions> characterBindingOptions,
        IOptions<NetworkBindingOptions> networkBindingOptions,
        IOptions<UnitManagerBindingOptions> sceneManagerBindingOptions,
        IOptions<PetManagerBindingOptions> petManagerBindingOptions,
        IOptions<PeriodicBindingOptions> periodicBindingOptions
    )
    {
        _browserManager = browserManager;
        Hooks = new ReloadedHooks();
        Memory = new Memory();
        Scanner = new Scanner(Process.GetCurrentProcess(), Process.GetCurrentProcess().MainModule);
        _characterBindingOptions = characterBindingOptions.Value;
        _networkBindingOptions = networkBindingOptions.Value;
        _unitManagerBindingOptions = sceneManagerBindingOptions.Value;
        _petManagerBindingOptions = petManagerBindingOptions.Value;
        _periodicBindingOptions = periodicBindingOptions.Value;
    }

    /// <summary>
    /// Gets the memory scanner.
    /// </summary>
    internal Scanner Scanner { get; }

    /// <summary>
    /// Gets the reloaded hooks.
    /// </summary>
    internal IReloadedHooks Hooks { get; }

    /// <summary>
    /// Gets the current process memory.
    /// </summary>
    internal IMemory Memory { get; }

    /// <summary>
    /// Gets the periodic binding.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the manager is not initialized yet.</exception>
    public PeriodicBinding Periodic
    {
        get
        {
            if (_periodicBinding is null)
            {
                throw new InvalidOperationException
                (
                    "Could not get periodic. The binding manager is not initialized. Did you forget to call NosBindingManager.Initialize?"
                );
            }

            return _periodicBinding;
        }
    }

    /// <summary>
    /// Gets the network binding.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the manager is not initialized yet.</exception>
    public NetworkBinding Network
    {
        get
        {
            if (_networkBinding is null)
            {
                throw new InvalidOperationException
                (
                    "Could not get network. The binding manager is not initialized. Did you forget to call NosBindingManager.Initialize?"
                );
            }

            return _networkBinding;
        }
    }

    /// <summary>
    /// Gets the character binding.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the manager is not initialized yet.</exception>
    public PlayerManagerBinding PlayerManager
    {
        get
        {
            if (_characterBinding is null)
            {
                throw new InvalidOperationException
                (
                    "Could not get character. The binding manager is not initialized. Did you forget to call NosBindingManager.Initialize?"
                );
            }

            return _characterBinding;
        }
    }

    /// <summary>
    /// Gets the character binding.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the manager is not initialized yet.</exception>
    public UnitManagerBinding UnitManager
    {
        get
        {
            if (_unitManagerBinding is null)
            {
                throw new InvalidOperationException
                (
                    "Could not get scene manager. The binding manager is not initialized. Did you forget to call NosBindingManager.Initialize?"
                );
            }

            return _unitManagerBinding;
        }
    }

    /// <summary>
    /// Gets the pet manager binding.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the manager is not initialized yet.</exception>
    public PetManagerBinding PetManager
    {
        get
        {
            if (_petManagerBinding is null)
            {
                throw new InvalidOperationException
                (
                    "Could not get pet manager. The binding manager is not initialized. Did you forget to call NosBindingManager.Initialize?"
                );
            }

            return _petManagerBinding;
        }
    }

    /// <summary>
    /// Initialize the existing bindings and hook NosTale functions.
    /// </summary>
    /// <returns>A result that may or may not have succeeded.</returns>
    public IResult Initialize()
    {
        if (_networkBinding is not null)
        { // already initialized
            return Result.FromSuccess();
        }

        List<IResult> errorResults = new List<IResult>();
        var browserInitializationResult = _browserManager.Initialize();
        if (!browserInitializationResult.IsSuccess)
        {
            if (browserInitializationResult.Error is NotNostaleProcessError)
            {
                return browserInitializationResult;
            }

            errorResults.Add(browserInitializationResult);
        }

        try
        {
            var periodicBinding = PeriodicBinding.Create
            (
                this,
                _periodicBindingOptions
            );
            if (!periodicBinding.IsSuccess)
            {
                errorResults.Add
                (
                    Result.FromError
                    (
                        new CouldNotInitializeModuleError(typeof(PeriodicBinding), periodicBinding.Error),
                        periodicBinding
                    )
                );
            }
            _periodicBinding = periodicBinding.Entity;
        }
        catch (Exception e)
        {
            errorResults.Add
            (
                Result.FromError
                (
                    new CouldNotInitializeModuleError(typeof(PeriodicBinding), new ExceptionError(e)),
                    (Result)new ExceptionError(e)
                )
            );
        }

        try
        {
            var network = NetworkBinding.Create(this, _networkBindingOptions);
            if (!network.IsSuccess)
            {
                errorResults.Add
                (
                    Result.FromError
                    (
                        new CouldNotInitializeModuleError(typeof(NetworkBinding), network.Error),
                        network
                    )
                );
            }

            _networkBinding = network.Entity;
        }
        catch (Exception e)
        {
            errorResults.Add
            (
                Result.FromError
                (
                    new CouldNotInitializeModuleError(typeof(NetworkBinding), new ExceptionError(e)),
                    (Result)new ExceptionError(e)
                )
            );
        }

        try
        {
            var playerManagerBinding = PlayerManagerBinding.Create
            (
                this,
                _browserManager.PlayerManager,
                _characterBindingOptions
            );
            if (!playerManagerBinding.IsSuccess)
            {
                errorResults.Add
                (
                    Result.FromError
                    (
                        new CouldNotInitializeModuleError(typeof(PlayerManagerBinding), playerManagerBinding.Error),
                        playerManagerBinding
                    )
                );
            }
            _characterBinding = playerManagerBinding.Entity;
        }
        catch (Exception e)
        {
            errorResults.Add
            (
                Result.FromError
                (
                    new CouldNotInitializeModuleError(typeof(PlayerManagerBinding), new ExceptionError(e)),
                    (Result)new ExceptionError(e)
                )
            );
        }

        try
        {
            var unitManagerBinding = UnitManagerBinding.Create
            (
                this,
                _unitManagerBindingOptions
            );
            if (!unitManagerBinding.IsSuccess)
            {
                errorResults.Add
                (
                    Result.FromError
                    (
                        new CouldNotInitializeModuleError(typeof(UnitManagerBinding), unitManagerBinding.Error),
                        unitManagerBinding
                    )
                );
            }
            _unitManagerBinding = unitManagerBinding.Entity;
        }
        catch (Exception e)
        {
            errorResults.Add
            (
                Result.FromError
                (
                    new CouldNotInitializeModuleError(typeof(UnitManagerBinding), new ExceptionError(e)),
                    (Result)new ExceptionError(e)
                )
            );
        }

        try
        {
            var petManagerBinding = PetManagerBinding.Create
            (
                this,
                _browserManager.PetManagerList,
                _petManagerBindingOptions
            );
            if (!petManagerBinding.IsSuccess)
            {
                errorResults.Add
                (
                    Result.FromError
                    (
                        new CouldNotInitializeModuleError(typeof(PetManagerBinding), petManagerBinding.Error),
                        petManagerBinding
                    )
                );
            }
            _petManagerBinding = petManagerBinding.Entity;
        }
        catch (Exception e)
        {
            errorResults.Add
            (
                Result.FromError
                (
                    new CouldNotInitializeModuleError(typeof(UnitManagerBinding), new ExceptionError(e)),
                    (Result)new ExceptionError(e)
                )
            );
        }

        return errorResults.Count switch
        {
            0 => Result.FromSuccess(),
            1 => errorResults[0],
            _ => (Result)new AggregateError(errorResults)
        };
    }

    /// <summary>
    /// Disable the currently enabled NosTale hooks.
    /// </summary>
    public void DisableHooks()
    {
        _networkBinding?.DisableHooks();
        _characterBinding?.DisableHooks();
        _unitManagerBinding?.DisableHooks();
        _petManagerBinding?.DisableHooks();
    }

    /// <summary>
    /// Enable all NosTale hooks.
    /// </summary>
    public void EnableHooks()
    {
        _networkBinding?.EnableHooks();
        _characterBinding?.EnableHooks();
        _unitManagerBinding?.EnableHooks();
        _petManagerBinding?.EnableHooks();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Scanner.Dispose();
        DisableHooks();
    }

    /// <summary>
    /// Create a hook object for the given pattern.
    /// </summary>
    /// <param name="name">The name of the binding.</param>
    /// <param name="callbackFunction">The callback function to call instead of the original one.</param>
    /// <param name="options">The options for the function hook. (pattern, offset, whether to activate).</param>
    /// <typeparam name="TFunction">The type of the function.</typeparam>
    /// <returns>The hook object or an error.</returns>
    internal Result<IHook<TFunction>> CreateHookFromPattern<TFunction>
    (
        string name,
        TFunction callbackFunction,
        HookOptions options
    )
    {
        var walkFunctionAddress = Scanner.FindPattern(options.MemoryPattern);
        if (!walkFunctionAddress.Found)
        {
            return new BindingNotFoundError(options.MemoryPattern, name);
        }

        try
        {
            var hook = Hooks.CreateHook
            (
                callbackFunction,
                walkFunctionAddress.Offset + (int)_browserManager.Process.MainModule!.BaseAddress + options.Offset
            );
            if (options.Hook)
            {
                hook.Activate();
            }

            return Result<IHook<TFunction>>.FromSuccess(hook);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Create custom assembler hook.
    /// </summary>
    /// <remarks>
    /// Sometimes there are more requirements than Reloaded-Hooks handles
    /// (or maybe I am just configuring it correctly).
    ///
    /// For these cases this method is here. It adds a detour call at the beginning
    /// of a function. The detour function should return 1 to continue,
    /// 0 to return at the beginning.
    /// </remarks>
    /// <param name="name">The name of the binding.</param>
    /// <param name="callbackFunction">The callback function to call instead of the original one.</param>
    /// <param name="options">The options for the function hook. (pattern, offset, whether to activate).</param>
    /// <typeparam name="TFunction">The type of the function.</typeparam>
    /// <returns>The hook object or an error.</returns>
    internal Result<NosAsmHook<TFunction>>
        CreateCustomAsmHookFromPattern<TFunction>
        (
            string name,
            TFunction callbackFunction,
            HookOptions options
        )
    {
        var walkFunctionAddress = Scanner.FindPattern(options.MemoryPattern);
        if (!walkFunctionAddress.Found)
        {
            return new BindingNotFoundError(options.MemoryPattern, name);
        }

        try
        {
            var address = walkFunctionAddress.Offset + (int)_browserManager.Process.MainModule!.BaseAddress
                + options.Offset;
            var wrapper = Hooks.CreateFunction<TFunction>(address);
            var reverseWrapper = Hooks.CreateReverseWrapper<TFunction>(callbackFunction);
            var callDetour = Utilities.GetAbsoluteCallMnemonics
                (reverseWrapper.WrapperPointer.ToUnsigned(), IntPtr.Size == 8);

            var hook = Hooks.CreateAsmHook
            (
                new[]
                {
                    "use32",
                    "pushad",
                    "pushfd",
                    "push edx",
                    "push eax",

                    // call managed function
                    callDetour,

                    // check result
                    // 1 means continue executing
                    // 0 means do not permit the call
                    "test eax, eax",
                    "jnz rest",

                    // returned 0, going to return early
                    "pop eax",
                    "pop edx",
                    "popfd",
                    "popad",
                    "ret", // return early

                    // returned 1, going to execute the function
                    "rest:",
                    "pop eax",
                    "pop edx",
                    "popfd",
                    "popad"
                },
                address
            );

            if (options.Hook)
            {
                hook.Activate();
            }

            return Result<NosAsmHook<TFunction>>.FromSuccess
                (new NosAsmHook<TFunction>(reverseWrapper, wrapper, hook));
        }
        catch (Exception e)
        {
            return e;
        }
    }
}