// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

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
    public unsafe void FillRect(Rect rect)
    {
        var velloRect = new VelloRect
        {
            X0 = rect.X0,
            Y0 = rect.Y0,
            X1 = rect.X1,
            Y1 = rect.Y1
        };
        NativeMethods.Recorder_FillRect(_handle, &velloRect);
    }

    /// <summary>
    /// Records a stroke rectangle operation.
    /// </summary>
    /// <param name="rect">The rectangle to stroke.</param>
    public unsafe void StrokeRect(Rect rect)
    {
        var velloRect = new VelloRect
        {
            X0 = rect.X0,
            Y0 = rect.Y0,
            X1 = rect.X1,
            Y1 = rect.Y1
        };
        NativeMethods.Recorder_StrokeRect(_handle, &velloRect);
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
