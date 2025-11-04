// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

namespace Vello;

/// <summary>
/// Represents the kind of paint (for querying paint type).
/// </summary>
public enum PaintKind : byte
{
    /// <summary>
    /// Solid color paint.
    /// </summary>
    Solid = 0,

    /// <summary>
    /// Linear gradient paint.
    /// </summary>
    LinearGradient = 1,

    /// <summary>
    /// Radial gradient paint.
    /// </summary>
    RadialGradient = 2,

    /// <summary>
    /// Sweep (angular) gradient paint.
    /// </summary>
    SweepGradient = 3,

    /// <summary>
    /// Image-based paint.
    /// </summary>
    Image = 4
}
