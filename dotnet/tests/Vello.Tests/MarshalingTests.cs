// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.InteropServices;
using Vello.Native;

namespace Vello.Tests;

/// <summary>
/// Tests to verify correct FFI marshaling between C# and Rust for all data structures.
/// These tests use echo functions in Rust that return the input unchanged.
/// </summary>
public class MarshalingTests
{
    private const string LibName = "vello_cpu_ffi";

    #region P/Invoke Declarations

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int vello_test_echo_render_settings(
        ref VelloRenderSettings input,
        out VelloRenderSettings output);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int vello_test_echo_stroke(
        ref VelloStroke input,
        out VelloStroke output);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int vello_test_echo_blend_mode(
        ref VelloBlendMode input,
        out VelloBlendMode output);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int vello_test_echo_color_stop(
        ref VelloColorStop input,
        out VelloColorStop output);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int vello_test_echo_point(
        ref VelloPoint input,
        out VelloPoint output);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int vello_test_echo_rect(
        ref VelloRect input,
        out VelloRect output);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int vello_test_echo_affine(
        ref VelloAffine input,
        out VelloAffine output);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int vello_test_echo_color(
        ref VelloPremulRgba8 input,
        out VelloPremulRgba8 output);

    #endregion

    [Fact]
    public void VelloRenderSettings_MarshalingCorrect()
    {
        var input = new VelloRenderSettings
        {
            Level = VelloSimdLevel.Avx2,
            NumThreads = 8,
            RenderMode = VelloRenderMode.OptimizeQuality
        };

        var result = vello_test_echo_render_settings(ref input, out var output);

        Assert.Equal(0, result); // VELLO_OK
        Assert.Equal(input.Level, output.Level);
        Assert.Equal(input.NumThreads, output.NumThreads);
        Assert.Equal(input.RenderMode, output.RenderMode);
    }

    [Fact]
    public void VelloStroke_MarshalingCorrect()
    {
        var input = new VelloStroke
        {
            Width = 2.5f,
            MiterLimit = 4.0f,
            Join = VelloJoin.Round,
            StartCap = VelloCap.Square,
            EndCap = VelloCap.Round
        };

        var result = vello_test_echo_stroke(ref input, out var output);

        Assert.Equal(0, result);
        Assert.Equal(input.Width, output.Width);
        Assert.Equal(input.MiterLimit, output.MiterLimit);
        Assert.Equal(input.Join, output.Join);
        Assert.Equal(input.StartCap, output.StartCap);
        Assert.Equal(input.EndCap, output.EndCap);
    }

    [Fact]
    public void VelloBlendMode_MarshalingCorrect()
    {
        var input = new VelloBlendMode
        {
            Mix = VelloMix.Multiply,
            Compose = VelloCompose.SrcOver
        };

        var result = vello_test_echo_blend_mode(ref input, out var output);

        Assert.Equal(0, result);
        Assert.Equal(input.Mix, output.Mix);
        Assert.Equal(input.Compose, output.Compose);
    }

    [Fact]
    public void VelloColorStop_MarshalingCorrect()
    {
        var input = new VelloColorStop
        {
            Offset = 0.75f,
            R = 128,
            G = 200,
            B = 64,
            A = 255
        };

        var result = vello_test_echo_color_stop(ref input, out var output);

        Assert.Equal(0, result);
        Assert.Equal(input.Offset, output.Offset);
        Assert.Equal(input.R, output.R);
        Assert.Equal(input.G, output.G);
        Assert.Equal(input.B, output.B);
        Assert.Equal(input.A, output.A);
    }

    [Fact]
    public void VelloPoint_MarshalingCorrect()
    {
        var input = new VelloPoint
        {
            X = 123.456,
            Y = 789.012
        };

        var result = vello_test_echo_point(ref input, out var output);

        Assert.Equal(0, result);
        Assert.Equal(input.X, output.X);
        Assert.Equal(input.Y, output.Y);
    }

    [Fact]
    public void VelloRect_MarshalingCorrect()
    {
        var input = new VelloRect
        {
            X0 = 10.5,
            Y0 = 20.5,
            X1 = 100.5,
            Y1 = 200.5
        };

        var result = vello_test_echo_rect(ref input, out var output);

        Assert.Equal(0, result);
        Assert.Equal(input.X0, output.X0);
        Assert.Equal(input.Y0, output.Y0);
        Assert.Equal(input.X1, output.X1);
        Assert.Equal(input.Y1, output.Y1);
    }

    [Fact]
    public void VelloAffine_MarshalingCorrect()
    {
        var input = new VelloAffine
        {
            M11 = 1.0,
            M12 = 0.0,
            M13 = 0.0,
            M21 = 0.0,
            M22 = 1.0,
            M23 = 0.0
        };

        var result = vello_test_echo_affine(ref input, out var output);

        Assert.Equal(0, result);
        Assert.Equal(input.M11, output.M11);
        Assert.Equal(input.M12, output.M12);
        Assert.Equal(input.M13, output.M13);
        Assert.Equal(input.M21, output.M21);
        Assert.Equal(input.M22, output.M22);
        Assert.Equal(input.M23, output.M23);
    }

