// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.InteropServices;

namespace Vello;

/// <summary>
/// Represents a positioned glyph for text rendering.
/// </summary>
/// <param name="Id">The font-specific glyph identifier (not Unicode codepoint)</param>
/// <param name="X">X offset in pixels</param>
/// <param name="Y">Y offset in pixels</param>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Glyph(uint Id, float X, float Y)
{
    /// <summary>
    /// Creates a glyph at the origin.
    /// </summary>
    public Glyph(uint id) : this(id, 0, 0) { }
}
