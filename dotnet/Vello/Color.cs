// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.InteropServices;

namespace Vello;

/// <summary>
/// Represents a premultiplied RGBA8 color.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct PremulRgba8 : IEquatable<PremulRgba8>
{
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly byte A;

    public PremulRgba8(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public bool Equals(PremulRgba8 other) => R == other.R && G == other.G && B == other.B && A == other.A;
    public override bool Equals(object? obj) => obj is PremulRgba8 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);
    public override string ToString() => $"PremulRgba8({R}, {G}, {B}, {A})";

    public static bool operator ==(PremulRgba8 left, PremulRgba8 right) => left.Equals(right);
    public static bool operator !=(PremulRgba8 left, PremulRgba8 right) => !left.Equals(right);
}

/// <summary>
/// Represents an RGBA color (non-premultiplied).
/// </summary>
public readonly struct Color
{
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly byte A;

    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public PremulRgba8 Premultiply()
    {
        if (A == 255)
            return new PremulRgba8(R, G, B, A);

        float alpha = A / 255f;
        return new PremulRgba8(
            (byte)(R * alpha),
            (byte)(G * alpha),
            (byte)(B * alpha),
            A
        );
    }

    // Common colors
    public static Color Black => new(0, 0, 0);
    public static Color White => new(255, 255, 255);
    public static Color Red => new(255, 0, 0);
    public static Color Green => new(0, 255, 0);
    public static Color Blue => new(0, 0, 255);
    public static Color Magenta => new(255, 0, 255);
    public static Color Cyan => new(0, 255, 255);
    public static Color Yellow => new(255, 255, 0);
    public static Color Transparent => new(0, 0, 0, 0);
}