    [Fact]
    public void VelloPremulRgba8_MarshalingCorrect()
    {
        var input = new VelloPremulRgba8
        {
            R = 100,
            G = 150,
            B = 200,
            A = 255
        };

        var result = vello_test_echo_color(ref input, out var output);

        Assert.Equal(0, result);
        Assert.Equal(input.R, output.R);
        Assert.Equal(input.G, output.G);
        Assert.Equal(input.B, output.B);
        Assert.Equal(input.A, output.A);
    }

    [Fact]
    public void VelloStroke_AllJoinTypes_MarshalCorrectly()
    {
        foreach (VelloJoin join in Enum.GetValues<VelloJoin>())
        {
            var input = new VelloStroke
            {
                Width = 1.0f,
                MiterLimit = 4.0f,
                Join = join,
                StartCap = VelloCap.Butt,
                EndCap = VelloCap.Butt
            };

            var result = vello_test_echo_stroke(ref input, out var output);

            Assert.Equal(0, result);
            Assert.Equal(join, output.Join);
        }
    }

    [Fact]
    public void VelloStroke_AllCapTypes_MarshalCorrectly()
    {
        foreach (VelloCap cap in Enum.GetValues<VelloCap>())
        {
            var input = new VelloStroke
            {
                Width = 1.0f,
                MiterLimit = 4.0f,
                Join = VelloJoin.Bevel,
                StartCap = cap,
                EndCap = cap
            };

            var result = vello_test_echo_stroke(ref input, out var output);

            Assert.Equal(0, result);
            Assert.Equal(cap, output.StartCap);
            Assert.Equal(cap, output.EndCap);
        }
    }

    [Fact]
    public void VelloBlendMode_AllMixModes_MarshalCorrectly()
    {
        foreach (VelloMix mix in Enum.GetValues<VelloMix>())
        {
            var input = new VelloBlendMode
            {
                Mix = mix,
                Compose = VelloCompose.SrcOver
            };

            var result = vello_test_echo_blend_mode(ref input, out var output);

            Assert.Equal(0, result);
            Assert.Equal(mix, output.Mix);
        }
    }

    [Fact]
    public void VelloBlendMode_AllComposeModes_MarshalCorrectly()
    {
        foreach (VelloCompose compose in Enum.GetValues<VelloCompose>())
        {
            var input = new VelloBlendMode
            {
                Mix = VelloMix.Normal,
                Compose = compose
            };

            var result = vello_test_echo_blend_mode(ref input, out var output);

            Assert.Equal(0, result);
            Assert.Equal(compose, output.Compose);
        }
    }

    [Fact]
    public void VelloRenderSettings_AllSimdLevels_MarshalCorrectly()
    {
        foreach (VelloSimdLevel level in Enum.GetValues<VelloSimdLevel>())
        {
            var input = new VelloRenderSettings
            {
                Level = level,
                NumThreads = 1,
                RenderMode = VelloRenderMode.OptimizeSpeed
            };

            var result = vello_test_echo_render_settings(ref input, out var output);

            Assert.Equal(0, result);
            Assert.Equal(level, output.Level);
        }
    }

    [Fact]
    public void VelloRenderSettings_AllRenderModes_MarshalCorrectly()
    {
        foreach (VelloRenderMode mode in Enum.GetValues<VelloRenderMode>())
        {
            var input = new VelloRenderSettings
            {
                Level = VelloSimdLevel.Fallback,
                NumThreads = 1,
                RenderMode = mode
            };

            var result = vello_test_echo_render_settings(ref input, out var output);

            Assert.Equal(0, result);
            Assert.Equal(mode, output.RenderMode);
        }
    }

    [Fact]
    public void VelloRenderSettings_EdgeCases_MarshalCorrectly()
    {
        // Test with 0 threads (should use all cores)
        var input1 = new VelloRenderSettings
        {
            Level = VelloSimdLevel.Avx2,
            NumThreads = 0,
            RenderMode = VelloRenderMode.OptimizeSpeed
        };

        var result1 = vello_test_echo_render_settings(ref input1, out var output1);
        Assert.Equal(0, result1);
        Assert.Equal((ushort)0, output1.NumThreads);

        // Test with max threads
        var input2 = new VelloRenderSettings
        {
            Level = VelloSimdLevel.Neon,
            NumThreads = ushort.MaxValue,
            RenderMode = VelloRenderMode.OptimizeQuality
        };

        var result2 = vello_test_echo_render_settings(ref input2, out var output2);
        Assert.Equal(0, result2);
        Assert.Equal(ushort.MaxValue, output2.NumThreads);
    }

    [Fact]
    public void VelloColorStop_EdgeValues_MarshalCorrectly()
    {
        // Test with offset at boundaries
        var input1 = new VelloColorStop { Offset = 0.0f, R = 0, G = 0, B = 0, A = 0 };
        var result1 = vello_test_echo_color_stop(ref input1, out var output1);
        Assert.Equal(0, result1);
        Assert.Equal(0.0f, output1.Offset);

        var input2 = new VelloColorStop { Offset = 1.0f, R = 255, G = 255, B = 255, A = 255 };
        var result2 = vello_test_echo_color_stop(ref input2, out var output2);
        Assert.Equal(0, result2);
        Assert.Equal(1.0f, output2.Offset);
        Assert.Equal((byte)255, output2.R);
        Assert.Equal((byte)255, output2.G);
        Assert.Equal((byte)255, output2.B);
        Assert.Equal((byte)255, output2.A);
    }
}
