// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Native;

namespace Vello.Geometry;

public enum Join
{
    Bevel = 0,
    Miter = 1,
    Round = 2
}

public enum Cap
{
    Butt = 0,
    Square = 1,
    Round = 2
}

/// <summary>
/// Represents stroke parameters.
/// </summary>
public readonly struct Stroke
{
    public readonly float Width;
    public readonly float MiterLimit;
    public readonly Join Join;
    public readonly Cap StartCap;
    public readonly Cap EndCap;

    public Stroke(
        float width = 1.0f,
        Join join = Join.Bevel,
        Cap startCap = Cap.Butt,
        Cap endCap = Cap.Butt,
        float miterLimit = 4.0f)
    {
        Width = width;
        Join = join;
        StartCap = startCap;
        EndCap = endCap;
        MiterLimit = miterLimit;
    }

    internal VelloStroke ToNative() => new()
    {
        Width = Width,
        MiterLimit = MiterLimit,
        Join = (VelloJoin)Join,
        StartCap = (VelloCap)StartCap,
        EndCap = (VelloCap)EndCap
    };
}
