// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

namespace Vello.Native;

/// <summary>
/// Render mode enumeration
/// </summary>
public enum VelloRenderMode : byte
{
    OptimizeSpeed = 0,
    OptimizeQuality = 1
}

/// <summary>
/// SIMD level enumeration
/// </summary>
public enum VelloSimdLevel : byte
{
    Fallback = 0,
    Sse2 = 1,
    Sse42 = 2,
    Avx = 3,
    Avx2 = 4,
    Avx512 = 5,
    Neon = 6
}

/// <summary>
/// Line join style
/// </summary>
public enum VelloJoin : byte
{
    Bevel = 0,
    Miter = 1,
    Round = 2
}

/// <summary>
/// Line cap style
/// </summary>
public enum VelloCap : byte
{
    Butt = 0,
    Square = 1,
    Round = 2
}

/// <summary>
/// Fill rule
/// </summary>
public enum VelloFillRule : byte
{
    NonZero = 0,
    EvenOdd = 1
}

/// <summary>
/// Blend mix mode
/// </summary>
public enum VelloMix : byte
{
    Normal = 0,
    Multiply = 1,
    Screen = 2,
    Overlay = 3,
    Darken = 4,
    Lighten = 5,
    ColorDodge = 6,
    ColorBurn = 7,
    HardLight = 8,
    SoftLight = 9,
    Difference = 10,
    Exclusion = 11,
    Hue = 12,
    Saturation = 13,
    Color = 14,
    Luminosity = 15
}

/// <summary>
/// Blend compose mode
/// </summary>
public enum VelloCompose : byte
{
    Clear = 0,
    Copy = 1,
    Dest = 2,
    SrcOver = 3,
    DestOver = 4,
    SrcIn = 5,
    DestIn = 6,
    SrcOut = 7,
    DestOut = 8,
    SrcAtop = 9,
    DestAtop = 10,
    Xor = 11,
    Plus = 12,
    PlusLighter = 13
}

/// <summary>
/// Gradient extend mode
/// </summary>
public enum VelloExtend : byte
{
    Pad = 0,
    Repeat = 1,
    Reflect = 2
}
