// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using System.Runtime.CompilerServices;

namespace Vello.Benchmarks;

/// <summary>
/// Simple deterministic RNG with no heap allocations, suitable for tight benchmark loops.
/// Uses the same LCG parameters as the reference .NET implementation.
/// </summary>
internal struct DeterministicRng
{
    private uint _state;

    public DeterministicRng(uint seed)
    {
        _state = seed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint NextState()
    {
        _state = unchecked((_state * 1664525u) + 1013904223u);
        return _state;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double NextDouble()
    {
        // Take the high 24 bits and scale to [0, 1)
        return (NextState() >> 8) * (1.0 / (1u << 24));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double NextDouble(double minValue, double maxValue)
    {
        return minValue + (maxValue - minValue) * NextDouble();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int NextInt(int maxExclusive)
    {
        if (maxExclusive <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive));
        }

        return (int)(NextState() % (uint)maxExclusive);
    }
}
