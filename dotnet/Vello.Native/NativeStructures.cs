// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.InteropServices;

namespace Vello.Native;

/// <summary>
/// Premultiplied RGBA8 color
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct VelloPremulRgba8
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;
}

/// <summary>
/// 2D point
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct VelloPoint
{
    public double X;
    public double Y;
}

/// <summary>
/// Rectangle
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct VelloRect
{
    public double X0;
    public double Y0;
    public double X1;
    public double Y1;
}

/// <summary>
/// 2D affine transformation (2x3 matrix)
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct VelloAffine
{
    public double M11;
    public double M12;
    public double M13;
    public double M21;
    public double M22;
    public double M23;
}

/// <summary>
/// Stroke parameters
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct VelloStroke
{
    public float Width;
    public float MiterLimit;
    public VelloJoin Join;
    public VelloCap StartCap;
    public VelloCap EndCap;
    private byte _padding1;
    private byte _padding2;
    private byte _padding3;
}

/// <summary>
/// Render settings
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct VelloRenderSettings
{
    public VelloSimdLevel Level;
    public ushort NumThreads;
    public VelloRenderMode RenderMode;
    private byte _padding;
}

/// <summary>
/// Blend mode
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct VelloBlendMode
{
    public VelloMix Mix;
    public VelloCompose Compose;
}

/// <summary>
/// Color stop for gradients
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct VelloColorStop
{
    public float Offset;
    public byte R;
    public byte G;
    public byte B;
    public byte A;
}

/// <summary>
/// Glyph for text rendering
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct VelloGlyph
{
    /// <summary>
    /// Glyph ID (font-specific, not Unicode)
    /// </summary>
    public uint Id;

    /// <summary>
    /// X offset in pixels
    /// </summary>
    public float X;

    /// <summary>
    /// Y offset in pixels
    /// </summary>
    public float Y;
}

/// <summary>
/// Image quality mode
/// </summary>
internal enum VelloImageQuality : byte
{
    Low = 0,
    Medium = 1,
    High = 2
}
