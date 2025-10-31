// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Runtime.CompilerServices;
using Vello.Geometry;
using Vello.Native;

namespace Vello;

/// <summary>
/// Provides methods for recording drawing operations.
/// </summary>
/// <remarks>
/// This class is similar to <see cref="RenderContext"/> but records operations
/// instead of executing them immediately.
/// </remarks>
public sealed class Recorder
{
    private readonly nint _handle;

    internal Recorder(nint handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Records a fill rectangle operation.
    /// </summary>
    /// <param name="rect">The rectangle to fill.</param>
    public unsafe void FillRect(in Rect rect)
    {
        ref Rect rectRef = ref Unsafe.AsRef(in rect);
        ref VelloRect native = ref Unsafe.As<Rect, VelloRect>(ref rectRef);
        fixed (VelloRect* ptr = &native)
        {
            NativeMethods.Recorder_FillRect(_handle, ptr);
        }
    }

    /// <summary>
    /// Records a stroke rectangle operation.
    /// </summary>
    /// <param name="rect">The rectangle to stroke.</param>
    public unsafe void StrokeRect(in Rect rect)
    {
        ref Rect rectRef = ref Unsafe.AsRef(in rect);
        ref VelloRect native = ref Unsafe.As<Rect, VelloRect>(ref rectRef);
        fixed (VelloRect* ptr = &native)
        {
            NativeMethods.Recorder_StrokeRect(_handle, ptr);
        }
    }

    /// <summary>
    /// Records a fill path operation.
    /// </summary>
    /// <param name="path">The path to fill.</param>
    public void FillPath(BezPath path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        NativeMethods.Recorder_FillPath(_handle, path.Handle);
    }

    /// <summary>
    /// Records a stroke path operation.
    /// </summary>
    /// <param name="path">The path to stroke.</param>
    public void StrokePath(BezPath path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        NativeMethods.Recorder_StrokePath(_handle, path.Handle);
    }

    /// <summary>
    /// Records a set paint operation with a solid color.
    /// </summary>
    /// <param name="color">The color to set.</param>
    public void SetPaint(Color color)
    {
        NativeMethods.Recorder_SetPaintSolid(_handle, color.R, color.G, color.B, color.A);
    }
}
