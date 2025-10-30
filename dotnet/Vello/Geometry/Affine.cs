// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.InteropServices;

namespace Vello.Geometry;

/// <summary>
/// Represents a 2D affine transformation.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Affine : IEquatable<Affine>
{
    public readonly double M11, M12, M13;
    public readonly double M21, M22, M23;

    public Affine(double m11, double m12, double m13, double m21, double m22, double m23)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M21 = m21;
        M22 = m22;
        M23 = m23;
    }

    public static Affine Identity => new(1, 0, 0, 0, 1, 0);

    public static Affine Translation(double x, double y) => new(1, 0, x, 0, 1, y);

    public static Affine Scale(double sx, double sy) => new(sx, 0, 0, 0, sy, 0);

    public static Affine Rotation(double angle)
    {
        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);
        return new(cos, -sin, 0, sin, cos, 0);
    }

    public bool Equals(Affine other) =>
        M11 == other.M11 && M12 == other.M12 && M13 == other.M13 &&
        M21 == other.M21 && M22 == other.M22 && M23 == other.M23;

    public override bool Equals(object? obj) => obj is Affine other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(M11, M12, M13, M21, M22, M23);

    public static bool operator ==(Affine left, Affine right) => left.Equals(right);
    public static bool operator !=(Affine left, Affine right) => !left.Equals(right);
}
