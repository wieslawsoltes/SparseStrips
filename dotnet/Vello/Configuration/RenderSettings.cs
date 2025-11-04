// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Native;

namespace Vello;

public enum RenderMode
{
    OptimizeSpeed = 0,
    OptimizeQuality = 1
}

public enum SimdLevel
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
/// Settings for render context.
/// </summary>
public readonly struct RenderSettings
{
    public readonly SimdLevel Level;
    public readonly ushort NumThreads;
    public readonly RenderMode Mode;

    public RenderSettings(
        SimdLevel? level = null,
        ushort? numThreads = null,
        RenderMode mode = RenderMode.OptimizeSpeed)
    {
        Level = level ?? DetectSimdLevel();
        NumThreads = numThreads ?? DetectNumThreads();
        Mode = mode;
    }

    public static SimdLevel DetectSimdLevel()
    {
        return (SimdLevel)NativeMethods.SimdDetect();
    }

    private static ushort DetectNumThreads()
    {
        int count = Environment.ProcessorCount - 1;
        return (ushort)Math.Max(0, Math.Min(count, 8));
    }

    public static RenderSettings Default => new();
    public static RenderSettings SingleThreaded => new(numThreads: 0);

    internal VelloRenderSettings ToNative() => new()
    {
        Level = (VelloSimdLevel)Level,
        NumThreads = NumThreads,
        RenderMode = (VelloRenderMode)Mode
    };
}
