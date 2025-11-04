// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.InteropServices;

namespace Vello.Geometry;

/// <summary>
/// Represents a rectangle.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Rect : IEquatable<Rect>
{
    public readonly double X0;
    public readonly double Y0;
    public readonly double X1;
    public readonly double Y1;

    public Rect(double x0, double y0, double x1, double y1)
    {
        X0 = x0;
        Y0 = y0;
        X1 = x1;
        Y1 = y1;
    }

    public double Width => X1 - X0;
    public double Height => Y1 - Y0;
    public Point TopLeft => new(X0, Y0);
    public Point BottomRight => new(X1, Y1);

    public static Rect FromXYWH(double x, double y, double width, double height)
        => new(x, y, x + width, y + height);

    public bool Equals(Rect other) => X0 == other.X0 && Y0 == other.Y0 && X1 == other.X1 && Y1 == other.Y1;
    public override bool Equals(object? obj) => obj is Rect other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X0, Y0, X1, Y1);
    public override string ToString() => $"Rect({X0}, {Y0}, {X1}, {Y1})";

    public static bool operator ==(Rect left, Rect right) => left.Equals(right);
    public static bool operator !=(Rect left, Rect right) => !left.Equals(right);
}
