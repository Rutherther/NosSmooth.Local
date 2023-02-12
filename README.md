# NosSmooth.Local
NosSmooth is a multi-platform library for NosTale
packets, data, game state and such.
See the main repository at [NosSmooth](https://github.com/Rutherther/NosSmooth).
NosSmooth.Local contains libraries for the regular NosTale client
such as memory mapping and NosSmooth client implementation for
handling the packets. NosSmooth.Local is Windows-only
(at least not tested on Linux as I was not able to get NosTale running on Linux correctly)

NosSmooth.Local contains bindings to parts of game memory and some functions that may be called or hooked.
It contains a client using these bindings to receive and send packets, to obtain received and sent packets.
Injector for injecting NosSmooth into the process is included as well.

See samples in the `src/Samples` folder.

## Features

### Bindings (hooks, memory mapping)
For accessing memory and functions, [NosSmooth.LocalBinding](https://github.com/Rutherther/NosSmooth.Local/tree/main/src/Core/NosSmooth.LocalBinding) can be used. LocalBinding uses Reloaded.Memory and Reloaded.Hooks. If a function is hooked, it may be canceled.

Reading NosTale memory may be done in an external process. `NosBrowserManager` may be used. It first have to be initialized by calling `NosBrowserManager.Initialize()`. Don't forget to check the result to know when something was not found in the memory. If something cannot be found, other things will still be initialized so if you don't use the part that wasn't loaded, your program may still work.

To bind inside of a NosTale process and use or hook functions, use `NosBindingManager`. Same as `NosBrowserManager`, initialization has to be called. `NosBindingManager.Initialize`. It initializes `NosBrowserManager` and `IHookManager`.

To obtain found functions and register event handlers for hooked functions, use `IHookManager`.

#### Configuration
For configuring patterns and offsets of objects, options for each object are exposed, ie. `PlayerManagerOptions`, `PetManagerOptions`.

If you want to configure what functions should be hooked and patterns to find them in memory, use `HookManagerOptions`. There is a builder for these options added. When using dependency injection, `ConfigureHooks` on `IServiceCollection` can be used, see:
```cs
.ConfigureHooks(b => b
  .HookFocus(focus => focus
      .MemoryPattern("73 00 00 00 55 8b ec b9 05 00 00 00")
      .Offset(4)))
```

#### Calling NosTale functions
Classes with hooks can be used for calling the functions. It is important to use NosTale functions from NosTale thread. If you don't do that, you may run into fatal errors, exceptions and the game may crash. For calling functions on NosTale thread, NosThreadSynchronizer may be used.

It is a class located in LocalBinding project. For it to work, PeriodicHook has to be hooked. Periodic hook can be any function that is called periodically from NosTale thread. (in case the current pattern did not work and new periodic function had to be retrieved)

### Client
To use all features of NosSmooth such as packet responders, obtaining game state etc., you will need an instance of `INostaleClient`.
`NosSmooth.LocalClient` implements `INostaleClient` using `NosSmooth.LocalBinding`.

The client supports receiving and sending packets as well as implementing walk and attack commands.
The attack command focues current entity. Support for cancelling commands by using user operation may be added by hooking all functions.
Cancelling user action is possible as well when the correct arguments are passed to the command itself.

### Injector
NosSmooth uses .NET 7 and as NosTale is 32-bit process, Native dll cannot be created for .NET 7 as far as I know. That means any library created in .NET 5+ will have to be injected using a custom library that runs the .net runtime. That is why NosSmooth.Inject has been created. It is a c++ library that can run .NET runtime and call a method inside .NET code.

If you want to inject NosSmooth into NosTale, see https://github.com/Rutherther/NosSmooth.Local/wiki/How-to-inject-NosSmooth-to-NosTale.

## Injecting multiple NosSmooth instances into NosTale
If you are going to hook any functions (such as packet recv, send for working with `INostaleClient`, these are hooked by default), another program that would try to hook these methods probably won't work. That is why you won't be able to use programs by other people when using NosSmooth.Local.

For using other program with NosSmooth, you may use packet capture instead of injection. See [NosSmooth.Pcap](https://github.com/Rutherther/NosSmooth/tree/main/Pcap/NosSmooth.Pcap). Pcap has its limitations, you won't be able to send or receive packets. Use that if you need to observe only. There are other options. If the program you want to use does not have to be injected, but can be used by communicating with NosTale using some kind of communication means such as tcp or named pipes, you may implement protocol of that application using NosSmooth. I have already implemented some means of communication in [NosSmooth.Comms](https://github.com/Rutherther/NosSmooth.Comms).

Using multiple instances of NosSmooth is possible by using `NosSmooth.Extensions.SharedBinding` with some restrictions. SharedBinding wraps NosSmooth hooks and gives the hooks out to any instance that connects. When using dependency injection, call `ShareNosSmooth()` on service collection. That will share the hooks, NosTale data, packet types and `NosBrowserManager`. Every NosSmooth instance has to use sharing in order to make sharing work correctly.
