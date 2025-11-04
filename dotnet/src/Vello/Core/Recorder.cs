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
            VelloException.ThrowIfError(
                NativeMethods.Recorder_FillRect(_handle, ptr));
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
            VelloException.ThrowIfError(
                NativeMethods.Recorder_StrokeRect(_handle, ptr));
        }
    }

    /// <summary>
    /// Records a fill path operation.
    /// </summary>
    /// <param name="path">The path to fill.</param>
    public void FillPath(BezPath path)
    {
        ArgumentNullException.ThrowIfNull(path);
        VelloException.ThrowIfError(
            NativeMethods.Recorder_FillPath(_handle, path.Handle));
    }

    /// <summary>
    /// Records a stroke path operation.
    /// </summary>
    /// <param name="path">The path to stroke.</param>
    public void StrokePath(BezPath path)
    {
        ArgumentNullException.ThrowIfNull(path);
        VelloException.ThrowIfError(
            NativeMethods.Recorder_StrokePath(_handle, path.Handle));
    }

    /// <summary>
    /// Records a set paint operation with a solid color.
    /// </summary>
    /// <param name="color">The color to set.</param>
    public void SetPaint(Color color)
    {
        VelloException.ThrowIfError(
            NativeMethods.Recorder_SetPaintSolid(_handle, color.R, color.G, color.B, color.A));
    }

    /// <summary>
    /// Applies stroke parameters to subsequent stroke operations.
    /// </summary>
    /// <param name="stroke">Stroke parameters to use.</param>
    public unsafe void SetStroke(Stroke stroke)
    {
        var native = stroke.ToNative();
        VelloException.ThrowIfError(
            NativeMethods.Recorder_SetStroke(_handle, &native));
    }

    /// <summary>
    /// Sets the current transform for subsequent drawing operations.
    /// </summary>
    /// <param name="transform">Affine transform to apply.</param>
    public unsafe void SetTransform(in Affine transform)
    {
        ref readonly VelloAffine native = ref Unsafe.As<Affine, VelloAffine>(ref Unsafe.AsRef(in transform));
        fixed (VelloAffine* ptr = &native)
        {
            VelloException.ThrowIfError(
                NativeMethods.Recorder_SetTransform(_handle, ptr));
        }
    }

    /// <summary>
    /// Sets the fill rule used for fill operations.
    /// </summary>
    /// <param name="fillRule">Fill rule to apply.</param>
    public void SetFillRule(FillRule fillRule)
    {
        VelloException.ThrowIfError(
            NativeMethods.Recorder_SetFillRule(_handle, (VelloFillRule)fillRule));
    }

    /// <summary>
    /// Sets the paint-space transform for gradient/image paints.
    /// </summary>
    /// <param name="transform">Affine transform applied to paint space.</param>
    public unsafe void SetPaintTransform(in Affine transform)
    {
        ref readonly VelloAffine native = ref Unsafe.As<Affine, VelloAffine>(ref Unsafe.AsRef(in transform));
        fixed (VelloAffine* ptr = &native)
        {
            VelloException.ThrowIfError(
                NativeMethods.Recorder_SetPaintTransform(_handle, ptr));
        }
    }

    /// <summary>
    /// Resets the paint transform to identity.
    /// </summary>
    public void ResetPaintTransform()
    {
        VelloException.ThrowIfError(
            NativeMethods.Recorder_ResetPaintTransform(_handle));
    }

    /// <summary>
    /// Pushes a clip layer using the specified path.
    /// </summary>
    /// <param name="clipPath">The path defining the clip.</param>
    public void PushClipLayer(BezPath clipPath)
    {
        ArgumentNullException.ThrowIfNull(clipPath);
        VelloException.ThrowIfError(
            NativeMethods.Recorder_PushClipLayer(_handle, clipPath.Handle));
    }

    /// <summary>
    /// Pops the most recently pushed layer.
    /// </summary>
    public void PopLayer()
    {
        VelloException.ThrowIfError(
            NativeMethods.Recorder_PopLayer(_handle));
    }
}
