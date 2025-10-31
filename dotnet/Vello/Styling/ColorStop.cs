// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.InteropServices;

namespace Vello;

/// <summary>
/// Represents a color stop in a gradient, consisting of an offset and raw RGBA values.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ColorStop : IEquatable<ColorStop>
{
    public readonly float Offset;
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly byte A;

    /// <summary>
    /// Creates a color stop with the specified offset and color.
    /// </summary>
    public ColorStop(float offset, Color color)
    {
        Offset = offset;
        R = color.R;
        G = color.G;
        B = color.B;
        A = color.A;
    }

    /// <summary>
    /// Creates a color stop with the specified offset and RGBA values.
    /// </summary>
    public ColorStop(float offset, byte r, byte g, byte b, byte a = 255)
    {
        Offset = offset;
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>
    /// Gets the color represented by this stop.
    /// </summary>
    public Color Color => new(R, G, B, A);

    public void Deconstruct(out float offset, out Color color)
    {
        offset = Offset;
        color = Color;
    }

    public bool Equals(ColorStop other) =>
        Offset == other.Offset &&
        R == other.R &&
        G == other.G &&
        B == other.B &&
        A == other.A;

    public override bool Equals(object? obj) => obj is ColorStop other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Offset, R, G, B, A);
    public override string ToString() => $"ColorStop(Offset: {Offset}, Color: {Color})";

    public static bool operator ==(ColorStop left, ColorStop right) => left.Equals(right);
    public static bool operator !=(ColorStop left, ColorStop right) => !left.Equals(right);
}
