// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

namespace Vello;

/// <summary>
/// Represents a color stop in a gradient, consisting of an offset and a color.
/// </summary>
/// <param name="Offset">Position of the color stop (0.0 to 1.0)</param>
/// <param name="Color">Color at this stop</param>
public readonly record struct ColorStop(float Offset, Color Color)
{
    /// <summary>
    /// Creates a color stop with the specified offset and RGBA values.
    /// </summary>
    public ColorStop(float offset, byte r, byte g, byte b, byte a = 255)
        : this(offset, new Color(r, g, b, a))
    {
    }
}
