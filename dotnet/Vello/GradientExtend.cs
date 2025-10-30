// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

namespace Vello;

/// <summary>
/// Defines how a gradient extends beyond its defined region.
/// </summary>
public enum GradientExtend : byte
{
    /// <summary>
    /// Pad the gradient with the edge colors.
    /// </summary>
    Pad = 0,

    /// <summary>
    /// Repeat the gradient pattern.
    /// </summary>
    Repeat = 1,

    /// <summary>
    /// Reflect the gradient pattern.
    /// </summary>
    Reflect = 2
}
