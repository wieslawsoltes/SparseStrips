// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

namespace Vello;

/// <summary>
/// Image sampling quality hints
/// </summary>
public enum ImageQuality : byte
{
    /// <summary>
    /// Low quality, fastest sampling
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium quality
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High quality, slowest sampling
    /// </summary>
    High = 2
}
