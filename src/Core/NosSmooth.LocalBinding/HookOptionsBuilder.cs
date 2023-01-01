//
//  HookOptionsBuilder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.LocalBinding;

/// <summary>
/// Builder for <see cref="HookOptions"/>.
/// </summary>
public class HookOptionsBuilder
{
    private bool _hook;
    private string _pattern;
    private int _offset;

    /// <summary>
    /// Initializes a new instance of the <see cref="HookOptionsBuilder"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    internal HookOptionsBuilder(HookOptions options)
    {
        _hook = options.Hook;
        _pattern = options.MemoryPattern;
        _offset = options.Offset;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HookOptionsBuilder"/> class.
    /// </summary>
    /// <param name="hook">Whether to hook the function.</param>
    /// <param name="pattern">The default pattern.</param>
    /// <param name="offset">The default offset.</param>
    internal HookOptionsBuilder(bool hook, string pattern, int offset)
    {
        _offset = offset;
        _pattern = pattern;
        _hook = hook;
    }

    /// <summary>
    /// Configure whether to hook this function.
    /// Default true.
    /// </summary>
    /// <param name="hook">Whether to hook the function.</param>
    /// <returns>This builder.</returns>
    public HookOptionsBuilder Hook(bool hook = true)
    {
        _hook = hook;
        return this;
    }

    /// <summary>
    /// Configure the memory pattern.
    /// Use ?? for any bytes.
    /// </summary>
    /// <param name="pattern">The memory pattern.</param>
    /// <returns>This builder.</returns>
    public HookOptionsBuilder MemoryPattern(string pattern)
    {
        _pattern = pattern;
        return this;
    }

    /// <summary>
    /// Configure the offset from the pattern.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <returns>This builder.</returns>
    public HookOptionsBuilder Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    /// <summary>
    /// Create hook options from this builder.
    /// </summary>
    /// <returns>The options.</returns>
    internal HookOptions Build()
        => new HookOptions(_hook, _pattern, _offset);
}