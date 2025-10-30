// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Native;

namespace Vello;

/// <summary>
/// Defines color mixing modes for blend operations.
/// </summary>
public enum Mix : byte
{
    /// <summary>Normal blending (default)</summary>
    Normal = 0,
    /// <summary>Multiply colors</summary>
    Multiply = 1,
    /// <summary>Screen blending</summary>
    Screen = 2,
    /// <summary>Overlay blending</summary>
    Overlay = 3,
    /// <summary>Darken colors</summary>
    Darken = 4,
    /// <summary>Lighten colors</summary>
    Lighten = 5,
    /// <summary>Color dodge</summary>
    ColorDodge = 6,
    /// <summary>Color burn</summary>
    ColorBurn = 7,
    /// <summary>Hard light</summary>
    HardLight = 8,
    /// <summary>Soft light</summary>
    SoftLight = 9,
    /// <summary>Difference</summary>
    Difference = 10,
    /// <summary>Exclusion</summary>
    Exclusion = 11,
    /// <summary>Hue blending</summary>
    Hue = 12,
    /// <summary>Saturation blending</summary>
    Saturation = 13,
    /// <summary>Color blending</summary>
    Color = 14,
    /// <summary>Luminosity blending</summary>
    Luminosity = 15
}

/// <summary>
/// Defines alpha compositing modes for blend operations.
/// </summary>
public enum Compose : byte
{
    /// <summary>Clear destination</summary>
    Clear = 0,
    /// <summary>Copy source</summary>
    Copy = 1,
    /// <summary>Keep destination</summary>
    Dest = 2,
    /// <summary>Source over destination (default)</summary>
    SrcOver = 3,
    /// <summary>Destination over source</summary>
    DestOver = 4,
    /// <summary>Source in destination</summary>
    SrcIn = 5,
    /// <summary>Destination in source</summary>
    DestIn = 6,
    /// <summary>Source out destination</summary>
    SrcOut = 7,
    /// <summary>Destination out source</summary>
    DestOut = 8,
    /// <summary>Source atop destination</summary>
    SrcAtop = 9,
    /// <summary>Destination atop source</summary>
    DestAtop = 10,
    /// <summary>XOR operation</summary>
    Xor = 11,
    /// <summary>Add source and destination</summary>
    Plus = 12,
    /// <summary>Lighter version of Plus</summary>
    PlusLighter = 13
}

/// <summary>
/// Represents a blend mode combining mix and compose operations.
/// </summary>
/// <param name="Mix">The color mixing mode</param>
/// <param name="Compose">The alpha compositing mode</param>
public readonly record struct BlendMode(Mix Mix, Compose Compose)
{
    /// <summary>
    /// Default blend mode (Normal mix with SrcOver compose).
    /// </summary>
    public static readonly BlendMode Default = new(Mix.Normal, Compose.SrcOver);

    /// <summary>
    /// Creates a blend mode with Normal mix and the specified compose mode.
    /// </summary>
    public static BlendMode Normal(Compose compose = Compose.SrcOver) => new(Mix.Normal, compose);

    /// <summary>
    /// Creates a blend mode with Multiply mix and the specified compose mode.
    /// </summary>
    public static BlendMode Multiply(Compose compose = Compose.SrcOver) => new(Mix.Multiply, compose);

    /// <summary>
    /// Creates a blend mode with Screen mix and the specified compose mode.
    /// </summary>
    public static BlendMode Screen(Compose compose = Compose.SrcOver) => new(Mix.Screen, compose);

    /// <summary>
    /// Convert to native blend mode structure.
    /// </summary>
    internal VelloBlendMode ToNative() => new()
    {
        Mix = (VelloMix)Mix,
        Compose = (VelloCompose)Compose
    };
}
